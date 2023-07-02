using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Maxst.Resource
{
    [Serializable]
    public class Container
    {
        [JsonProperty("type")]
        public string type;

        [JsonProperty("name")]
        public string name;

        [JsonProperty("contains")]
        public List<Contain> contains;

        [JsonProperty("resources")]
        public List<Resource> resources;

        [JsonProperty("uri")]
        public string uri;
    }
}