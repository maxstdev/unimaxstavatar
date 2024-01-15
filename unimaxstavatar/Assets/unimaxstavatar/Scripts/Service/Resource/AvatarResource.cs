using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Maxst.Avatar
{
    [Serializable]
    public class AvatarResource
    {
        [JsonProperty("id")]
        public long id;

        [JsonProperty("mainCategory")]
        public string mainCategory;

        [JsonProperty("subCategory")]
        public string subCategory;

        [JsonProperty("name")]
        public string name;

        [JsonProperty("imageUri")]
        public string imageUri;
        
        [JsonProperty("hidden")]
        public bool hidden;

        [JsonProperty("resources")]
        public List<Resource> resources;

        [JsonProperty("createdAt")]
        public string createdAt;

        public ThumbnailDownLoadUri thumbnailDownLoadUri;
    }
}
