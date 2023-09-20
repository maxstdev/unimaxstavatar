using Maxst.Settings;
using Maxst.Token;
using System;
using System.Collections;
using UniRx;
using UnityEngine;
using static Maxst.Token.JwtTokenParser;

namespace Maxst.Passport
{
    public class Token
    {
        public string idToken;
        public string accessToken;
        public string refreshToken;

        public TokenDictionary accessTokenDictionary;
        public TokenDictionary idTokenDictionary;
        public TokenDictionary refreshTokenDictionary;
    }

    public enum TokenStatus
    {
        Validate,
        Expired,
        Renewing,
    }

    public class TokenRepo : MaxstUtils.Singleton<TokenRepo>
    {
        private const long DEFAULT_EFFECTIVE_TIME = 300;
        private const long ESTIMATED_EXPIRATION_TIME = 30;

        private const string ClientTokenKey = "Passport_ClientToken";

        private const string IdTokenKey = "Passport_IdToken";
        private const string AccessTokenKey = "Passport_AccessToken";
        private const string RefreshTokenKey = "Passport_RefreshToken";

        private const string GrantType = "refresh_token";

        private ClientToken clientToken;
        private TokenDictionary clientTokenDictionary;

        private Token token;

        private Coroutine refreshTokenCoroutine;

        private string IdToken => token?.idToken ?? string.Empty;
        private string BearerAccessToken => string.IsNullOrEmpty(token?.accessToken) ? "" : "Bearer " + token.accessToken;
        private string RefreshToken => token?.refreshToken ?? "";

        public ReactiveProperty<TokenStatus> tokenStatus = new(TokenStatus.Expired);
        public ReactiveProperty<TokenStatus> clientTokenStatus = new(TokenStatus.Expired);
        public string ClientID { get; set; } = string.Empty;


        [RuntimeInitializeOnLoadMethod]
        public static void TokenRepoOnLoad()
        {
            TokenRepo.Instance.RestoreToken();
        }

        public Token GetToken()
        {
            return token;
        }

        public ClientToken GetClientToken()
        {
            return clientToken;
        }

        public TokenDictionary GetClinetTokenDictionary()
        {
            return clientTokenDictionary;
        }

        public void ClientTokenConfig(ClientToken token)
        {
            this.clientToken = token;
            StoreClientToken(token);
            if (token != null)
            {
                clientTokenDictionary = new TokenDictionary(BodyDecodeDictionary(token.access_token, DecodingType.BASE64_URL_SAFE));
                long exp = clientTokenDictionary.GetTypedValue<long>(JwtTokenConstants.exp);
                exp = exp > DEFAULT_EFFECTIVE_TIME ?
                    exp : CurrentTimeSeconds() + DEFAULT_EFFECTIVE_TIME;

                clientTokenStatus.Value = TokenStatus.Validate;
            }
            else
            {
                clientTokenStatus.Value = TokenStatus.Expired;
            }
        }

        public void Config(Token token)
        {
            this.token = token;
            StoreToken(token);
            if (token != null)
            {
                token.accessTokenDictionary = new TokenDictionary(BodyDecodeDictionary(token.accessToken, DecodingType.BASE64_URL_SAFE));
                token.idTokenDictionary = new TokenDictionary(BodyDecodeDictionary(token.idToken, DecodingType.BASE64_URL_SAFE));
                token.refreshTokenDictionary = new TokenDictionary(BodyDecodeDictionary(token.refreshToken, DecodingType.BASE64_URL_SAFE));
                
                long exp = token.accessTokenDictionary.GetTypedValue<long>(JwtTokenConstants.exp);
                exp = exp > DEFAULT_EFFECTIVE_TIME ?
                    exp : CurrentTimeSeconds() + DEFAULT_EFFECTIVE_TIME;

                //force test code
                //jwtTokenBody.exp = CurrentTimeSeconds() + ESTIMATED_EXPIRATION_TIME + 5;
                tokenStatus.Value = TokenStatus.Validate;
#if MAXST_TOKEN_AUTO_REFRESH
                StartRefreshTokenCoroutine();
#endif
            }
            else
            {
                tokenStatus.Value = TokenStatus.Expired;
                StopRefreshTokenCoroutine();
            }
        }

        private bool IsTokenNotRenewing()
        {
            return tokenStatus.Value != TokenStatus.Renewing;
        }

