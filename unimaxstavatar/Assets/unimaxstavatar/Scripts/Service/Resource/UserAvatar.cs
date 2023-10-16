using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Maxst.Avatar
{
    [Serializable]
    public class UserAvatar
    {
        [JsonProperty("recipeStr")]
        [SerializeField]
        public string recipeStr;

        [JsonProperty("slots")]
        [SerializeField]
        public List<Slot> slots;

        public class Slot
        {
            [JsonProperty("slot")]
            [SerializeField]
            public string slot;

            [JsonProperty("itemId")]
            [SerializeField]
            public long itemId;

            [JsonProperty("recipe")]
            [SerializeField]
            public string recipe;

            [JsonProperty("imageUri")]
            public string imageUri;

            [JsonProperty("assetResourceInfo")]
            [SerializeField]
            public List<Resource> assetResourceInfo;

            public ThumbnailDownLoadUri thumbnailDownLoadUri;
        }
    }
}