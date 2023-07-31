using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Maxst.Avatar
{
    [Serializable]
    public class ContainerMeta
    {
        [JsonProperty("type")]
        public string type;

        [JsonProperty("name")]
        public string name;
        
        [JsonProperty("contains")]
        public List<ContainMeta> contains;
        
        [JsonProperty("resources")]
        public List<ResourceMeta> resources;
    }
}