using Maxst.Avatar;
using Retrofit.Methods;
using Retrofit.Parameters;
using System;
using System.Collections.Generic;

namespace Maxst.Resource
{
    public interface IAssetApi
    {
        [Get("/asset-integration/assets/avatars")]
        IObservable<List<AvatarResource>> FetchAllAvatarResources(
        [Header("Authorization")] string accessToken,
        [Query("mainCategory")] string mainCategory,
        [Query("os")] string os,
        [Query("appId")] string appId
        );
        
        [Get("/asset-integration/assets/avatars")]
        IObservable<List<AvatarResource>> FetchAvatarResources(
        [Header("Authorization")] string accessToken,
        [Query("mainCategory")] string mainCategory,
        [Query("subCategory")] string subCategory,
        [Query("os")] string os,
        [Query("appId")] string appId
        );

        [Get("/asset-integration/users/avatar/assets")]
        IObservable<UserAvatar> FetchUserSaveRecipe(
        [Header("Authorization")] string accessToken,
        [Query("os")] string os
        );

        [Headers("Content-Type: application/json")]
        [Post("/asset-integration/users/avatar/assets")]
        IObservable<string> PostUserSaveRecipe(
        [Header("Authorization")] string accessToken,
        [Body] SaveRecipeExtensions saveRecipeExtension
        );
    }
}
