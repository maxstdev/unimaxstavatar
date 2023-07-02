using Retrofit.Methods;
using Retrofit.Parameters;
using System;

namespace Maxst.Passport
{
    public interface IAuthApi
    {
        [Post("/profile/v1/public/oauth/token")]
        IObservable<CredentialsToken> PassportToken([Field("client_id")] string client_id,
        [Field("code_verifier")] string code_verifier,
        [Field("grant_type")] string grant_type,
        [Field("redirect_uri")] string redirect_uri,
        [Field("code")] string code);
        
        [Post("/profile/v1/public/oauth/token/refresh")]
        IObservable<CredentialsToken> PassportRefreshToken(
        [Field("client_id")] string client_id,
        [Field("grant_type")] string grant_type,
        [Field("refresh_token")] string refresh_token
        );
        
        [Post("/profile/v1/passport/logout")]
        IObservable<string> PassportLogout(
        [Header("Authorization")] string accessToken,
        [Field("client_id")] string client_id,
        [Field("refresh_token")] string refresh_token,
        [Field("id_token")] string id_token);
    }
}
