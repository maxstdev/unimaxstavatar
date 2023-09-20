using Cysharp.Threading.Tasks;
using Maxst.Avatar;
using Maxst.Passport;
using System;
using System.Collections.Generic;
using System.Linq;
using UMA;
using UMA.CharacterSystem;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.AsyncOperations;

public enum Avatarlabel
{
    slotdata,
    baseavatar,
    female,
    male,
    eyebrow,
    hair,
    eyelash,
    shose,
    eyeware,
    headware,
    legs,
    chest,
    dress,
    feet
}

public enum ExceptionKeyword
{
    placeholder
}

public class AvatarAddressableManager : MonoBehaviour
{
    [SerializeField] private List<OverlayDataAsset> OverlayDataAsset = new();
    [SerializeField] private List<UMATextRecipe> UMATextRecipe = new();
    [SerializeField] private List<RaceData> RaceData = new();
    [SerializeField] private List<UmaTPose> UmaTPose = new();
    [SerializeField] private List<UMAWardrobeRecipe> UMAWardrobeRecipe = new();
    [SerializeField] private List<DynamicUMADnaAsset> DynamicUMADnaAsset = new();
    [SerializeField] private List<DynamicDNAConverterController> DynamicDNAConverterController = new();
    [SerializeField] private List<UnityEngine.Object> SlotData = new();
    [SerializeField] private List<Texture2D> Texture2D = new();

    public Subject<List<UMATextRecipe>> addressableloadComplete = new Subject<List<UMATextRecipe>>();
    public Subject<bool> addressableUpdateComplete = new Subject<bool>();
    private AvatarResourceManager avatarResourceManager;

    private Dictionary<string, string> resAppIdDict = new Dictionary<string, string>();

    Dictionary<string, AsyncOperationHandle<IList<UnityEngine.Object>>> bundleCacheDic = new Dictionary<string, AsyncOperationHandle<IList<UnityEngine.Object>>>();

    private void Start()
    {
#if UMA_ADDRESSABLES
        AddressableException();
        CreateAsync().Forget();
#else
        addressableloadComplete.OnNext(new List<UMATextRecipe>());
#endif
    }

    private void SetAvatarDataManager()
    {
        var o = GameObject.Find("AvatarResourceManager");
        if (o != null)
        {
            avatarResourceManager = o.GetComponent<AvatarResourceManager>();
        }
    }

    private Dictionary<Category, List<AvatarResource>> GetAvatarResources()
    {
        return avatarResourceManager.GetAvatarResources();
    }

    private Dictionary<Category, List<AvatarResource>> GetSaveAvatarResources()
    {
        return avatarResourceManager.GetSaveAvatarResources();
    }

    private Dictionary<Category, List<AvatarResource>> GetPublicResources()
    {
        return avatarResourceManager.GetPublicResources();
    }

#if UMA_ADDRESSABLES
    private async UniTask CreateAsync()
    {
        SetAvatarDataManager();

        var avatarResources = GetAvatarResources();
        var saveAvatarResources = GetSaveAvatarResources();
        var publicResources = GetPublicResources();
        var avatrarRes = SetAvatarRseources(avatarResources, saveAvatarResources, publicResources);

        Addressables.WebRequestOverride += SetHeader;
        await AsyncInitTask(avatrarRes);

        AddressableLoadComplete();
    }

    private List<AvatarResource> SetAvatarRseources(params Dictionary<Category, List<AvatarResource>>[] dicts)
    {
        var result = new List<AvatarResource>();
        foreach (var dict in dicts)
        {
            if (dict.Count > 0)
            {
                foreach (var each in dict.Values)
                {
                    foreach (var item in each)
                    {
                        result.Add(item);
                    }
                }
            }
        }
        return result.Distinct().ToList();
    }

