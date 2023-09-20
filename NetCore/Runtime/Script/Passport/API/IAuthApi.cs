using Retrofit.Methods;
using Retrofit.Parameters;
using System;

namespace Maxst.Passport
{
    public interface IAuthApi
    {
        [Obsolete]
        [Post("/api/passport/token")]
        IObservable<ClientToken> AlphaPassportClientToken(
        [Field("client_id")] string applicationId,
        [Field("client_secret")] string applicationKey,
        [Field("grant_type")] string grantType);

        [Obsolete]
        [Post("/passport/token")]
        IObservable<ClientToken> PassportClientToken(
        [Field("client_id")] string applicationId,
        [Field("client_secret")] string applicationKey,
        [Field("grant_type")] string grantType);

        [Post("/auth/realms/{realm}/protocol/openid-connect/token")]
        IObservable<ClientToken> PassportClientTokenWithRealm([Path("realm")] string realm,
        [Field("client_id")] string applicationId,
        [Field("client_secret")] string applicationKey,
        [Field("grant_type")] string grantType);

        [Post("/profile/v1/public/oauth/token")]
        IObservable<CredentialsToken> ConfidentialPassportToken([Field("client_id")] string client_id,
        [Field("client_secret")] string client_secret,
        [Field("grant_type")] string grant_type,
        [Field("redirect_uri")] string redirect_uri,
        [Field("code")] string code);

        [Post("/profile/v1/public/oauth/token")]
        IObservable<CredentialsToken> PublicPassportToken([Field("client_id")] string client_id,
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
        IObservable<string> PublicPassportLogout(
        [Header("Authorization")] string accessToken,
        [Field("client_id")] string client_id,
        [Field("refresh_token")] string refresh_token,
        [Field("id_token")] string id_token);

        [Post("/profile/v1/passport/logout")]
        IObservable<string> ConfidentialPassportLogout(
        [Header("Authorization")] string accessToken,
        [Field("client_id")] string client_id,
        [Field("refresh_token")] string refresh_token,
        [Field("client_secret")] string client_secret);
    }
}
