using Maxst.Settings;
using Maxst.Token;
using System;
using System.Collections;
using UniRx;
using UnityEngine;

namespace Maxst.Passport
{
    public class Token
    {
        public string idToken;
        public string accessToken;
        public string refreshToken;
    }

    public enum TokenStatus
    {
        Validate,
        Expired,
        Renewing,
    }

    public class TokenRepo : Singleton<TokenRepo>
    {
        private const long DEFAULT_EFFECTIVE_TIME = 300;
        private const long ESTIMATED_EXPIRATION_TIME = 30;
        private const string IdTokenKey = "MaxstSSO_IdToken";
        private const string AccessTokenKey = "MaxstSSO_AccessToken";
        private const string RefreshTokenKey = "MaxstSSO_RefreshToken";
        private Token token;
        private JwtTokenBody jwtTokenBody;
        private Coroutine refreshTokenCoroutine;

        private string IdToken => token?.idToken ?? string.Empty;
        private string BearerAccessToken => string.IsNullOrEmpty(token?.accessToken) ? "" : "Bearer " + token.accessToken;
        private string RefreshToken => token?.refreshToken ?? "";

        public ReactiveProperty<TokenStatus> tokenStatus = new(TokenStatus.Expired);

        [RuntimeInitializeOnLoadMethod]
        public static void TokenRepoOnLoad()
        {
            TokenRepo.Instance.RestoreToken();
        }

        public Token GetToken()
        {
            return token;
        }
        public JwtTokenBody GetJwtTokenBody()
        {
            return jwtTokenBody;
        }

        public void Config(Token token)
        {
            this.token = token;
            StoreToken(token);
            if (token != null)
            {
                jwtTokenBody = JwtTokenParser.BodyDecode(token.accessToken);
                jwtTokenBody.exp = jwtTokenBody.exp > DEFAULT_EFFECTIVE_TIME ? jwtTokenBody.exp : CurrentTimeSeconds() + DEFAULT_EFFECTIVE_TIME;
                //force test code
                //jwtTokenBody.exp = CurrentTimeSeconds() + ESTIMATED_EXPIRATION_TIME + 5;
                tokenStatus.Value = TokenStatus.Validate;
                //StartRefreshTokenCoroutine();
            }
            else
            {
                StopRefreshTokenCoroutine();
            }
        }

        public IEnumerator GetPassportToken(
            OpenIDConnectArguments OpenIDConnectArguments, string code, string CodeVerifier,
            System.Action<TokenStatus, Token> callback,
            Action<ErrorCode, Exception> LoginFailAction
            )
        {
            yield return new WaitUntil(() => tokenStatus.Value != TokenStatus.Renewing);
            yield return FetchPassportToken(OpenIDConnectArguments, code, CodeVerifier, LoginFailAction);
            
            callback?.Invoke(tokenStatus.Value, token);
        }

        public IEnumerator GetPassportRefreshToken(OpenIDConnectArguments OpenIDConnectArguments, 
            System.Action<TokenStatus, Token> callback,
             Action<Exception> RefreshFailAction)
        {
            yield return new WaitUntil(() => tokenStatus.Value != TokenStatus.Renewing);
            if (IsTokenExpired())
            {
                OpenIDConnectArguments.TryGetValue(OpenIDConnectArgument.ClientID, out var ClientID);
                string grant_type = "refresh_token";

                Debug.Log($"GetPassportRefreshToken : {RefreshToken}");
                
                yield return FetchPassportRefreshToken(ClientID, grant_type, RefreshToken, RefreshFailAction);
            }
            callback?.Invoke(tokenStatus.Value, token);
        }

        private void StopRefreshTokenCoroutine()
        {
            if (refreshTokenCoroutine != null)
            {
                StopCoroutine(refreshTokenCoroutine);
                refreshTokenCoroutine = null;
            }
        }

        private long MeasureRemainTimeSeconds()
        {
            return jwtTokenBody?.exp - CurrentTimeSeconds() ?? 0;
        }

        private bool IsTokenExpired()
        {
            return MeasureRemainTimeSeconds() < ESTIMATED_EXPIRATION_TIME;
        }