    private async UniTask AsyncInitTask(List<AvatarResource> resources)
    {
        var completionSource = new UniTaskCompletionSource();

        Dictionary<string, string> jsonpathdic = new();

        resources.ForEach(each =>
            each.resources.ForEach(res =>
            {
                jsonpathdic.Add(each.id.ToString(), res.catalogDownloadUri.uri);
            })
        );

        foreach (var each in jsonpathdic)
        {
            UMAAssetIndexer.Instance.changeAddressableName = each.Key;

            List<string> keys = new List<string>();

            var content = await Addressables.LoadContentCatalogAsync(each.Value, true).Task;

            keys.AddRange(content.Keys.Select(s => s.ToString()));

            await foreach (var _ in LocationLoad(each.Value, keys))
            {

            }
        }

        completionSource.TrySetResult();
    }

    private async IAsyncEnumerable<AsyncOperationHandle<IList<UnityEngine.Object>>> LocationLoad(string path, IEnumerable<object> Keys)
    {
        var locations = Addressables.LoadResourceLocationsAsync(Keys, Addressables.MergeMode.Union);
        await locations.Task;

        var containLocationKey = locations.Result.Select((location) => location.PrimaryKey).Where((loc) => Keys.Contains(loc));

        var recipeOp = UMAAssetIndexer.Instance.LoadLabelList(containLocationKey.ToList<string>(), true);
        await recipeOp.Task;

        if (recipeOp.Result != null)
        {
            bundleCacheDic.Add(path, recipeOp);
        }

        recipeOp.Completed += (obj) =>
        {
            Recipes_Loaded(obj.Result, path);
        };

        yield return recipeOp;
    }

    public async UniTask UpdateCatalogChecked()
    {
        var updated = await UpdateCatalog();
        addressableUpdateComplete.OnNext(updated);
    }

    private async UniTask<bool> UpdateCatalog()
    {
        var completionSource = new UniTaskCompletionSource();

        List<string> updateCatalogPaths = new List<string>();

        AsyncOperationHandle<List<string>> checkForUpdateHandle = Addressables.CheckForCatalogUpdates();
        checkForUpdateHandle.Completed += op =>
        {
            updateCatalogPaths.AddRange(op.Result);
        };

        await checkForUpdateHandle.Task;

        if (updateCatalogPaths.Count > 0)
        {
            foreach (var path in updateCatalogPaths)
            {
                foreach (var cache in bundleCacheDic)
                {
                    if (path.Equals(cache.Key))
                    {
                        foreach (var item in cache.Value.Result)
                        {
                            UMAAssetIndexer.Instance.RemoveRecipe(item);
                        }
                        UMAAssetIndexer.Instance.Unload(cache.Value);

                        break;
                    }
                }
                bundleCacheDic.Remove(path);
            }

            var update = await Addressables.UpdateCatalogs(updateCatalogPaths).Task;

            foreach (var up in update)
            {
                await foreach (var _ in LocationLoad(up.LocatorId, up.Keys))
                {

                }
            }
        }
        else
        {
            return false;
        }

        return completionSource.TrySetResult();
    }
    private void Recipes_Loaded(AsyncOperationHandle<IList<UnityEngine.Object>> obj)
    {
        var uniqueObject = new HashSet<UnityEngine.Object>();

        foreach (var ob in obj.Result)
        {
            if (!uniqueObject.Contains(ob))
            {
                uniqueObject.Add(ob);
            }
        }

        foreach (var Each in uniqueObject)
        {
            if (Each != null && !Each.name.ToLower().Contains(ExceptionKeyword.placeholder.ToString()))
            {
                switch (Each)
                {
                    case UMAWardrobeRecipe umaWardrobeRecipe:
                        UMAWardrobeRecipe.Add(umaWardrobeRecipe);
                        break;
                    case OverlayDataAsset overlayDataAsset:
                        OverlayDataAsset.Add(overlayDataAsset);
                        UMAAssetIndexer.Instance.ProcessNewItem(Each, true, true);
                        break;
                    case UMATextRecipe umaTextRecipe:
                        UMATextRecipe.Add(umaTextRecipe);
                        break;
                    case RaceData raceData:
                        RaceData.Add(raceData);
                        break;
                    case UmaTPose umaTPose:
                        UmaTPose.Add(umaTPose);
                        break;
                    case DynamicUMADnaAsset dynamicUMADnaAsset:
                        DynamicUMADnaAsset.Add(dynamicUMADnaAsset);
                        break;
                    case DynamicDNAConverterController dynamicDNAConverterController:
                        DynamicDNAConverterController.Add(dynamicDNAConverterController);
                        break;
                    case Texture2D texture2D:
                        Texture2D.Add(texture2D);
                        break;
                    default:
                        SlotData.Add(Each);
                        UMAAssetIndexer.Instance.ProcessNewItem(Each, true, true);
                        break;
                }
            }
        }

        Debug.Log($"Recipes_Loaded {obj.Result.Count}");

        AddressableLoadComplete();
    }

