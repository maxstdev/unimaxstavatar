using Cysharp.Threading.Tasks;
using Maxst.Resource;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UMA;
using UMA.CharacterSystem;
using UniRx;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Maxst.Avatar
{
    public class AvatarSceneUIBehaviour : InjectorBehaviour
    {
        [DI(DIScope.singleton)] protected AvatarCustomViewModel avatarCustomViewModel { get; }

        private AvatarResourceRepo avatarResourceRepo;
        private UMAAssetData assetData;

        private ViewType beforeViewType = ViewType.None;

        [Space(16)]
        [Header("Main Avatar")]
        [SerializeField] private DynamicCharacterAvatar Avatar;

        [Space(16)]
        [Header("UnderWare")]
        [SerializeField] private UMATextRecipe underWearTop;
        [SerializeField] private UMATextRecipe underWearBottom;

        [Space(16)]
        [Header("UI Component")]
        [SerializeField] private CategoryAreaUIview categoryAreaUI;
        [SerializeField] private AvatarControlPanelButton avatarControlPanelButton;
        [SerializeField] private AssetAreaUIView assetAreaUI;
        [SerializeField] private ColorAreaUIView colorAreaUI;
        [SerializeField] private ResultArea resultArea;
        [SerializeField] private ActionList actionList;
        [SerializeField] private GameObject[] VisibleObjects;
        [SerializeField] private AvatarAreaUIView avatarAreaUI;
        [SerializeField] private GameObject ResultGradientView;
        [SerializeField] private GameObject DefaultGradientView;

        [Space(16)]
        [Header("SO")]
        [SerializeField] private WardrobeSlotConvertSO wardrobeConvertSO;

        private void Awake()
        {
            avatarResourceRepo = AvatarResourceRepo.Instance;
            avatarResourceRepo.Init();

            assetData = new UMAAssetData();

            avatarControlPanelButton.statecheck
                .Subscribe(state =>
                {
                    CategoryUIChange(state);
                    categoryAreaUI.categoryChangeViewType.Value = state;
                    avatarCustomViewModel.ViewTypeLiveEvent.Post(state);
                })
                .AddTo(this);

            categoryAreaUI.categoryWardrobeslot
                .Where(value => !string.IsNullOrEmpty(value))
                .Subscribe(value => LoadScrollDataFromResourceServer(value))
                .AddTo(this);

            assetAreaUI.slotname
                .Subscribe(name =>
                {
                    Debug.Log($"assetAreaUI.slotname : {name}");
                    SetAvatarHistory(Avatar);
                    SelectSlotUI(name);
                })
                .AddTo(this);

            categoryAreaUI.categoryChangeViewType
                .DistinctUntilChanged()
                .Subscribe((viewType) => OnAdjust(viewType)
                )
                .AddTo(this);

            colorAreaUI.getDefaultColor = () =>
            {
                Color initcolor = new Color();
                var currentslot = categoryAreaUI.categoryWardrobeslot.Value;

                Avatar.characterColors.Colors.ForEach(color =>
                {
                    if (currentslot == color.name)
                    {
                        initcolor = Avatar.GetColor(currentslot).channelMask[0];
                    }
                });

                return initcolor;
            };

            colorAreaUI.previewcolor
                .Where(color => color.a > 0f)
                .Subscribe(value =>
                {
                    PreviewAvatarColor(value);
                });

            colorAreaUI.setupColorpreset
               .Where(preset => preset != null)
               .Subscribe(value =>
               {
                   PreviewAvatarColor(value.selectColor.Value);
               })
               .AddTo(this);

            avatarAreaUI.OnClickUndo += UndoAvatar;
            avatarAreaUI.OnClickClose += CloseAvatar;
            avatarAreaUI.OnClickViedo += VideoAvatar;
            avatarAreaUI.OnClickNext += NextAvatar;
            avatarAreaUI.OnClickRefresh += RefreshAvatar;

            resultArea.OnClickAgain += CloseAvatar;
            resultArea.OnClickStart += NextScene;
        }

        private void OnEnable()
        {
            avatarResourceRepo.loadSlotListEvent.AddObserver(this, LoadSlotList);
            avatarResourceRepo.loadAvatarResourceAddressableEvent.AddObserver(this, LoadAvatarResourceAddressable);

            avatarResourceRepo.loadUserAvatarRecipeEvent.AddObserver(this, LoadUserAvatarRecipe);
            avatarResourceRepo.loadUserAvatarResourceDoneEvent.AddObserver(this, LoadUserAvatarResourceDone);

            avatarResourceRepo.resourceDownloadStatusEvent.AddObserver(this, ResourceDownloadStatus);
        }

        private void ResourceDownloadStatus(AvatarResource resource, DownloadStatus status)
        {
            if (resource == null) return;

            Debug.Log($"[AvatarSceneUIBehaviour] ResourceDownloadStatus resource : {resource.id}");
            Debug.Log($"[AvatarSceneUIBehaviour] ResourceDownloadStatus TotalBytes : {status.TotalBytes}");
            Debug.Log($"[AvatarSceneUIBehaviour] ResourceDownloadStatus DownloadedBytes : {status.DownloadedBytes}");
            Debug.Log($"[AvatarSceneUIBehaviour] ResourceDownloadStatus status Percent : {status.Percent}");
        }

        void Start()
        {
            var platform = ResourceSettingSO.Instance.Platform;
            avatarResourceRepo.LoadSaveRecipeExtensions(platform).Forget();
        }

        void Update()
        {

        }

        private void OnDisable()
        {
            avatarResourceRepo.loadSlotListEvent.RemoveAllObserver(this);
            avatarResourceRepo.loadAvatarResourceAddressableEvent.RemoveAllObserver(this);

            avatarResourceRepo.loadUserAvatarRecipeEvent.RemoveAllObserver(this);
            avatarResourceRepo.loadUserAvatarResourceDoneEvent.RemoveAllObserver(this);

            avatarResourceRepo.resourceDownloadStatusEvent.RemoveAllObserver(this);
        }

        private void UndoAvatar()
        {
            SetAvatarUI(Avatar);
        }

        private void UndoBtnStateUI()
        {
            var isUndoBtnActive = avatarResourceRepo.AvatarHistoryCount() != 0;
            avatarAreaUI.SetUndoBtnUI(isUndoBtnActive);
            avatarAreaUI.SetRefreshBtnUI(isUndoBtnActive);
        }

        private void RefreshAvatar()
        {
            avatarResourceRepo.AvatarHistoryClear();
            SetAvatarUI(Avatar);
        }

        private void SetAvatarUI(DynamicCharacterAvatar avatar) {
            var recipe = avatarResourceRepo.GetAvatarHistory();
            if (recipe != null)
            {
                avatar.ClearSlots();
                avatar.SetRecipeString(recipe);
                avatar.MaxstDoLoad();
            }
            
            RefreshAssetAreaUI();
            UndoBtnStateUI();
        }

        private void SelectSlotUI(string slotName)
        {
            var currentslot = categoryAreaUI.categoryWardrobeslot.Value;

            if (IsLocalSlot(currentslot))
            {
                Avatar.SetSlot(assetData.GetTextRecipe(slotName));
                SetUnderWear(Avatar);
                Avatar.BuildCharacter(true);
            }
            else
            {
                var item = avatarResourceRepo.loadSlotListEvent.Value
                    .SingleOrDefault(item => item.id.ToString().Equals(slotName));

                if (item != null)
                {
                    avatarResourceRepo.LoadAvatarResource(item).Forget();
                }
                else
                {
                    Debug.Log("SlotName is not loaded");
                }
            }
        }

        private void RefreshAssetAreaUI()
        {
            string slot = categoryAreaUI.categoryWardrobeslot.Value;
            Avatar.WardrobeRecipes.TryGetValue(slot, out var recipe);
            if (recipe == null)
            {
                assetAreaUI.InitSelect();
            }
            else
            {
                assetAreaUI.ItemSelectChange(recipe.name);
            }
        }

        private void AvatarDressUp(DynamicCharacterAvatar avatar, UserAvatar userAvatar)
        {
            Avatar.ClearSlots();
            avatar.SetRecipeString(userAvatar.recipeStr);
            avatar.MaxstDoLoad();
            SetUnderWear(avatar);

            RefreshAssetAreaUI();
        }

        private void AvatarDressUp(DynamicCharacterAvatar avatar, params AvatarResource[] avatarResources)
        {
            var temp = new Dictionary<string, UMATextRecipe>(avatar.WardrobeRecipes);

            avatar.ClearSlots();

            var assetData = new UMAAssetData();

            foreach (var each in avatarResources)
            {
                var currentRaceRecipes = UMAAssetIndexer.Instance.GetRecipes(avatar.activeRace.name);

                List<UMATextRecipe> list = currentRaceRecipes[each.subCategory];

                UMATextRecipe textRecipe = list.Find(i => i.name.Equals(each.id.ToString()));

                if (textRecipe != null)
                {
                    var isSetResource = each.subCategory.Equals(Category.Set.ToString());
                    var isChestResource = each.subCategory.Equals(Category.Chest.ToString());
                    var isLegsResource = each.subCategory.Equals(Category.Legs.ToString());

                    if (isSetResource)
                    {
                        temp.Remove(Category.Chest.ToString());
                        temp.Remove(Category.Legs.ToString());
                        temp.Remove(Category.Undertop.ToString());
                        temp.Remove(Category.Underbottom.ToString());
                    }
                    else
                    {
                        if (isChestResource || isLegsResource) temp.Remove(Category.Set.ToString());
                        if (isChestResource)
                        {
                            temp.Remove(Category.Undertop.ToString());
                        }

                        if (isLegsResource)
                        {
                            temp.Remove(Category.Underbottom.ToString());
                        }
                    }

                    temp[each.subCategory] = textRecipe;

                }
            }

            foreach (var key in temp.Keys)
            {
                avatar.SetSlot(temp[key]);
            }

            SetUnderWear(avatar);

            avatar.BuildCharacter(true);

            RefreshAssetAreaUI();
        }

        private void SetUnderWear(DynamicCharacterAvatar avatar)
        {
            avatar.WardrobeRecipes.TryGetValue(Category.Set.ToString(), out var set);
            if (set != null) { return; }

            avatar.WardrobeRecipes.TryGetValue(Category.Chest.ToString(), out var chest);
            avatar.WardrobeRecipes.TryGetValue(Category.Legs.ToString(), out var legs);

            if (chest == null)
            {
                Avatar.SetSlot(underWearTop);
                Avatar.BuildCharacter(true);
            }

            if (legs == null)
            {
                Avatar.SetSlot(underWearBottom);
                Avatar.BuildCharacter(true);
            }
        }

        private void SetAvatarHistory(DynamicCharacterAvatar avatar)
        {
            var recipe = avatar.MaxstDoSave();
            avatarResourceRepo.SetAvatarHistory(recipe);
            UndoBtnStateUI();
        }

        private void LoadUserAvatarResourceDone(UserAvatar userAvatar)
        {
            if (userAvatar == null) return;

            var avatar = userAvatar.avatar;

            AvatarDressUp(avatar, userAvatar);

            LoadAllSlotList();
        }

        private void LoadUserAvatarRecipe(UserAvatar avatarData)
        {
            if (avatarData == null) return;

            if (avatarData.avatar == null)
            {
                avatarData.avatar = Avatar;
            }

            if (avatarData.recipeStr == null)
            {
                LoadAllSlotList();
            }
            else
            {
                avatarResourceRepo.LoadAvatarResource(avatarData, true).Forget();
            }
        }

        private void LoadAvatarResourceAddressable(AvatarResource avatarResource)
        {
            Debug.Log($"Debug LoadAvatarResourceAddressable : {avatarResource}");

            if (avatarResource == null) return;

            AvatarDressUp(Avatar, avatarResource);
        }

        private void LoadSlotList(List<AvatarResource> list)
        {
            categoryAreaUI.categoryWardrobeslot.SetValueAndForceNotify(categoryAreaUI.categoryWardrobeslot.Value);

            if (list.Count != 0 && avatarResourceRepo.loadUserAvatarRecipeEvent.Value.recipeStr == null) {
                avatarResourceRepo.LoadDefaultAvatarResource(list);
            }
        }

        private void LoadAllSlotList()
        {
            string appId = AvatarConstant.PUBLIC_RESOURCE_APPID;
            var platform = ResourceSettingSO.Instance.Platform;

            var list = new List<string>();
            list.Add(appId);

            // add AppId
            // list.Add("appId");

            avatarResourceRepo.LoadAllSlotList(AvatarConstant.MAIN_CATEGORY, platform.ToString(), list).Forget();
        }

        private void LoadScrollDataFromResourceServer(string slot)
        {
            assetAreaUI.DeleteAllItem();

            if (IsLocalSlot(slot))
            {
                assetData.GetWardrobeSlotList(Avatar.activeRace.name);
                var temp = assetData.GetAssetData(slot);

                Avatar.WardrobeRecipes.TryGetValue(slot, out var recipe);
                temp.ForEach(slot =>
                {
                    var isSelect = recipe != null && slot.recipeString.Equals(recipe.recipeString);
                    assetAreaUI.CreateAssetItem(slot.resId, slot.thumbnail, isSelect);
                });
            }
            else
            {
                var list = avatarResourceRepo.loadSlotListEvent.Value;
                var temp = list.Where(item => item.subCategory.Equals(slot)).ToList();
                temp.ForEach(avatarResource =>
                {
                    var slot = new AssetAreaData()
                    {
                        resId = avatarResource.id.ToString(),
                        slotName = avatarResource.id.ToString(),
                        thumbnailpath = null,
                    };

                    Avatar.WardrobeRecipes.TryGetValue(avatarResource.subCategory, out var recipe);
                    var isSelect = recipe != null && slot.resId.Equals(recipe.name);

                    var item = assetAreaUI.CreateAssetItem(slot.resId, isSelect);

                    LoadThumbNails(item, avatarResource);
                });
            }
        }

        private void LoadThumbNails(AssetUIItem item, AvatarResource avatarResource)
        {
            avatarResourceRepo.LoadResourceThumbnail(avatarResource, (thumbnailDownLoadUri) =>
            {
                Debug.Log($"ThumbNail resource id : {avatarResource.id}");
                Debug.Log($"ThumbNail Path : {thumbnailDownLoadUri.uri}");
                item.SetData(thumbnailDownLoadUri.uri);
            }).Forget();
        }

        private void CategoryUIChange(ViewType state)
        {
            categoryAreaUI.DeleteAllSubject();

            Observable.FromCoroutine<Action>((observer) => CategoryUICreate(state, observer))
                    .Subscribe(value =>
                    {
                        value.Invoke();
                    })
                    .AddTo(this);

            categoryAreaUI.categoryChangeViewType.Value = state;
        }

        private IEnumerator CategoryUICreate(ViewType state, IObserver<Action> observer)
        {
            bool firstSlot = false;

            var categoryNames = Enum.GetValues(typeof(Category));
            foreach (var category in categoryNames)
            {
                var eachSlot = category.ToString();

                if (state == wardrobeConvertSO.GetFaceBodyValue(eachSlot))
                {
                    var data = wardrobeConvertSO.GetEachSlotData(eachSlot);

                    Action onclick;
                    categoryAreaUI.CreateSubject(eachSlot, data.koreanName.ToString(),
                        data.defaultIcon, data.selectIcon, out onclick);

                    if (!firstSlot)
                    {
                        firstSlot = true;
                        observer.OnNext(onclick);
                    }
                }
            }

            yield return null;
        }
        private bool IsLocalSlot(string slot)
        {
            return AvatarWardrobeSlot.Face.ToString().Equals(slot);
        }

        private void OnAdjust(ViewType viewType)
        {
            switch (viewType)
            {
                case ViewType.Hair:
                    actionList.ResetActionList();
                    SetActives(false, actionList.gameObject, resultArea.gameObject);
                    SetActives(true, VisibleObjects);
                    avatarAreaUI.SetActiveCloseImg(true);
                    avatarAreaUI.SetSpriteCloseImg();
                    SetBackgroundGradient(true);
                    colorAreaUI.transform.parent.gameObject.SetActive(true);
                    break;
                case ViewType.Face:
                    actionList.ResetActionList();
                    SetActives(false, actionList.gameObject, resultArea.gameObject);
                    SetActives(true, VisibleObjects);
                    avatarAreaUI.SetActiveCloseImg(true);
                    avatarAreaUI.SetSpriteCloseImg();
                    SetBackgroundGradient(true);
                    colorAreaUI.transform.parent.gameObject.SetActive(false);
                    break;
                case ViewType.Body:
                    actionList.ResetActionList();
                    SetActives(false, actionList.gameObject, resultArea.gameObject);
                    SetActives(true, VisibleObjects);
                    avatarAreaUI.SetActiveCloseImg(true);
                    avatarAreaUI.SetSpriteCloseImg();
                    colorAreaUI.transform.parent.gameObject.SetActive(false);
                    SetBackgroundGradient(true);
                    break;
                case ViewType.Animation:
                    SetActives(false, VisibleObjects);
                    SetActives(true, actionList.gameObject);
                    avatarAreaUI.SetSpriteArrowLeft();
                    avatarCustomViewModel.ViewTypeLiveEvent.Post(viewType);
                    colorAreaUI.transform.parent.gameObject.SetActive(false);
                    SetBackgroundGradient(true);
                    break;
                case ViewType.Result:
                    SetActives(false, VisibleObjects);
                    SetActives(true, resultArea.gameObject);
                    avatarAreaUI.SetActiveCloseImg(false);
                    avatarCustomViewModel.ViewTypeLiveEvent.Post(viewType);
                    colorAreaUI.transform.parent.gameObject.SetActive(false);
                    SetBackgroundGradient(false);
                    break;

                default: break;
            }
        }
        private void SetActives(bool isActive, params GameObject[] objs)
        {
            foreach (GameObject obj in objs) { obj.SetActive(isActive); }
        }

        private void SetBackgroundGradient(bool isDefault)
        {
            DefaultGradientView.SetActive(isDefault);
            ResultGradientView.SetActive(!isDefault);
        }

        private void PreviewAvatarColor(Color value)
        {
            var recipe = Avatar.MaxstDoSave();
            avatarResourceRepo.SetAvatarHistory(recipe);
            Avatar.SetColor(categoryAreaUI.categoryWardrobeslot.Value, value);
            Avatar.UpdateColors(true);
            UndoBtnStateUI();
        }

        private void CloseAvatar()
        {
            var viewType = categoryAreaUI.categoryChangeViewType.Value;
            if (viewType == ViewType.Animation || viewType == ViewType.Result)
            {
                categoryAreaUI.categoryChangeViewType.Value = beforeViewType;
                avatarCustomViewModel.ViewTypeLiveEvent.Post(categoryAreaUI.categoryChangeViewType.Value);
                beforeViewType = ViewType.None;
            }
            else
            {
                Debug.Log("End Scene!!");
            }
        }

        private void VideoAvatar()
        {
            beforeViewType = categoryAreaUI.categoryChangeViewType.Value;
            categoryAreaUI.categoryChangeViewType.Value = ViewType.Animation;
        }

        private void NextScene()
        {
            //SaveRecipeExtensions();
            var saveRecipe = avatarResourceRepo.GetSaveRecipeExtensions(Avatar);
            var result = PostSaveRecipeExtensions(saveRecipe);
            Debug.Log($"[MaxstAvatarCustom] Next Scene!! : {result}");
        }

        private async UniTask<string> PostSaveRecipeExtensions(SaveRecipeExtensions saveRecipeExtension)
        {
            return await avatarResourceRepo.PostSaveRecipeExtensions(saveRecipeExtension);
        }

        private void NextAvatar()
        {
            beforeViewType = categoryAreaUI.categoryChangeViewType.Value;
            categoryAreaUI.categoryChangeViewType.Value = ViewType.Result;
        }
    }
}
