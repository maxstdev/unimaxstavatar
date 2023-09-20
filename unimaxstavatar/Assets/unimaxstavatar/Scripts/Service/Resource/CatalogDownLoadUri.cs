using Newtonsoft.Json;
using System;
namespace Maxst.Avatar
{
    [Serializable]
    public class CatalogDownLoadUri
    {
        [JsonProperty("type")]
        public string type;

        [JsonProperty("uri")]
        public string uri;
    }
}