using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Maxst.Avatar
{
    [Serializable]
    public class Resource
    {
        [JsonProperty("os")]
        public string os;

        [JsonProperty("catalogUri")]
        public string catalogUri;

        [JsonProperty("hashUri")]
        public string hashUri;

        [JsonProperty("bundleUris")]
        public List<string> bundleUris;

        public CatalogDownLoadUri catalogDownloadUri;
    }
}