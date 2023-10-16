using Newtonsoft.Json;
using System;

namespace Maxst.Avatar
{
    [Serializable]
    public class ThumbnailDownLoadUri
    {
        [JsonProperty("type")]
        public string type;

        [JsonProperty("uri")]
        public string uri;
    }
}
