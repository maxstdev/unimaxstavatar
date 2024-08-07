using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Maxst.Avatar
{
    [Serializable]
    public class AvatarProfile
    {
        [JsonProperty("profile_uuid")]
        public string profileUuid;

        [JsonProperty("image_file_name")]
        public string imageFileName;

        [JsonProperty("pre_signed_upload_url")]
        public string uploadUrl;
    }
}
