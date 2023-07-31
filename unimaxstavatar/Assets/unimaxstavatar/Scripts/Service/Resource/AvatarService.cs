using Maxst.Avatar;
using Retrofit;
using Retrofit.HttpImpl;
using Retrofit.Parameters;
using System;
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

        public IObservable<Container> FetchPublicContainer([Retrofit.Parameters.Header("Authorization")] string accessToken, string publicPath)
        {
            return SendRequest<Container>(MethodBase.GetCurrentMethod(), accessToken, publicPath) as IObservable<Container>;
        }

        public IObservable<Contain> FetchContain([Retrofit.Parameters.Header("Authorization")] string accessToken, string uri)
        {
            return SendRequest<Contain>(MethodBase.GetCurrentMethod(), accessToken, uri) as IObservable<Contain>;
        }

        public IObservable<ContainMeta> FetchContainerMeta([Retrofit.Parameters.Header("Authorization")] string accessToken, [Path("clientId")] string clientId, [Path("appFolderPrefix")] string appFolderPrefix, [Path("category")] string category, [Query("type")] string type)
        {
            return SendRequest<ContainMeta>(MethodBase.GetCurrentMethod(), accessToken, clientId, appFolderPrefix, category, type) as IObservable<ContainMeta>; ;
        }

        public IObservable<ContainerMeta> FetchCategoryContainerMetaList([Retrofit.Parameters.Header("Authorization")] string accessToken, [Path("clientId")] string clientId, [Path("appFolderPrefix")] string appFolderPrefix, [Path("category")] string category, [Query("type")] string type)
        {
            return SendRequest<ContainerMeta>(MethodBase.GetCurrentMethod(), accessToken, clientId, appFolderPrefix, category, type) as IObservable<ContainerMeta>; 
        }

        public IObservable<ContainerMeta> FetchCustomMeta([Retrofit.Parameters.Header("Authorization")] string accessToken, [Path("custom")] string custom, [Query("type")] string type)
        {
            return SendRequest<ContainerMeta>(MethodBase.GetCurrentMethod(), accessToken, custom, type) as IObservable<ContainerMeta>;
        }
    }
}