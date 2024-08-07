using Maxst.Passport;
using Maxst.Settings;
using Retrofit;
using Retrofit.HttpImpl;
using Retrofit.Parameters;
using System;
using System.Reflection;
using UnityEngine;

namespace Maxst.Avatar
{
    public class ProfileService : RestAdapter, IProfileApi
    {
        private static ProfileService _instance;

        public static ProfileService Instance
        {
            get
            {
                if(_instance == null)
                {
                    var profileService = new GameObject(typeof(ProfileService).FullName);
                    _instance = profileService.AddComponent<ProfileService>();
                    if (Application.isPlaying)
                        DontDestroyOnLoad(profileService);
                }
                return _instance;
            }
        }

        protected override HttpImplement SetHttpImpl()
        {
            var httpImpl = new UnityWebRequestImpl();
            httpImpl.EnableDebug = true;
            return httpImpl;
        }

        protected override RequestInterceptor SetIntercepter()
        {
            return null;
        }

        private string GetUrl()
        {
            return $"{EnvAdmin.Instance.AuthUrlSetting.Urls[URLType.API]}";
        }

        protected override void SetRestAPI()
        {
            baseUrl = GetUrl();
            iRestInterface = typeof(IProfileApi);
        }

        public IObservable<AvatarProfile> PutAvatarProfileURL(
            [Retrofit.Parameters.Header("Authorization")] string accessToken,
            [Body] AvatarProfile avatarProfile
            )
        {
            return SendRequest<AvatarProfile>(MethodBase.GetCurrentMethod(), accessToken, avatarProfile) as IObservable<AvatarProfile>;
        }

        public IObservable<AvatarProfile> PatchAvatarProfileURL(
            [Retrofit.Parameters.Header("Authorization")] string accessToken, 
            [Body] AvatarProfile avatarProfile)
        {
            return SendRequest<AvatarProfile>(MethodBase.GetCurrentMethod(), accessToken, avatarProfile) as IObservable<AvatarProfile>;
        }
    }
}
