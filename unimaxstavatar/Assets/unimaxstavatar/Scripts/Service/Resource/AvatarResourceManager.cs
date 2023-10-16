using Cysharp.Threading.Tasks;
using Maxst.Passport;
using Maxst.Resource;
using Maxst.Token;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Maxst.Avatar
{
    public class AvatarResourceManager : MonoBehaviour
    {
        AvatarResourceService avatarResourceService;

        private Dictionary<Category, List<AvatarResource>> avatarResources = new Dictionary<Category, List<AvatarResource>>();
        private Dictionary<Category, List<AvatarResource>> publicResources = new Dictionary<Category, List<AvatarResource>>();
        private Dictionary<Category, List<AvatarResource>> saveAvatarResources = new Dictionary<Category, List<AvatarResource>>();

        private UserAvatar userAvatar;

        private List<string> visibleResAppIds = new List<string>();

        [SerializeField]
        private bool isResourceAllLoad;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            avatarResourceService = new AvatarResourceService();
        }

        public void SetVisibleResAppIds(List<string> appIds) {
            visibleResAppIds = appIds;
        }

        public List<string> GetVisibleResAppIds() {
            return visibleResAppIds;
        }

        public void AvatarSaveExtensionsDataListener(SaveRecipeExtensions saveRecipeExtensions)
        {
            Debug.Log($"[AvatarResourceManager] AvatarSaveExtensionsDataListener recipeString : {saveRecipeExtensions}");

            PostSaveRecipeExtensions(saveRecipeExtensions, (result) =>
            {
                Debug.Log($"[AvatarResourceManager] AvatarSaveExtensionsDataListener result : {result}");
            });
        }

        public void OnClickRecipe()
        {
            FetchSaveRecipeExtensions((data) =>
            {
                userAvatar = data;
                Debug.Log($"[AvatarResourceManager] FetchSaveRecipeExtensions data :{data}");
            });
        }

        public async void FetchSaveRecipeExtensions(Action<UserAvatar> action)
        {
            string token = GetAccessToken();
            var platform = ResourceSettingSO.Instance.Platform;

            UserAvatar saveRecipeExtensions = await avatarResourceService.FetchSaveRecipeExtensions(token, platform.ToString());

            FetchSaveAvatarResources(saveRecipeExtensions, token, (result) =>
            {
                foreach (var key in result.Keys)
                {
                    Debug.Log($"FetchSaveAvatarResources result {key} : {result[key]}");
                }
                saveAvatarResources = result;
                action.Invoke(saveRecipeExtensions);
            });
        }

        public async void PostSaveRecipeExtensions(SaveRecipeExtensions saveRecipeExtensions, Action<string> action = null)
        {
            string token = GetAccessToken();
            string result = await avatarResourceService.PostSaveRecipeExtensions(token, saveRecipeExtensions);
            action?.Invoke(result);
        }

        public void OnClickAvatar()
        {
            var appId = TokenRepo.Instance.GetToken().accessTokenDictionary.GetTypedValue<string>(JwtTokenConstants.app);
            SetVisibleResAppIds(new List<string> { appId, AvatarConstant.PUBLIC_RESOURCE_APPID });

            FetchAvatarResource();
        }

        public async void FetchAvatarResource()
        {
            var categoryList = new List<Category>() { Category.Hair, Category.Legs, Category.Feet, Category.Chest };
            var token = GetAccessToken();
            string appId = TokenRepo.Instance.GetToken().accessTokenDictionary.GetTypedValue<string>(JwtTokenConstants.app);

            var platform = ResourceSettingSO.Instance.Platform;

            await FetchAppAvatarResources(AvatarConstant.MAIN_CATEGORY, categoryList, platform, appId, (result =>
            {
                avatarResources = result;
            }));
            await FetchAppAvatarResources(AvatarConstant.MAIN_CATEGORY, categoryList, platform, AvatarConstant.PUBLIC_RESOURCE_APPID, (result =>
            {
                publicResources = result;
            }));

            SceneManager.LoadScene("AvatarScene");
        }

        public async void FetchSaveAvatarResources(
            UserAvatar saveRecipeExtensions,
            string token,
            Action<Dictionary<Category, List<AvatarResource>>> onComplete
        )
        {
            Dictionary<Category, List<AvatarResource>> saveAvatarResources = new Dictionary<Category, List<AvatarResource>>();

            foreach (var slot in saveRecipeExtensions.slots)
            {
                var avatarResouce = new AvatarResource();

                avatarResouce.thumbnailDownLoadUri = await avatarResourceService.FetchThumbnailDownLoadUri(token, slot.imageUri);

                foreach (var each in slot.assetResourceInfo)
                {
                    each.catalogDownloadUri = await avatarResourceService.FetchCatalogDownLoadUri(token, each.catalogUri);
                    avatarResouce.id = slot.itemId;
                    avatarResouce.subCategory = slot.slot;
                    avatarResouce.resources = new List<Resource>() { each };
                }
                saveAvatarResources[CategoryHelper.GetCategoryFromString(slot.slot)] = new List<AvatarResource>() { avatarResouce };
            }
            onComplete?.Invoke(saveAvatarResources);
        }

        public async UniTask FetchAppAvatarResources(string mainCategory, List<Category> subCategoryList, Platform platform, string appId, Action<Dictionary<Category, List<AvatarResource>>> onComplete)
        {
            string token = GetAccessToken();
            string platformString = platform.ToString();

            Dictionary<Category, List<AvatarResource>> avatarResources = new Dictionary<Category, List<AvatarResource>>();

            foreach (var subCategory in subCategoryList)
            {
                List<AvatarResource> resources = await avatarResourceService.FetchAvatarResources(token, mainCategory, subCategory.ToString(), platformString, appId);
                List<AvatarResource> temp = new List<AvatarResource>();

                foreach (var resource in resources)
                {
                    if (resource.hidden) continue;

                    resource.thumbnailDownLoadUri = await avatarResourceService.FetchThumbnailDownLoadUri(token, resource.imageUri);
                    foreach (var each in resource.resources)
                    {
                        each.catalogDownloadUri = await avatarResourceService.FetchCatalogDownLoadUri(token, each.catalogUri);
                        temp.Add(resource);
                    }
                }
                avatarResources[subCategory] = temp;
            }

            onComplete.Invoke(avatarResources);
        }

        private string GetAccessToken()
        {
            return TokenRepo.Instance.GetToken().accessToken;
        }

        public UserAvatar GetUserAvatar()
        {
            return userAvatar;
        }

        public Dictionary<Category, List<AvatarResource>> GetAvatarResources()
        {
            return avatarResources;
        }

        public Dictionary<Category, List<AvatarResource>> GetSaveAvatarResources()
        {
            return saveAvatarResources;
        }
        public Dictionary<Category, List<AvatarResource>> GetPublicResources()
        {
            return publicResources;
        }

        public bool IsResourceAllLoad() {
            return isResourceAllLoad;
        }
    }
}