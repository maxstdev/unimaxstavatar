using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Maxst.Avatar
{
    [Serializable]
    public class ResourceMeta
    {
        [JsonProperty("uuid")]
        public string uuid;

        [JsonProperty("assetResourceFileId")]
        public string assetResourceFileId;

        [JsonProperty("originalFileName")]
        public string originalFileName;

        [JsonProperty("fileExtension")]
        public string fileExtension;

        [JsonProperty("description")]
        public string description;

        [JsonProperty("dataUrl")]
        public string dataUrl;

        [JsonProperty("parents")]
        public string parents;

        [JsonProperty("owners")]
        public List<String> owners;

        [JsonProperty("isPublic")]
        public bool isPublic;

        [JsonProperty("updatedAt")]
        public String updatedAt;

        [JsonProperty("trashingUserId")]
        public String trashingUserId;

        [JsonProperty("trashedAt")]
        public String trashedAt;

        [JsonProperty("readAt")]
        public String readAt;

        [JsonProperty("createdAt")]
        public String createdAt;

        [JsonProperty("type")]
        public String type;

        [JsonProperty("extension")]
        public Dictionary<string, string> extension;

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            ResourceMeta resourceMeta = (ResourceMeta)obj;
            return uuid.Equals(resourceMeta.uuid) && dataUrl.Equals(resourceMeta.dataUrl);
        }

        public override int GetHashCode() { 
            return HashCode.Combine(uuid, dataUrl); 
        }
    }
}
