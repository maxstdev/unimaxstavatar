using Maxst;
using UnityEngine;

public enum Platform
{
    StandaloneWindows64, iOS, Android
}

[CreateAssetMenu(fileName = "ResourceSettingSO", menuName = "ScriptableObjects/ResourceSettingSO", order = 1000)]
public class ResourceSettingSO : ScriptableSingleton<ResourceSettingSO>
{
    public string BaseUrl;
    public string Container;
    public string SaveFileName;
    public string SaveExtensionsFileName;
    public string CatalogJsonFileName;
    public string Ext;
    public string Tusupload;
    public Platform Platform;
}
