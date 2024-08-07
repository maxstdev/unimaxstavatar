using Retrofit.Methods;
using Retrofit.Parameters;
using System;

namespace Maxst.Avatar
{
    public interface IProfileApi
    {
        [Headers("Content-Type: application/json")]
        [Put("/profile/v2/profile/image")]
        IObservable<AvatarProfile> PutAvatarProfileURL(
        [Header("Authorization")] string accessToken,
        [Body] AvatarProfile avatarProfile
        );

        [Headers("Content-Type: application/json")]
        [Patch("/profile/v2/profile/image")]
        IObservable<AvatarProfile> PatchAvatarProfileURL(
        [Header("Authorization")] string accessToken,
        [Body] AvatarProfile avatarProfile
        );
    }
}
