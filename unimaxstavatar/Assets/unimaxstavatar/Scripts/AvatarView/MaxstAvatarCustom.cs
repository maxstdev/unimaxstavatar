using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UMA;
using UMA.CharacterSystem;
using UniRx;
using UnityEngine;

namespace Maxst.Avatar
{
    public interface ICharacterChange
    {
        void Update();
    }

    public class MaxstAvatarCustom : InjectorBehaviour
    {
        public const string PREFS_MAXST_AVATAR_KEY = "prefs_maxst_avatar_key";

        [DI(DIScope.singleton)] protected AvatarCustomViewModel avatarCustomViewModel { get; }

        private UMAAssetData assetData;
        private UndoRedoManager undoRedoManager;

        [SerializeField] private GameObject ResultGradientView;
        [SerializeField] private GameObject DefaultGradientView;

        [SerializeField] private WardrobeSlotConvertSO wardrobeConvertSO;

        [SerializeField] private AvatarControlPanelButton avatarControlPanelButton;
        [SerializeField] private CategoryAreaUIview categoryAreaUI;
        [SerializeField] private ColorAreaUIView colorAreaUI;
        [SerializeField] private UMATextRecipe[] TestReceipe;

        [SerializeField] private AvatarAreaUIView avatarAreaUI;
        [SerializeField] private List<UMATextRecipe> wearingRecipeList = new List<UMATextRecipe>();
        [SerializeField] private List<ColorPreset> presetList = new List<ColorPreset>();

        [SerializeField] private ResultArea resultArea;
        [SerializeField] private ActionList actionList;
        [SerializeField] private GameObject[] VisibleObjects;

        [SerializeField] private AssetAreaUIView assetAreaUI;

        [SerializeField] private List<UMATextRecipe> defaultTextRecipe;

        public DynamicCharacterAvatar Avatar;
        private AvatarAddressableManager addressableloader;

        private ViewType beforeViewType = ViewType.None;

        void Awake()
        {
            addressableloader = GetComponent<AvatarAddressableManager>();
            addressableloader.addressableloadComplete
                .Subscribe(list =>
                {
                    defaultTextRecipe.AddRange(list);
                    InitAvatarCustom();
                });
        }

        private string GetSaveRecipe() {
            var o = GameObject.Find("AvatarDataManager");
            if (o != null)
            {
                var avatarDataManager = o.GetComponent<AvatarResourceManager>();
                return avatarDataManager.GetSaveRecipe();
            }
            return null;
        }

        public void InitAvatarCustom()
        {
            SetDefaultAvatar();
            Avatar.SetDefaultTextRecipe(defaultTextRecipe, GetSaveRecipe());

            assetData = new UMAAssetData();
            undoRedoManager = new UndoRedoManager();
            undoRedoManager.SetExecuteCommandAction(() =>
            {
                UndoBtnStateUI();
            });

            avatarControlPanelButton.statecheck
                .Subscribe(state =>
                {
                    CategoryUIChange(state);
                    avatarCustomViewModel.ViewTypeLiveEvent.Post(state);
                })
                .AddTo(this);

            categoryAreaUI.categoryWardrobeslot
                .Where(value => !string.IsNullOrEmpty(value))
                .Subscribe(value =>
                {
                    //ColorUiInitColor(value);
                    GetAvatarWardrobeSlotRecipe(value);

                    LoadScrollerDataFromAsset(value);

                })
                .AddTo(this);

            categoryAreaUI.categoryChangeViewType
                .DistinctUntilChanged()
                .Subscribe((viewType) => OnAdjust(viewType)
                )
                .AddTo(this);

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
                   undoRedoManager.ExecuteCommand(new SetUpAvatarColor(Avatar,
                       categoryAreaUI.categoryWardrobeslot.Value, value, presetList));
               })
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

            avatarAreaUI.OnClickUndo += UndoAvatar;
            avatarAreaUI.OnClickClose += CloseAvatar;
            avatarAreaUI.OnClickViedo += VideoAvatar;
            avatarAreaUI.OnClickNext += NextAvatar;
            avatarAreaUI.OnClickRefresh += RefreshAvatar;

            resultArea.OnClickAgain += CloseAvatar;
            resultArea.OnClickStart += NextScene;

