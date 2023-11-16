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

    public List<UMAWardrobeRecipe> GetAddressableUmaRecipe { get { return UMAWardrobeRecipe; }}
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
        bool isResourceFullLoad = avatarResourceManager.IsResourceFullLoad();

        var avatarResources = GetAvatarResources();
        var saveAvatarResources = GetSaveAvatarResources();
        var publicResources = GetPublicResources();
        var avatarRes = SetAvatarRseources(saveAvatarResources, avatarResources, publicResources);

        Addressables.WebRequestOverride += SetHeader;

        if (isResourceFullLoad)
        {
            await AsyncInitTask(avatarRes);
        }
        else {
            await DefulatRenderResourcesLoad();
        }
        
        AddressableLoadComplete();
    }

    private async UniTask DefulatRenderResourcesLoad()
    {
        var saveResouces = GetSaveAvatarResources();
        var defaultResource = GetPublicResources().Any(list => list.Value.Count > 0) ? 
                                GetPublicResources() : GetAvatarResources();
        Dictionary<string, AvatarResource> defaultRes = new Dictionary<string, AvatarResource>();

        if (saveResouces.Count > 0)
        {
            await AsyncInitTask(SetAvatarRseources(saveResouces));
        }
        else
        {
            foreach(var res in defaultResource)
            {
                if (!defaultRes.ContainsKey(res.Key.ToString()) && res.Value.Count > 0)
                {
                    defaultRes.Add(res.Key.ToString(), res.Value[0]);
                }
            }

            await AsyncInitTask(defaultRes.Values.ToList());
        }
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

    public async UniTask<IList<UnityEngine.Object>> AsyncEachTask(AvatarResource resouce)
    {
        var completionSource = new UniTaskCompletionSource<IList<UnityEngine.Object>>();

        UMAAssetIndexer.Instance.changeAddressableName = resouce.id.ToString();

        foreach(var each in resouce.resources)
        {
            var content = await Addressables.LoadContentCatalogAsync(each.catalogDownloadUri.uri, true).Task;

            List<string> keys = new List<string>();

            keys.AddRange(content.Keys.Select(s => s.ToString()));

            await foreach (var _ in LocationLoad(each.catalogDownloadUri.uri, keys))
            {
                completionSource.TrySetResult(_.Result);
            }
        }

        return await completionSource.Task;
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
            if (!bundleCacheDic.ContainsKey(path))
            {
                bundleCacheDic.Add(path, recipeOp);
            }
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
                        AddCheckList(UMAWardrobeRecipe, umaWardrobeRecipe);
                        break;
                    case OverlayDataAsset overlayDataAsset:
                        AddCheckList(OverlayDataAsset, overlayDataAsset);
                        UMAAssetIndexer.Instance.ProcessNewItem(Each, true, true);
                        break;
                    case UMATextRecipe umaTextRecipe:
                        AddCheckList(UMATextRecipe, umaTextRecipe);
                        break;
                    case RaceData raceData:
                        AddCheckList(RaceData, raceData);
                        break;
                    case UmaTPose umaTPose:
                        AddCheckList(UmaTPose, umaTPose);
                        break;
                    case DynamicUMADnaAsset dynamicUMADnaAsset:
                        AddCheckList(DynamicUMADnaAsset, dynamicUMADnaAsset);
                        break;
                    case DynamicDNAConverterController dynamicDNAConverterController:
                        AddCheckList(DynamicDNAConverterController, dynamicDNAConverterController);
                        break;
                    case Texture2D texture2D:
                        AddCheckList(Texture2D, texture2D);
                        break;
                    case SlotDataAsset slotdata:
                        AddCheckList(SlotData, slotdata);
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
                        AddCheckList(UMAWardrobeRecipe, umaWardrobeRecipe);
                        break;
                    case OverlayDataAsset overlayDataAsset:
                        AddCheckList(OverlayDataAsset, overlayDataAsset);
                        UMAAssetIndexer.Instance.ProcessNewItem(Each, true, true);
                        break;
                    case UMATextRecipe umaTextRecipe:
                        AddCheckList(UMATextRecipe, umaTextRecipe);
                        break;
                    case RaceData raceData:
                        AddCheckList(RaceData, raceData);
                        break;
                    case UmaTPose umaTPose:
                        AddCheckList(UmaTPose, umaTPose);
                        break;
                    case DynamicUMADnaAsset dynamicUMADnaAsset:
                        AddCheckList(DynamicUMADnaAsset, dynamicUMADnaAsset);
                        break;
                    case DynamicDNAConverterController dynamicDNAConverterController:
                        AddCheckList(DynamicDNAConverterController, dynamicDNAConverterController);
                        break;
                    case Texture2D texture2D:
                        AddCheckList(Texture2D, texture2D);
                        break;
                    case SlotDataAsset slotdata:
                        AddCheckList(SlotData, slotdata);
                        SetRestAppIdDict(slotdata.slotName, path);

                        UMAAssetIndexer.Instance.ProcessNewItem(Each, true, true);
                        break;
                }
            }
        }
        Debug.Log($"Recipes_Loaded {obj.Count}");
    }

    private void AddCheckList<T>(List<T> list, T item)
    {
        if (!list.Contains(item))
        {
            list.Add(item);
        }
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

    public void SetRestAppIdDict(string slotName, string path)
    {
        var temp = path.Split('/');
        if (temp.Length >= 4) resAppIdDict[slotName] = path.Split('/')[4];
    }
#endif
}
