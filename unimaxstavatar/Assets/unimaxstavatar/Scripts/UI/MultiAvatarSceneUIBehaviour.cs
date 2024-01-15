using Cysharp.Threading.Tasks;
using Maxst.Passport;
using Maxst.Token;
using System;
using System.Collections.Generic;
using UMA;
using UMA.CharacterSystem;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace Maxst.Avatar
{
    public class MultiAvatarSceneUIBehaviour : MonoBehaviour
    {
        [Space(16)]
        [Header("Avatars")]
        [SerializeField]
        private GameObject goAvatars;
        [SerializeField]
        private DynamicCharacterAvatar userAvatar;
        [SerializeField]
        private DynamicCharacterAvatar slotAvatar;

        [Space(16)]
        [Header("UnderWare")]
        [SerializeField] private UMATextRecipe underWearTop;
        [SerializeField] private UMATextRecipe underWearBottom;


        private AvatarResourceRepo avatarResourceRepo;

        private DynamicCharacterAvatar[] avatars;

        private void Awake()
        {
            Screen.orientation = ScreenOrientation.LandscapeLeft;

            Addressables.WebRequestOverride = SetHeader;
            avatarResourceRepo = AvatarResourceRepo.Instance;
            avatarResourceRepo.SetToken(GetToken());
        }

        private void Start()
        {
            avatars = goAvatars.GetComponentsInChildren<DynamicCharacterAvatar>();
        }

        private void OnEnable()
        {
            avatarResourceRepo.loadSlotListEvent.AddObserver(this, LoadSlotList);
            avatarResourceRepo.loadAvatarResourceAddressableEvent.AddObserver(this, LoadAvatarResourceAddressable);
            avatarResourceRepo.loadUserAvatarResourceDoneEvent.AddObserver(this, LoadUserAvatarResourceDone);
            avatarResourceRepo.loadUserAvatarRecipeEvent.AddObserver(this, LoadUserAvatarRecipe);
            avatarResourceRepo.loadUserAvatarResourceAddressableEvent.AddObserver(this, LoadUserAvatarResourceAddressable);
        }

        private void OnDisable()
        {
            avatarResourceRepo.loadSlotListEvent.RemoveAllObserver(this);
            avatarResourceRepo.loadAvatarResourceAddressableEvent.RemoveAllObserver(this);
            avatarResourceRepo.loadUserAvatarResourceDoneEvent.RemoveAllObserver(this);
            avatarResourceRepo.loadUserAvatarRecipeEvent.RemoveAllObserver(this);
            avatarResourceRepo.loadUserAvatarResourceAddressableEvent.RemoveAllObserver(this);
        }

        private void SetHeader(UnityWebRequest unityWebRequest)
        {
            Debug.Log($"ModifyWebRequest Before {unityWebRequest.uri}");
            unityWebRequest.uri = new Uri(unityWebRequest.uri.ToString());
            unityWebRequest.SetRequestHeader("token", $"Bearer {GetToken().accessToken}");
            Debug.Log($"ModifyWebRequest end {unityWebRequest}");
        }

        public void OnClickAll()
        {
            var platform = ResourceSettingSO.Instance.Platform;

            avatarResourceRepo.LoadSaveRecipeExtensions(platform).Forget();
            avatarResourceRepo.LoadDummyUsers();

            LoadAllSlotList();
        }

        private void LoadAvatarResources()
        {
            avatarResourceRepo.loadSlotListEvent.Value.ForEach((slot) =>
            {
                avatarResourceRepo.LoadAvatarResource(slot).Forget();
            });
        }

        private void LoadSlotList(Category category)
        {
            string appId = AvatarConstant.PUBLIC_RESOURCE_APPID;
            var platform = ResourceSettingSO.Instance.Platform;

            avatarResourceRepo.LoadSlotList(AvatarConstant.MAIN_CATEGORY, category.ToString(), platform.ToString(), appId).Forget();
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

        private void LoadThumbNails()
        {
            avatarResourceRepo.loadSlotListEvent.Value.ForEach(item =>
            {
                var res = avatarResourceRepo.LoadResourceThumbnail(item, (thumbnailDownLoadUri) =>
                {
                    Debug.Log($"ThumbNail resource id : {item.id}");
                    Debug.Log($"ThumbNail Path : {thumbnailDownLoadUri.uri}");
                });
            });
        }

        private void LoadSlotList(List<AvatarResource> list)
        {
            // Draw UI
            Debug.Log($"Debug LoadSlotList : {list?.Count}");

            // BackGroud Resource Load And Thumbnail
            LoadAvatarResources();
            LoadThumbNails();
        }

        private void LoadUserAvatarResourceAddressable(UserAvatar userAvatar, AvatarResource avatarResource)
        {
            if (userAvatar != null && avatarResource != null)
            {
                AvatarDressUp(userAvatar.avatar, avatarResource);
            }
        }
        private void LoadAvatarResourceAddressable(AvatarResource avatarResource)
        {
            Debug.Log($"Debug LoadAvatarResourceAddressable : {avatarResource}");

            if (avatarResource == null) return;

            AvatarDressUp(slotAvatar, avatarResource);
        }
        private void LoadUserAvatarResourceDone(UserAvatar userAvatar)
        {
            if (userAvatar == null) return;

            var avatar = userAvatar.avatar;

            AvatarDressUp(avatar, userAvatar);
        }

        private void LoadUserAvatarRecipe(UserAvatar avatarData)
        {
            if (avatarData.recipeStr == null) return;

            if (avatarData.avatar == null)
            {
                var sub = GetToken().idTokenDictionary.GetTypedValue<string>(JwtTokenConstants.sub);
                int.TryParse(avatarData.id, out int index);

                avatarData.avatar = avatarData.id.Equals(sub) ? userAvatar : avatars[index];
            }

            avatarResourceRepo.LoadAvatarResource(avatarData, true).Forget();
        }
        private void SetUnderWear(DynamicCharacterAvatar avatar)
        {
            avatar.WardrobeRecipes.TryGetValue(Category.Set.ToString(), out var set);
            if (set != null) { return; }

            avatar.WardrobeRecipes.TryGetValue(Category.Chest.ToString(), out var chest);
            avatar.WardrobeRecipes.TryGetValue(Category.Legs.ToString(), out var legs);

            if (chest == null)
            {
                avatar.SetSlot(underWearTop);
                avatar.BuildCharacter(true);
            }

            if (legs == null)
            {
                avatar.SetSlot(underWearBottom);
                avatar.BuildCharacter(true);
            }
        }

        private void AvatarDressUp(DynamicCharacterAvatar avatar, UserAvatar userAvatar)
        {
            avatar.ClearSlots();
            avatar.SetRecipeString(userAvatar.recipeStr);
            avatar.MaxstDoLoad();
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
        }

        private Passport.Token GetToken()
        {
            return TokenRepo.Instance.GetToken();
        }
    }
}
