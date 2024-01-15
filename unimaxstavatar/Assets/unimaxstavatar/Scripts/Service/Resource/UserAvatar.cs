using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using UMA.CharacterSystem;

namespace Maxst.Avatar
{
    [Serializable]
    public class UserAvatar
    {
        [JsonProperty("id")]
        public string id;

        [JsonProperty("recipeStr")]
        public string recipeStr;

        [JsonProperty("slots")]
        public List<Slot> slots;

        public DynamicCharacterAvatar avatar { get; set; }

        public class Slot
        {
            [JsonProperty("slot")]
            public string slot;

            [JsonProperty("itemId")]
            public long itemId;

            [JsonProperty("recipe")]
            public string recipe;

            [JsonProperty("imageUri")]
            public string imageUri;

            [JsonProperty("assetResourceInfo")]
            public List<Resource> assetResourceInfo;

            public ThumbnailDownLoadUri thumbnailDownLoadUri;
        }
    }
}