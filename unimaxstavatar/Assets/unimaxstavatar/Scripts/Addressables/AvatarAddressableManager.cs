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
    private List<UMAWardrobeRecipe> Res = new();

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
    private AvatarResourceManager avatarResourceManager;

    private void Start()
    {
#if UMA_ADDRESSABLES
        /*
            AsyncInit();
            AddressableException();
        */
        SetAvatarDataManager();
        var catalogJsonPathList = GetCatalogJsonPathList();

        Addressables.WebRequestOverride += SetHeader;
        AsyncInit(catalogJsonPathList, true);
        AddressableException();
#else
        addressableloadComplete.OnNext(new List<UMATextRecipe>());
#endif
    }

    private string GetCatalogJsonPath()
    {
        string result = "";

        var o = GameObject.Find("AvatarDataManager");
        if (o != null)
        {
            var avatarResourceManager = o.GetComponent<AvatarResourceManager>();
            result = avatarResourceManager.GetCatalogJsonPath();
        }
        else
        {
            ResourceSettingSO resourceSettingSO = ResourceSettingSO.Instance;
            result = $"{resourceSettingSO.BaseUrl}{resourceSettingSO.Container}{resourceSettingSO.Platform}/{resourceSettingSO.CatalogJsonFileName}{resourceSettingSO.Ext}";
        }
        return result;
    }

    private void SetAvatarDataManager()
    {
        var o = GameObject.Find("AvatarDataManager");
        if (o != null)
        {
            avatarResourceManager = o.GetComponent<AvatarResourceManager>();
        }
    }

    private List<ResourceMeta> GetCatalogJsonPathList()
    {
        return avatarResourceManager.GetCatalogJsonMetaList();
    }
    private List<ContainMeta> GetUmaIdList()
    {
        return avatarResourceManager.GetUmaIdList();
    }

#if UMA_ADDRESSABLES
    /*public async UniTask LoadBundle(string bundlePath)
    {
        string uri = bundlePath;
        UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(uri);
        SetHeader(request);

        await request.SendWebRequest();

        AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);

        UMAAssetIndexer.Instance.AddFromAssetBundle(bundle);
        var assets = bundle.LoadAllAssets();

        foreach (var Each in assets)
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

        AddressableLoadComplete();
    }*/

    private async void AsyncInit(List<ResourceMeta> catalogJsonMetaList, bool isPublicResourceLoad = false)
    {
        List<string> keys = new List<string>();
        var labels = Enum.GetNames(typeof(Avatarlabel));

        Debug.Log($"[AvatarAddressableManager] AsyncInit catalogJsonMetaList : {catalogJsonMetaList.Count}");

        if (catalogJsonMetaList.Count > 0)
        {
            catalogJsonMetaList.ForEach((item) =>
            {
                Debug.Log($"[AvatarAddressableManager] catalogJsonPath : {item.dataUrl}");
            });

            var temp = catalogJsonMetaList.Distinct().ToList();

            foreach (var each in temp)
            {
                Debug.Log($"[AvatarAddressableManager] catalogJson path : {each.dataUrl}");

                var content = await Addressables.LoadContentCatalogAsync(each.dataUrl).Task;
                keys.AddRange(labels.Where(s => content.Keys.Contains(s)).ToList());
            }
        }

        if (isPublicResourceLoad)
        {
            var catalogJsonPath = GetCatalogJsonPath();
            var publicContent = await Addressables.LoadContentCatalogAsync(catalogJsonPath).Task;
            keys.AddRange(labels.Where(s => publicContent.Keys.Contains(s)).ToList());
        }

        //UMAAssetIndexer.Instance.SetMaxstUmaIdList(GetUmaIdList());

        if (keys.Count > 0) {
            var recipeOp = UMAAssetIndexer.Instance.LoadLabelList(keys, true);
            recipeOp.Completed += (obj) =>
            {
                Recipes_Loaded(obj);
            };

            await recipeOp.Task;
        }
    }

    private async void AsyncInit()
    {
        List<string> keys = new List<string>();

        var catalogJsonPath = GetCatalogJsonPath();
        //Debug.Log($"catalogJsonPath : {catalogJsonPath}");

        Addressables.WebRequestOverride += SetHeader;

        var content = await Addressables.LoadContentCatalogAsync(catalogJsonPath).Task;
        //var content = await Addressables.LoadContentCatalogAsync(catalogJsonPath, true).Task;

        keys.AddRange(content.Keys.Select(s => s.ToString()));

        var recipeOp = UMAAssetIndexer.Instance.LoadLabelList(keys, true);
        recipeOp.Completed += (obj) =>
        {
            Recipes_Loaded(obj);
        };

        await recipeOp.Task;
    }

    public void TestButtonClick(int i)
    {
        UpdateCatalogChecked();
    }

    private async void UpdateCatalogChecked()
    {
        List<string> updateCatalogPaths = new List<string>();

        AsyncOperationHandle<List<string>> checkForUpdateHandle = Addressables.CheckForCatalogUpdates();
        checkForUpdateHandle.Completed += op =>
        {
            updateCatalogPaths.AddRange(op.Result);
        };

        await checkForUpdateHandle.Task;

        List<string> keys = new List<string>();

        if (updateCatalogPaths.Count > 0)
        {
            foreach(var path in updateCatalogPaths)
            {
                Debug.Log(path);
            }
        }
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

        //var RaceName = RaceData[Random.Range(0, RaceData.Count)].raceName;
        //Debug.Log($"[AddressablesManager] Load Avatar RaceName : {RaceName}");

        //Res = UMAWardrobeRecipe
        //    .Where(item => item.compatibleRaces.Any(compatibleRace => compatibleRace.Equals(RaceName)))
        //    .ToList();

        AddressableLoadComplete();
    }

    private void AddressableLoadComplete()
    {
        var defaultWardrobe = new Dictionary<string, UMATextRecipe>();

        foreach (var recipe in UMAWardrobeRecipe)
        {
            //recipe.Load(SlotData[0]);
            if (!defaultWardrobe.ContainsKey(recipe.wardrobeSlot))
            {
                defaultWardrobe.Add(recipe.wardrobeSlot, recipe);
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
#endif
}
