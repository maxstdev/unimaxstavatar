using UnityEngine;

namespace Maxst.Settings
{
    public enum OpenIDConnectSettingKey
    {
        CodeChallengeMethod,
        LoginAPI,
        LoginUrl,
        GrantType
    }

    [CreateAssetMenu(fileName = "OpenIDConnectSetting", menuName = "Packages/MaxSSO/OpenIDConnectSetting", order = 1000)]
    public class OpenIDConnectSetting : ScriptableDictionary<OpenIDConnectSettingKey, string>
    {

    }
}
