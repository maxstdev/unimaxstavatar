using Newtonsoft.Json;
using System;

namespace Maxst.Passport
{
    [Serializable]
    public class CredentialsToken
    {
        [JsonProperty("token")]
        public string access_token;
        [JsonProperty("refresh_token")]
        public string refresh_token;
        [JsonProperty("id_token")]
        public string id_token;
    }
}
