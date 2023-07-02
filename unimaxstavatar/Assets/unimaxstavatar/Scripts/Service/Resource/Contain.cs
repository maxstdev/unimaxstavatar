using Newtonsoft.Json;
using System;

namespace Maxst.Resource
{
    [Serializable]
    public class Contain
    {
        [JsonProperty("type")]
        public string type;

        [JsonProperty("uri")]
        public string uri;
    }
}