using i5.Toolkit.Core.OpenIDConnectClient;
using i5.Toolkit.Core.ServiceCore;
using i5.Toolkit.Core.Utilities;
using Maxst.Settings;
using System;
using System.Linq;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;


namespace Maxst.Passport
{
    public class OpenIDConnectAdapter
    {
        public OpenIDConnectArguments OpenIDConnectArguments;
        public IOpenIDConnectListener IOpenIDConnectListener { get; set; } = null;

        private string CodeVerifier;
        private static OpenIDConnectAdapter instance;

#if !UNITY_ANDROID && !UNITY_IOS
        private MaxstOpenIDConnectService OpenIDConnectService;
#endif
        public static OpenIDConnectAdapter Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new OpenIDConnectAdapter();

                }
                return instance;
            }
        }

        public void InitOpenIDConnectAdapter(OpenIDConnectArguments openidArguments, IOpenIDConnectListener listner)
        {
            OpenIDConnectArguments = openidArguments;
            IOpenIDConnectListener = listner;
        }
#if !UNITY_ANDROID && !UNITY_IOS
        public void SetWindowLoginServiceManger(MaxstIOpenIDConnectProvider IOpenIDConnectProvider)
        {
            if (OpenIDConnectService != null)
            {
                ServiceManager.RemoveService<MaxstOpenIDConnectService>();
                OpenIDConnectService = null;
            }

            OpenIDConnectService = new MaxstOpenIDConnectService(IOpenIDConnectProvider)
            {
                OidcProvider = new MaxstOpenIDConnectProvider(IOpenIDConnectProvider)
            };
            ServiceManager.RegisterService(OpenIDConnectService);
        }
