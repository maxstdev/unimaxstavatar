using i5.Toolkit.Core.OpenIDConnectClient;
using Maxst.Settings;
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Maxst.Passport
{
#if !UNITY_ANDROID && !UNITY_IOS
    public class LoginMenu : MonoBehaviour, IOpenIDConnectListener, MaxstIOpenIDConnectProvider
#else
    public class LoginMenu : MonoBehaviour, IOpenIDConnectListener
#endif
    {
        [SerializeField] private GameObject loginPopupPrefeb;
        [SerializeField] private Button loginFromEmailBtn;
        [SerializeField] private OpenTab openTab;
        [SerializeField] private Button LogoutBtn;
        [SerializeField] private Button ReFreshBtn;
        [SerializeField] private OpenIDConnectArguments openidConnectArguments;

        [SerializeField] private UnityEvent successEvent;
        [SerializeField] private UnityEvent failEvent;

        private OpenIDConnectAdapter OpenIdConnectAdapter;

        private void Awake()
        {
#if !UNITY_EDITOR && UNITY_WEBGL
            Debug.Log("LoginMenu Awake UNITY");
#else
            Debug.Log("LoginMenu UNITY");
            loginFromEmailBtn.onClick.AddListener(OnClickloginFromEmailBtn);
            LogoutBtn.onClick.AddListener(OnClickLogoutBtn);
            ReFreshBtn.onClick.AddListener(OnClickRefreshBtn);
            OpenIdConnectAdapter = OpenIDConnectAdapter.Instance;
            OpenIdConnectAdapter.InitOpenIDConnectAdapter(openidConnectArguments, this);
#if !UNITY_ANDROID && !UNITY_IOS
            OpenIdConnectAdapter.SetWindowLoginServiceManger(this);
#endif

#endif

        }

#if UNITY_EDITOR || !UNITY_WEBGL
        private void OnClickLogoutBtn()
        {
            OpenIdConnectAdapter.OnLogout();
        }

        private void OnClickloginFromEmailBtn()
        {
            var PKCEManagerInstance = PKCEManager.GetInstance();
            var CodeVerifier = PKCEManagerInstance.GetCodeVerifier();
            var CodeChallenge = PKCEManagerInstance.GetCodeChallenge(CodeVerifier);
            OpenIdConnectAdapter.ShowOIDCProtocolLoginPage(CodeVerifier, CodeChallenge);
        }

        private void OnClickRefreshBtn()
        {
            OpenIdConnectAdapter.OnRefreshToken();
        }
#endif

        public void OnSuccess(Token Token, RequestType Type)
        {
            Debug.Log($"[LoginMenu] OnSuccess RequestType : {Type}");
            Debug.Log($"[LoginMenu] OnSuccess idToken : {Token.idToken}");
            Debug.Log($"[LoginMenu] OnSuccess accessToken : {Token.accessToken}");
            Debug.Log($"[LoginMenu] OnSuccess refreshToken : {Token.refreshToken}");
            successEvent?.Invoke();
        }

        public void OnFail(ErrorCode ErrorCode, Exception e)
        {
            Debug.Log($"[LoginMenu] OnFail : {ErrorCode}");
            Debug.Log($"[LoginMenu] Exception : {e}");
            failEvent?.Invoke();
        }

        public void OnLogout()
        {
            Debug.Log($"[LoginMenu] OnLogout");
        }

#if !UNITY_ANDROID && !UNITY_IOS
        public void OnAuthorazationCode(string code)
        {
            Debug.Log($"OnAuthorazationCode : {code}");
            OpenIdConnectAdapter.AcceceToken(code);
        }

        public string GetLoginPageURL(string redirectUri)
        {
            var PKCEManagerInstance = PKCEManager.GetInstance();
            var CodeVerifier = PKCEManagerInstance.GetCodeVerifier();
            var CodeChallenge = PKCEManagerInstance.GetCodeChallenge(CodeVerifier);
            return OpenIdConnectAdapter.GetURL(redirectUri, CodeVerifier, CodeChallenge);
        }
#endif
    }
}