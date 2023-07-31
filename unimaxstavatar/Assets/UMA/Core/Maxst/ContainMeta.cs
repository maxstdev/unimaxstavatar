using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Maxst.Avatar
{
    [Serializable]
    public class ContainMeta
    {
        [JsonProperty("_id")]
        public string _id;

        [JsonProperty("uuid")]
        public string uuid;

        [JsonProperty("type")]
        public string type;

        [JsonProperty("name")]
        public string name;

        [JsonProperty("parents")]
        public string parents;

        [JsonProperty("description")]
        public string description;

        [JsonProperty("isPublic")]
        public bool isPublic;

        [JsonProperty("owners")]
        public List<String> owners;

        [JsonProperty("updatedAt")]
        public String updatedAt;

        [JsonProperty("createdAt")]
        public String createdAt;

        [JsonProperty("extension")]
        public Dictionary<string, string> extension;

        [JsonProperty("updateAt")]
        public String updateAt;

        [JsonProperty("uri")]
        public String uri;
    }
}