#endif
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void SetDeeplink()
        {
            Application.deepLinkActivated += Instance.OnSuccessAuthorization;
        }

        public void ShowOIDCProtocolLoginPage(string CodeVerifier, string CodeChallenge)
        {
#if UNITY_ANDROID
            OpenIDConnectArguments.TryGetValue(OpenIDConnectArgument.AndroidRedirectUri, out var RedirectURI);
            Application.OpenURL(GetURL(RedirectURI, CodeVerifier, CodeChallenge));
#elif UNITY_IOS
            OpenIDConnectArguments.TryGetValue(OpenIDConnectArgument.iOSRedirectUri, out var RedirectURI);
            Application.OpenURL(GetURL(RedirectURI, CodeVerifier, CodeChallenge));
#else
            OpenIDConnectArguments.TryGetValue(OpenIDConnectArgument.WebRedirectUri, out var RedirectURI);
            ServiceManager.GetService<MaxstOpenIDConnectService>().OpenLoginPageAsync();
#endif
        }

        public string GetURL(string RedirectURI, string CodeVerifier, string CodeChallenge)
        {
#if !UNITY_ANDROID && !UNITY_IOS
            OpenIDConnectArguments.SetValue(OpenIDConnectArgument.WebRedirectUri, RedirectURI);
# endif
            var Setting = EnvAdmin.Instance.OpenIDConnectSetting;
            Setting.TryGetValue(OpenIDConnectSettingKey.LoginUrl, out var LoginUrl);
            OpenIDConnectArguments.TryGetValue(OpenIDConnectArgument.ClientID, out var ClientID);
            OpenIDConnectArguments.TryGetValue(OpenIDConnectArgument.ResponseType, out var ResponseType);
            OpenIDConnectArguments.TryGetValue(OpenIDConnectArgument.Scope, out var Scope);

            this.CodeVerifier = CodeVerifier;

            Setting.TryGetValue(OpenIDConnectSettingKey.CodeChallengeMethod, out var CodeChallengeMethod);
            Setting.TryGetValue(OpenIDConnectSettingKey.LoginAPI, out var LoginAPI);

            var Host = EnvAdmin.Instance.AuthUrlSetting[URLType.API];

            var URL = string.Format(LoginUrl, Host, LoginAPI, ClientID, ResponseType, Scope, RedirectURI, CodeChallenge, CodeChallengeMethod);

            Debug.Log($"[OpenIDConnectAdapter] : url : {URL}");

            return URL;
        }

        public void OnRefreshToken(Action completeAction = null)
        {
            Debug.Log("[OpenIDConnectAdapter] OnRefresh");
            RefreshToken(completeAction);
        }

        public void OnLogout(Action completeAction = null)
        {
            Debug.Log("[OpenIDConnectAdapter] OnLogout");
            SessionLogout(completeAction);
        }

        public void ShowSAMLProtocolLoginPage()
        {
            Debug.Log("[OpenIDConnectAdapter] : SAML Protocol is not supported.");
        }
        private void OnSuccessAuthorization(string url)
        {
            string Query = url.Split("?"[0])[1];

            var AuthorizationDictionary = Query.Replace("?", "").Split('&').ToDictionary(x => x.Split('=')[0], x => x.Split('=')[1]);

            var state = AuthorizationDictionary["state"];
            var session_state = AuthorizationDictionary["session_state"];
            var code = AuthorizationDictionary["code"];

            foreach (var each in AuthorizationDictionary)
            {
                Debug.Log($"[OpenIDConnectAdapter] OnSuccessAuthorization Key: {each.Key}, Value: {each.Value}");
            }

            AcceceToken(code);
        }

        public void AcceceToken(string code)
        {
            MainThreadDispatcher.StartCoroutine(
                    TokenRepo.Instance.GetPassportToken(
                        OpenIDConnectArguments, code, CodeVerifier,
                        (status, token) =>
                        {
                            if (status != TokenStatus.Validate)
                            {
                                Debug.LogWarning("[OpenIDConnectAdapter] auth fail.. need retry login");
                                TokenRepo.Instance.Config(null);
                                return;
                            }
                            IOpenIDConnectListener?.OnSuccess(token, RequestType.ACCECE_TOKEN);
                            Debug.Log($"[OpenIDConnectAdapter] token.idToken : {token.idToken}");
                            Debug.Log($"[OpenIDConnectAdapter] token.accessToken : {token.accessToken}");
                            Debug.Log($"[OpenIDConnectAdapter] token.refreshToken : {token.refreshToken}");
                        },
                        (LoginErrorCode, Exception) =>
                        {
                            IOpenIDConnectListener?.OnFail(LoginErrorCode, Exception);
                            Debug.Log($"[OpenIDConnectAdapter] LoginErrorCode : {LoginErrorCode}");
                        }
                    )
                );
        }
        private void RefreshToken(Action complete)
        {
            MainThreadDispatcher.StartCoroutine(
                    TokenRepo.Instance.GetPassportRefreshToken(
                        OpenIDConnectArguments,
                        (status, token) =>
                        {
                            if (status != TokenStatus.Validate)
                            {
                                Debug.LogWarning("[OpenIDConnectAdapter] auth fail.. need retry login");
                                TokenRepo.Instance.Config(null);
                                complete?.Invoke();
                                return;
                            }
                            
                            if (token == null)
                            {
                                Debug.Log($"[OpenIDConnectAdapter] Token value does not exist");
                            }
                            else {
                                Debug.Log($"[OpenIDConnectAdapter] OnSuccess RefreshToken idToken : {token.idToken}");
                                Debug.Log($"[OpenIDConnectAdapter] OnSuccess RefreshToken accessToken : {token.accessToken}");
                                Debug.Log($"[OpenIDConnectAdapter] OnSuccess RefreshToken refreshToken : {token.refreshToken}");

                                IOpenIDConnectListener?.OnSuccess(token, RequestType.REFRESH_TOKEN);
                            }
                            complete?.Invoke();
                        },
                        (Exception) =>
                        {
                            IOpenIDConnectListener?.OnFail(ErrorCode.REFRESH, Exception);
                            complete?.Invoke();
                        }
                    )
                );
        }

        private void SessionLogout(Action complete)
        {
            MainThreadDispatcher.StartCoroutine(
                TokenRepo.Instance.GetPassportRefreshToken(
                    OpenIDConnectArguments,
                    (status, token) =>
                    {
                        if (status != TokenStatus.Validate)
                        {
                            Debug.LogWarning("[OpenIDConnectAdapter] auth fail.. need retry login");
                            TokenRepo.Instance.Config(null);
                            complete?.Invoke();
                            return;
                        }

                        TokenRepo.Instance.PassportLogout(
                            OpenIDConnectArguments,
                        () =>
                        {
                            Debug.Log($"[OpenIDConnectAdapter] SessionLogout success");
                            complete?.Invoke();
                            IOpenIDConnectListener?.OnLogout();
                        },
                        (Exception) =>
                        {
                            Debug.Log($"[OpenIDConnectAdapter] SessionLogout fail : {Exception}");
                            complete?.Invoke();
                            IOpenIDConnectListener?.OnFail(ErrorCode.LOGOUT, Exception);
                        });
                    },
                    (Exception) =>
                    {
                        IOpenIDConnectListener?.OnFail(ErrorCode.LOGOUT_REFRESH, Exception);
                        Debug.Log($"[OpenIDConnectAdapter] Exception : {Exception}");
                        complete?.Invoke();
                    }
                )
            );
        }
    }
}
