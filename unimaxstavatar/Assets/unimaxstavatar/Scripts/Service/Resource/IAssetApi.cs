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
    }
}
