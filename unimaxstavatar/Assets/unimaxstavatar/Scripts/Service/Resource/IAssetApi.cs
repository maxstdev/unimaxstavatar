using Maxst.Avatar;
using Retrofit.Methods;
using Retrofit.Parameters;
using System;

namespace Maxst.Resource
{
    public interface IAssetApi
    {
        [Get("{publicPath}")]
        IObservable<Container> FetchPublicContainer(
        [Header("Authorization")] string accessToken, [Path("publicPath")] string publicPath);

        [Get("{uri}")]
        IObservable<Contain> FetchContain(
        [Header("Authorization")] string accessToken, [Path("uri")] string uri);
        
        /*
        [Get("/{clientId}/{appFolderName}/{platform}/")]
        IObservable<String> FetchAllCategoryContainerMetaList(
        [Header("Authorization")] string accessToken, [Path("clientId")] string clientId,
        [Path("appFolderName")] string appFolderName, [Path("platform")] string platform, [Query("type")] string type);
        */
        
        [Get("/{clientId}/{appFolderPrefix}/{category}/")]
        IObservable<ContainerMeta> FetchCategoryContainerMetaList(
        [Header("Authorization")] string accessToken, [Path("clientId")] string clientId,
        [Path("appFolderPrefix")] string appFolderPrefix, [Path("category")] string category, [Query("type")] string type);

        [Get("/{clientId}/{appFolderPrefix}/{category}/")]
        IObservable<ContainMeta> FetchContainerMeta(
        [Header("Authorization")] string accessToken, [Path("clientId")] string clientId,
        [Path("appFolderPrefix")] string appFolderPrefix, [Path("category")] string category, [Query("type")] string type);

        // Need to delete test function
        [Get("/{custom}/")]
        IObservable<ContainerMeta> FetchCustomMeta(
        [Header("Authorization")] string accessToken, [Path("custom")] string custom, [Query("type")] string type);
    }
}