        private bool IsClientTokenNotRenewing()
        {
            return clientTokenStatus.Value != TokenStatus.Renewing;
        }

        [Obsolete]
        public IEnumerator GetPassportClientToken(
            string applicationId, string applicationKey, string grantType,
            Action<TokenStatus, ClientToken> callback = null,
            Action<ErrorCode, Exception> LoginFailAction = null,
            bool isForcedRefresh = true)
        {

            if (isForcedRefresh || (clientToken == null || ClientIsTokenExpired()))
            {
                yield return new WaitUntil(() => IsClientTokenNotRenewing());
                yield return FetchPassportClientToken(applicationId, applicationKey, grantType, LoginFailAction);
            }

            callback?.Invoke(clientTokenStatus.Value, clientToken);
        }

        public IEnumerator GetPassportClientTokenWithRealm(
            string realm, string applicationId, string applicationKey, string grantType,
            Action<TokenStatus, ClientToken> callback = null,
            Action<ErrorCode, Exception> LoginFailAction = null,
            bool isForcedRefresh = true
            )
        {
            if (isForcedRefresh || (clientToken == null || ClientIsTokenExpired()))
            {
                yield return new WaitUntil(() => IsClientTokenNotRenewing());
                yield return FetchPassportClientTokenWithRealm(realm, applicationId, applicationKey, grantType, LoginFailAction);
            }
            callback?.Invoke(clientTokenStatus.Value, clientToken);
        }

        public IEnumerator GetPublicPassportToken(
            OpenIDConnectArguments OpenIDConnectArguments, string code, string CodeVerifier,
            System.Action<TokenStatus, Token> callback,
            Action<ErrorCode, Exception> LoginFailAction,
            bool isForcedRefresh = true)
        {
            if (isForcedRefresh || (token == null || IsTokenExpired()))
            {
                yield return new WaitUntil(() => IsTokenNotRenewing());
                yield return FetchPublicPassportToken(OpenIDConnectArguments, code, CodeVerifier, LoginFailAction);
            }

            callback?.Invoke(tokenStatus.Value, token);
        }

        public IEnumerator GetConfidentialPassportToken(
            OpenIDConnectArguments OpenIDConnectArguments, string code, string ClientSecret,
            System.Action<TokenStatus, Token> callback,
            Action<ErrorCode, Exception> LoginFailAction
            )
        {
            yield return new WaitUntil(() => IsTokenNotRenewing());
            yield return FetchConfidentialPassportToken(OpenIDConnectArguments, code, ClientSecret, LoginFailAction);

            callback?.Invoke(tokenStatus.Value, token);
        }

        public IEnumerator GetPassportRefreshToken(System.Action<TokenStatus, Token> callback,
             Action<Exception> RefreshFailAction)
        {
            yield return new WaitUntil(() => IsTokenNotRenewing());
            if (IsTokenExpired())
            {
                //Debug.Log($"GetPassportRefreshToken : {RefreshToken}");
                yield return FetchPassportRefreshToken(ClientID, GrantType, RefreshToken, RefreshFailAction);
            }
            callback?.Invoke(tokenStatus.Value, token);
        }

        private void StartRefreshTokenCoroutine()
        {
            StopRefreshTokenCoroutine();
            refreshTokenCoroutine = StartCoroutine(RefreshTokenRoutine());
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
            var dict = token.accessTokenDictionary;
            return dict != null && dict.GetTokenDictionary().ContainsKey(JwtTokenConstants.exp) ?
                dict.GetTypedValue<long>(JwtTokenConstants.exp) - CurrentTimeSeconds(): 0;
        }

        public bool IsTokenExpired()
        {
            return MeasureRemainTimeSeconds() < ESTIMATED_EXPIRATION_TIME;
        }

        private long ClinetTokenMeasureRemainTimeSeconds()
        {
            var dict = clientTokenDictionary;
            return dict != null && dict.GetTokenDictionary().ContainsKey(JwtTokenConstants.exp) ?
                dict.GetTypedValue<long>(JwtTokenConstants.exp) - CurrentTimeSeconds() : 0;
        }

        public bool ClientIsTokenExpired()
        {
            return ClinetTokenMeasureRemainTimeSeconds() < ESTIMATED_EXPIRATION_TIME;
        }

