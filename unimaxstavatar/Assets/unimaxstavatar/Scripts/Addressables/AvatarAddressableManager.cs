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

    private void Start()
    {
#if UMA_ADDRESSABLES
        AsyncInit();
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
            var avatarDataManager = o.GetComponent<AvatarResourceManager>();
            result = avatarDataManager.GetCatalogJsonPath();
        }
        else
        {
            ResourceSettingSO resourceSettingSO = ResourceSettingSO.Instance;
            result = $"{resourceSettingSO.BaseUrl}{resourceSettingSO.Container}{resourceSettingSO.Platform}/{resourceSettingSO.CatalogJsonFileName}{resourceSettingSO.Ext}";
        }
        return result;
    }

#if UMA_ADDRESSABLES
    private async void AsyncInit()
    {
        List<string> keys = new List<string>();

        var catalogJsonPath = GetCatalogJsonPath();
        Debug.Log($"catalogJsonPath : {catalogJsonPath}");

        Addressables.WebRequestOverride += SetHeader;

        var content = await Addressables.LoadContentCatalogAsync(catalogJsonPath).Task;

        var labels = System.Enum.GetNames(typeof(Avatarlabel));
        keys.AddRange(labels.Where(s => content.Keys.Contains(s)).ToList());

        var recipeOp = UMAAssetIndexer.Instance.LoadLabelList(keys, true);
        await recipeOp.Task;
        recipeOp.Completed += (obj) =>
        {
            Recipes_Loaded(obj);
        };
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
#endif
}
