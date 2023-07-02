using Newtonsoft.Json;
using System;

namespace Maxst.Resource
{
    [Serializable]
    public class Resource
    {
        [JsonProperty("type")]
        public string type;

        [JsonProperty("name")]
        public string name;

        [JsonProperty("uri")]
        public string uri;
    }
}