        private long CurrentTimeSeconds()
        {
            return (long)(System.DateTime.UtcNow - new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds;
        }

        public void PassportLogout(OpenIDConnectArguments OpenIDConnectArguments, System.Action success = null, System.Action<System.Exception> fail = null) {
            StopRefreshTokenCoroutine();
            System.IObservable<System.Object> ob = null;

            OpenIDConnectArguments.TryGetValue(OpenIDConnectArgument.ClientID, out var ClientID);

            ob = AuthService.Instance.PassportLogout(BearerAccessToken, ClientID, RefreshToken, IdToken);

            ob.SubscribeOn(Scheduler.MainThreadEndOfFrame)
                .ObserveOn(Scheduler.MainThread)
                .Subscribe(data =>   // on success
                {
                    Debug.Log($"[SessionLogout] : {data}");
                },
                error => // on error
                {
                    Config(null);
                    Debug.Log($"[SessionLogout] error {error}");
                    fail?.Invoke(error);
                },
                () =>
                {
                    Config(null);
                    Debug.Log("[SessionLogout] success");
                    success?.Invoke();
                });
        }

        private IEnumerator FetchPassportRefreshToken(string clientId, string grantType, string refreshToken,
            Action<Exception> RefreshFailAction) {
            System.IObservable<CredentialsToken> ob = AuthService.Instance.PassportRefreshToken(clientId, grantType, refreshToken); 

            tokenStatus.Value = TokenStatus.Renewing;

            var disposable = ob.SubscribeOn(Scheduler.MainThreadEndOfFrame)
                .ObserveOn(Scheduler.MainThread)
                .Subscribe(data =>   // on success
                {
                    Debug.Log("[FetchPassportRefreshToken] : " + data);
                    if (data != null)
                    {
                        tokenStatus.Value = TokenStatus.Validate;
                        Config(new Token
                        {
                            idToken = data.id_token,
                            accessToken = data.access_token,
                            refreshToken = data.refresh_token,
                        });
                    }
                    else
                    {
                        tokenStatus.Value = TokenStatus.Expired;
                        RefreshFailAction.Invoke(null);
                    }
                },
                error => // on error
                {
                    Debug.LogWarning($"[FetchPassportRefreshToken] error : {error}");
                    tokenStatus.Value = TokenStatus.Expired;
                    RefreshFailAction.Invoke(error);
                },
                () =>
                {
                    //Debug.Log("FetchRefreshToken complte : ");
                });
            yield return new WaitUntil(() => tokenStatus.Value != TokenStatus.Renewing);
            disposable.Dispose();
        }

        private IEnumerator FetchPassportToken(
            OpenIDConnectArguments OpenIDConnectArguments, string Code, string CodeVerifier,
            Action<ErrorCode, Exception> LoginFailAction
        )
        {
            tokenStatus.Value = TokenStatus.Renewing;

            System.IObservable<CredentialsToken> ob = null;

            var Setting = EnvAdmin.Instance.OpenIDConnectSetting;
            Setting.TryGetValue(OpenIDConnectSettingKey.GrantType, out var GrantType);
            
            OpenIDConnectArguments.TryGetValue(OpenIDConnectArgument.ClientID, out var ClientID);

#if UNITY_ANDROID
            OpenIDConnectArguments.TryGetValue(OpenIDConnectArgument.AndroidRedirectUri, out var RedirectURI);
#elif UNITY_IOS
            OpenIDConnectArguments.TryGetValue(OpenIDConnectArgument.iOSRedirectUri, out var RedirectURI);
#else
            OpenIDConnectArguments.TryGetValue(OpenIDConnectArgument.WebRedirectUri, out var RedirectURI);
#endif

            Debug.Log($"[FetchToken] ClientID : {ClientID}");
            Debug.Log($"[FetchToken] CodeVerifier : {CodeVerifier}");
            Debug.Log($"[FetchToken] GrantType : {GrantType}");
            Debug.Log($"[FetchToken] RedirectURI : {RedirectURI}");
            Debug.Log($"[FetchToken] code : {Code}");

            ob = AuthService.Instance.PassportToken(ClientID, CodeVerifier, GrantType, RedirectURI, Code);

            var disposable = ob.SubscribeOn(Scheduler.MainThreadEndOfFrame)
                    .ObserveOn(Scheduler.MainThread)
                    .Subscribe(data =>   // on success
                    {
                        Debug.Log("[FetchToken] FetchToken : " + data);
                        if (data != null)
                        {
                            var idToken = data.id_token;
                            var accessToken = data.access_token;
                            var refreshToken = data.refresh_token;

                            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshToken))
                            {
                                LoginFailAction?.Invoke(ErrorCode.TOKEN_IS_EMPTY, null);
                                tokenStatus.Value = TokenStatus.Expired;
                                Config(null);
                            }
                            else
                            {
                                tokenStatus.Value = TokenStatus.Validate;
                                Config(new Token
                                {
                                    idToken = data.id_token,
                                    accessToken = data.access_token,
                                    refreshToken = data.refresh_token,
                                });
                            }
                        }
                        else
                        {
                            tokenStatus.Value = TokenStatus.Expired;
                        }
                    },
                    error => // on error
                    {
                        Debug.LogWarning($"[FetchToken] FetchToken error : {error}");
                        tokenStatus.Value = TokenStatus.Expired;
                        LoginFailAction?.Invoke(ErrorCode.TOKEN_IS_EMPTY, error);
                    },
                    () =>
                    {
                        Debug.Log("[FetchToken] FetchToken complte : ");
                    });

            yield return new WaitUntil(() => tokenStatus.Value != TokenStatus.Renewing);
            disposable.Dispose();
        }

        private void StoreToken(Token token = null)
        {
            PlayerPrefs.SetString(IdTokenKey, token?.idToken ?? "");
            PlayerPrefs.SetString(AccessTokenKey, token?.accessToken ?? "");
            PlayerPrefs.SetString(RefreshTokenKey, token?.refreshToken ?? "");
        }

        private void RestoreToken()
        {
            var idToken = PlayerPrefs.GetString(IdTokenKey, "");
            var accessToken = PlayerPrefs.GetString(AccessTokenKey, "");
            var refreshToken = PlayerPrefs.GetString(RefreshTokenKey, "");
            if (string.IsNullOrEmpty(accessToken)
                || string.IsNullOrEmpty(refreshToken))
            {
                Config(null);
            }
            else
            {
                Config(new Token
                {
                    idToken = idToken,
                    accessToken = accessToken,
                    refreshToken = refreshToken,
                });
            }
        }
    }
}