            assetAreaUI.slotname
                .Subscribe(name =>
                {
                    DressUpAvatar(assetData.GetTextRecipe(name));
                });
        }

        private void SetDefaultAvatar()
        {
            defaultTextRecipe.ForEach(recipe =>
            {
                Avatar.SetSlot(recipe);
            });
            Avatar.BuildCharacter(true);
        }

        private void OnAdjust(ViewType viewType)
        {
            Debug.Log($"[MaxstAvatarCustom] OnAdjust : {viewType}");
            switch (viewType)
            {
                case ViewType.Face_Hair:
                case ViewType.Face_Eyebrows:
                    actionList.ResetActionList();
                    SetActives(false, actionList.gameObject, resultArea.gameObject);
                    SetActives(true, VisibleObjects);
                    avatarAreaUI.SetActiveCloseImg(true);
                    avatarAreaUI.SetSpriteCloseImg();
                    colorAreaUI.transform.parent.gameObject.SetActive(true);
                    SetBackgroundGradient(true);
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

        private void SetBackgroundGradient(bool isDefault)
        {
            DefaultGradientView.SetActive(isDefault);
            ResultGradientView.SetActive(!isDefault);
        }

        private void SetActives(bool isActive, params GameObject[] objs)
        {
            foreach (GameObject obj in objs) { obj.SetActive(isActive); }
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
            Action onclick = null;
            bool firstSlot = false;

            assetData.GetWardrobeSlotList(Avatar.activeRace.name);

            Avatar.activeRace.data.wardrobeSlots.ForEach(eachSlot =>
            {
                if (state == wardrobeConvertSO.GetFaceBodyValue(eachSlot))
                {
                    var data = wardrobeConvertSO.GetEachSlotData(eachSlot);

                    categoryAreaUI.CreateSubject(eachSlot, data.koreanName.ToString(),
                        data.defaultIcon, data.selectIcon, out onclick);

                    if (!firstSlot)
                    {
                        firstSlot = true;
                        observer.OnNext(onclick);
                    }
                }
            });

            yield return null;
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
                Debug.Log("[MaxstAvatarCustom] End Scene!!");
            }
        }

        public async void SaveRecipe()
        {
            if (Avatar != null)
            {
                var recipeString = Avatar.MaxstDoSave(false);
                Debug.Log(recipeString);

                var o = GameObject.Find("AvatarDataManager");
                if (o != null)
                {
                    var AvatarSample = o.GetComponent<AvatarResourceManager>();
                    await AvatarSample.AvatarSaveDataListener(recipeString);
                }
            }
        }

        private void NextScene()
        {
            SaveRecipe();
            Debug.Log("[MaxstAvatarCustom] Next Scene!!");
        }

        private void VideoAvatar()
        {
            beforeViewType = categoryAreaUI.categoryChangeViewType.Value;
            categoryAreaUI.categoryChangeViewType.Value = ViewType.Animation;
        }

        private void NextAvatar()
        {
            beforeViewType = categoryAreaUI.categoryChangeViewType.Value;
            categoryAreaUI.categoryChangeViewType.Value = ViewType.Result;
        }

        private void RefreshAvatar()
        {
            undoRedoManager.ExecuteCommand(new SetUpAvatarSlot(Avatar, null, wearingRecipeList));
            InitializeSelectUI();
        }

        private void PreviewAvatarColor(Color value)
        {
            Avatar.SetColor(categoryAreaUI.categoryWardrobeslot.Value, value);
            Avatar.UpdateColors(true);
        }

        private UMATextRecipe GetAvatarWardrobeSlotRecipe(string slot)
        {
            UMATextRecipe recipe = null;

            foreach (var key in Avatar.WardrobeRecipes)
            {
                if (key.Value.wardrobeSlot == slot)
                {
                    recipe = key.Value;
                }
            }

            return recipe;
        }

        public void DressUpAvatar(UMATextRecipe recipe)
        {
            undoRedoManager.ExecuteCommand(new SetUpAvatarSlot(Avatar, recipe, wearingRecipeList));
        }

        private void UndoAvatar()
        {
            undoRedoManager.Undo();
            UndoSelectUI();
        }

        public void RedoAvatar()
        {
            undoRedoManager.Redo();
        }

        private void LoadScrollerDataFromAsset(string value)
        {
            var list = assetData.GetAssetData(value);

            assetAreaUI.DeleteAllItem();

            list.ForEach(slot =>
            {
                var isSelected = false;
                foreach (var each in Avatar.WardrobeRecipes)
                {
                    if (slot.recipeString.Equals(each.Value.recipeString))
                    {
                        isSelected = true;
                        break;
                    }
                }
                assetAreaUI.CreateAssetItem(slot.slotName, slot.thumbnail, isSelected);
            });
        }

        private void InitializeSelectUI()
        {
            string slotValue = categoryAreaUI.categoryWardrobeslot.Value;

            if (!string.IsNullOrEmpty(slotValue))
            {
                LoadScrollerDataFromAsset(slotValue);
            }

            RefreshBtnStateUI();
        }

        private void UndoSelectUI()
        {
            LoadScrollerDataFromAsset(categoryAreaUI.categoryWardrobeslot.Value);
            UndoBtnStateUI();
        }

        private void UndoBtnStateUI()
        {
            var isUndoBtnActive = undoRedoManager.UndoCount() != 0;
            avatarAreaUI.SetUndoBtnUI(isUndoBtnActive);
            avatarAreaUI.SetRefreshBtnUI(isUndoBtnActive);
        }

        private void RefreshBtnStateUI()
        {
            undoRedoManager.ClearUndoStack();
            avatarAreaUI.SetRefreshBtnUI(false);

            var isUndoBtnActive = undoRedoManager.UndoCount() != 0;
            avatarAreaUI.SetUndoBtnUI(isUndoBtnActive);
        }
    }
}