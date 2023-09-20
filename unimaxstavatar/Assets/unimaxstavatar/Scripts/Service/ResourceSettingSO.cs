using UnityEngine;

namespace Maxst.Avatar
{
    [CreateAssetMenu(fileName = "ResourceSettingSO", menuName = "ScriptableObjects/ResourceSettingSO", order = 1000)]
    public class ResourceSettingSO : ScriptableSingleton<ResourceSettingSO>
    {
        public string BaseUrl;
        public Platform Platform;
    }
}