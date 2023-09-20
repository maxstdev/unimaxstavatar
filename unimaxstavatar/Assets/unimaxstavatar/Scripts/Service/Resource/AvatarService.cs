using Maxst.Avatar;
using Retrofit;
using Retrofit.HttpImpl;
using Retrofit.Parameters;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Maxst.Resource
{
    public class AvatarService : RestAdapter, IAssetApi
    {
        private static AvatarService _instance;

        public static AvatarService Instance
        {
            get
            {
                if (_instance == null)
                {
                    var assetService = new GameObject(typeof(AvatarService).FullName);
                    _instance = assetService.AddComponent<AvatarService>();
                    DontDestroyOnLoad(assetService);
                }
                return _instance;
            }
        }

        protected override void SetRestAPI()
        {
            baseUrl = GetUrl();
            iRestInterface = typeof(IAssetApi);
        }

        private string GetUrl()
        {
            return ResourceSettingSO.Instance.BaseUrl;
        }

        protected override RequestInterceptor SetIntercepter()
        {
            return null;
        }

        protected override HttpImplement SetHttpImpl()
        {
            var httpImpl = new UnityWebRequestImpl();
            httpImpl.EnableDebug = true;
            return httpImpl;
        }

        public IObservable<List<AvatarResource>> FetchAvatarResources([Retrofit.Parameters.Header("Authorization")] string accessToken, [Query("mainCategory")] string mainCategory, [Query("subCategory")] string subCategory, [Query("os")] string os, [Query("appId")] string appId)
        {
             return SendRequest<List<AvatarResource>>(MethodBase.GetCurrentMethod(), accessToken, mainCategory, subCategory, os, appId) as IObservable<List<AvatarResource>>;
        }

        public IObservable<UserAvatar> FetchUserSaveRecipe(
            [Retrofit.Parameters.Header("Authorization")] string accessToken,
            [Query("os")] string os
        )
        {
            return SendRequest<UserAvatar>(MethodBase.GetCurrentMethod(), accessToken, os) as IObservable<UserAvatar>;
        }

        public IObservable<string> PostUserSaveRecipe([Retrofit.Parameters.Header("Authorization")] string accessToken, [Body] SaveRecipeExtensions saveRecipeExtension)
        {
            return SendRequest<string>(MethodBase.GetCurrentMethod(), accessToken, saveRecipeExtension) as IObservable<string>;
        }
    }
}