    private void Recipes_Loaded(IList<UnityEngine.Object> obj, string path)
    {
        var uniqueObject = new HashSet<UnityEngine.Object>();

        if (obj == null)
        {
            return;
        }

        foreach (var ob in obj)
        {
            if (!uniqueObject.Contains(ob))
            {
                uniqueObject.Add(ob);
            }
        }

        foreach (var Each in uniqueObject)
        {
            if (Each != null && !Each.name.ToLower().Contains(ExceptionKeyword.placeholder.ToString()))
            {
                switch (Each)
                {
                    case UMAWardrobeRecipe umaWardrobeRecipe:
                        UMAWardrobeRecipe.Add(umaWardrobeRecipe);
                        break;
                    case OverlayDataAsset overlayDataAsset:
                        OverlayDataAsset.Add(overlayDataAsset);
                        UMAAssetIndexer.Instance.ProcessNewItem(Each, true, true);
                        break;
                    case UMATextRecipe umaTextRecipe:
                        UMATextRecipe.Add(umaTextRecipe);
                        break;
                    case RaceData raceData:
                        RaceData.Add(raceData);
                        break;
                    case UmaTPose umaTPose:
                        UmaTPose.Add(umaTPose);
                        break;
                    case DynamicUMADnaAsset dynamicUMADnaAsset:
                        DynamicUMADnaAsset.Add(dynamicUMADnaAsset);
                        break;
                    case DynamicDNAConverterController dynamicDNAConverterController:
                        DynamicDNAConverterController.Add(dynamicDNAConverterController);
                        break;
                    case Texture2D texture2D:
                        Texture2D.Add(texture2D);
                        break;
                    case SlotDataAsset slotdata:
                        SlotData.Add(slotdata);
                        var temp = path.Split('/');
                        if (temp.Length >= 4) resAppIdDict[slotdata.slotName] = path.Split('/')[4];

                        UMAAssetIndexer.Instance.ProcessNewItem(Each, true, true);
                        break;
                }
            }
        }
        Debug.Log($"Recipes_Loaded {obj.Count}");

    }

    private void AddressableLoadComplete()
    {
        var defaultWardrobe = new Dictionary<string, UMATextRecipe>();

        foreach (var recipe in UMAWardrobeRecipe)
        {
            //recipe.Load(SlotData[0]);
            if (!defaultWardrobe.ContainsKey(recipe.wardrobeSlot))
            {
                var checkId = resAppIdDict[recipe.name];
                foreach (var id in avatarResourceManager.GetVisibleResAppIds())
                {
                    if (checkId.Equals(id))
                    {
                        defaultWardrobe.Add(recipe.wardrobeSlot, recipe);
                        break;
                    }
                }
            }
        }

        Debug.Log($"AddressableLoadComplete ");
        addressableloadComplete.OnNext(defaultWardrobe.Values.ToList());
    }

    private void SetHeader(UnityWebRequest unityWebRequest)
    {
        Debug.Log($"ModifyWebRequest Before {unityWebRequest.uri}");
        unityWebRequest.uri = new Uri(unityWebRequest.uri.ToString());
        unityWebRequest.SetRequestHeader("token", $"Bearer {TokenRepo.Instance.GetToken().accessToken}");
        Debug.Log($"ModifyWebRequest end {unityWebRequest}");
    }

    private void AddressableException()
    {
        ResourceManager.ExceptionHandler = (handle, exception) =>
        {
            //Debug.Log(exception);
        };
    }

    public Dictionary<string, string> GetResAppIdDict()
    {
        return resAppIdDict;
    }
#endif
}