        private long CurrentTimeSeconds()
        {
            return (long)(System.DateTime.UtcNow - new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds;
        }

        private IEnumerator RefreshTokenRoutine()
        {
            while (true)
            {
                if (IsTokenExpired())
                {
                    yield return FetchPassportRefreshToken(ClientID, GrantType, RefreshToken
                        , e =>
                        {

                        });
                    if (tokenStatus.Value != TokenStatus.Validate)
                    {
                        yield return new WaitForSeconds(5);
                    }
                }
                else
                {
                    var time = MeasureRemainTimeSeconds();
                    yield return new WaitForSeconds(System.Math.Max(time / 2, 5));
                }
            }
        }

        public void ConfidentialPassportLogout(string clientSecret, System.Action success = null, System.Action<System.Exception> fail = null)
        {
            StopRefreshTokenCoroutine();
            System.IObservable<System.Object> ob = null;

            ob = AuthService.Instance.ConfidentialPassportLogout(BearerAccessToken, ClientID, RefreshToken, clientSecret);
            LogoutSubscribeOn(ob, success, fail);
        }

        public void PublicPassportLogout(System.Action success = null, System.Action<System.Exception> fail = null)
        {
            StopRefreshTokenCoroutine();
            System.IObservable<System.Object> ob = null;

            ob = AuthService.Instance.PublicPassportLogout(BearerAccessToken, ClientID, RefreshToken, IdToken);
            LogoutSubscribeOn(ob, success, fail);
        }

        private void LogoutSubscribeOn(IObservable<System.Object> ob, Action success, Action<Exception> fail)
        {
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
            Action<Exception> RefreshFailAction)
        {
            System.IObservable<CredentialsToken> ob = AuthService.Instance.PassportRefreshToken(clientId, grantType, refreshToken);

            tokenStatus.Value = TokenStatus.Renewing;

            var disposable = ob.SubscribeOn(Scheduler.MainThreadEndOfFrame)
                .ObserveOn(Scheduler.MainThread)
                .OnErrorRetry((Exception ex) => Debug.Log(ex), retryCount: 3, TimeSpan.FromSeconds(1))
                .Subscribe(data =>   // on success
                {
                    Debug.Log("[FetchPassportRefreshToken] : " + data);
                    if (data != null)
                    {
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

        private IEnumerator FetchConfidentialPassportToken(
            OpenIDConnectArguments OpenIDConnectArguments, string Code, string ClientSecret,
            Action<ErrorCode, Exception> LoginFailAction)
        {
            tokenStatus.Value = TokenStatus.Renewing;

            IObservable<CredentialsToken> ob = null;

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

            Debug.Log($"[FetchToken] Confidential ClientID : {ClientID}");
            Debug.Log($"[FetchToken] Confidential ClientSecret : {ClientSecret}");
            Debug.Log($"[FetchToken] Confidential GrantType : {GrantType}");
            Debug.Log($"[FetchToken] Confidential RedirectURI : {RedirectURI}");
            Debug.Log($"[FetchToken] Confidential code : {Code}");

            ob = AuthService.Instance.ConfidentialPassportToken(ClientID, ClientSecret, GrantType, RedirectURI, Code);

            var disposable = TokenSubscribeOn(ob, LoginFailAction);

            yield return new WaitUntil(() => tokenStatus.Value != TokenStatus.Renewing);
            disposable.Dispose();
        }

        [Obsolete]
        private IEnumerator FetchPassportClientToken(string applicationId, string applicationKey, string grantType, Action<ErrorCode, Exception> FailAction)
        {
            clientTokenStatus.Value = TokenStatus.Renewing;

            IObservable<ClientToken> ob = null;

            if (EnvAdmin.Instance.CurrentEnv.Value == EnvType.Alpha)
            {
                ob = AuthService.Instance.AlphaPassportClientToken(applicationId, applicationKey, grantType);
            }
            else
            {
                ob = AuthService.Instance.PassportClientToken(applicationId, applicationKey, grantType);
            }

            var disposable = ob.SubscribeOn(Scheduler.MainThreadEndOfFrame)
                                .ObserveOn(Scheduler.MainThread)
                                .OnErrorRetry((Exception ex) => Debug.Log(ex), retryCount: 3, TimeSpan.FromSeconds(1))
                                .Subscribe(data =>   // on success
                                {
                                    Debug.Log("[FetchPassportAppToken] FetchPassportAppToken : " + data);
                                    if (data != null)
                                    {
                                        ClientTokenConfig(data);
                                    }
                                    else
                                    {
                                        clientTokenStatus.Value = TokenStatus.Expired;
                                        //LoginFailAction?.Invoke(null);
                                    }
                                },
                                error => // on error
                                {
                                    Debug.LogWarning($"[FetchPassportAppToken] FetchPassportAppToken error : {error}");
                                    clientTokenStatus.Value = TokenStatus.Expired;
                                    FailAction?.Invoke(ErrorCode.TOKEN_IS_EMPTY, error);
                                },
                                () =>
                                {
                                    Debug.Log("[FetchPassportAppToken] FetchPassportAppToken complte : ");
                                });

            yield return new WaitUntil(() => IsClientTokenNotRenewing());


            disposable.Dispose();
        }

        private IEnumerator FetchPassportClientTokenWithRealm(string realm, string applicationId, string applicationKey, string grantType, Action<ErrorCode, Exception> FailAction)
        {
            clientTokenStatus.Value = TokenStatus.Renewing;

            IObservable<ClientToken> ob = null;

            ob = AuthService.Instance.PassportClientTokenWithRealm(
                realm, applicationId, applicationKey, grantType
            );

            var disposable = ob.SubscribeOn(Scheduler.MainThreadEndOfFrame)
                                .ObserveOn(Scheduler.MainThread)
                                .OnErrorRetry((Exception ex) => Debug.Log(ex), retryCount: 3, TimeSpan.FromSeconds(1))
                                .Subscribe(data =>   // on success
                                {
                                    Debug.Log("[FetchPassportAppTokenWithRealm] FetchToken : " + data);
                                    if (data != null)
                                    {
                                        ClientTokenConfig(data);
                                    }
                                    else
                                    {
                                        clientTokenStatus.Value = TokenStatus.Expired;
                                        //LoginFailAction?.Invoke(null);
                                    }
                                },
                                error => // on error
                                {
                                    Debug.LogWarning($"[FetchPassportAppTokenWithRealm] FetchToken error : {error}");
                                    clientTokenStatus.Value = TokenStatus.Expired;
                                    FailAction?.Invoke(ErrorCode.TOKEN_IS_EMPTY, error);
                                },
                                () =>
                                {
                                    Debug.Log("[FetchPassportAppTokenWithRealm] FetchToken complte : ");
                                });

            yield return new WaitUntil(() => IsClientTokenNotRenewing());
            disposable.Dispose();
        }


        private IEnumerator FetchPublicPassportToken(
            OpenIDConnectArguments OpenIDConnectArguments, string Code, string CodeVerifier,
            Action<ErrorCode, Exception> LoginFailAction
        )
        {
            tokenStatus.Value = TokenStatus.Renewing;

            IObservable<CredentialsToken> ob = null;

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

            Debug.Log($"[FetchToken] Public ClientID : {ClientID}");
            Debug.Log($"[FetchToken] Public CodeVerifier : {CodeVerifier}");
            Debug.Log($"[FetchToken] Public GrantType : {GrantType}");
            Debug.Log($"[FetchToken] Public RedirectURI : {RedirectURI}");
            Debug.Log($"[FetchToken] Public code : {Code}");

            ob = AuthService.Instance.PublicPassportToken(ClientID, CodeVerifier, GrantType, RedirectURI, Code);

            var disposable = TokenSubscribeOn(ob, LoginFailAction);

            yield return new WaitUntil(() => IsTokenNotRenewing());
            disposable.Dispose();
        }

        private IDisposable TokenSubscribeOn(IObservable<CredentialsToken> ob, Action<ErrorCode, Exception> LoginFailAction)
        {
            return ob.SubscribeOn(Scheduler.MainThreadEndOfFrame)
                        .ObserveOn(Scheduler.MainThread)
                        .OnErrorRetry((Exception ex) => Debug.Log(ex), retryCount: 3, TimeSpan.FromSeconds(1))
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
        }

        private void StoreToken(Token token = null)
        {
            PlayerPrefs.SetString(IdTokenKey, token?.idToken ?? "");
            PlayerPrefs.SetString(AccessTokenKey, token?.accessToken ?? "");
            PlayerPrefs.SetString(RefreshTokenKey, token?.refreshToken ?? "");
        }

        private void StoreClientToken(ClientToken token = null)
        {
            PlayerPrefs.SetString(ClientTokenKey, token?.access_token ?? "");
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
