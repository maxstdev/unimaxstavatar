using Cysharp.Threading.Tasks;
using Maxst.Passport;
using Maxst.Resource;
using Maxst.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
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
        private bool isResourceFullLoad;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            
            TokenRepo.Instance
                .tokenStatus.Subscribe(status =>
                {
                    if (status == TokenStatus.Validate) {
                        Passport.Token token = TokenRepo.Instance.GetToken();

                        avatarResourceService = new AvatarResourceService();
                        avatarResourceService.SetToken(token.accessToken);

                        var appId = token.accessTokenDictionary.GetTypedValue<string>(JwtTokenConstants.app);
                        SetVisibleResAppIds(new List<string> { appId, AvatarConstant.PUBLIC_RESOURCE_APPID });
                    }
                })
                .AddTo(this);
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
#if false
            var appId = TokenRepo.Instance.GetToken().accessTokenDictionary.GetTypedValue<string>(JwtTokenConstants.app);
            FetchAvatarResource(() => {
                Debug.Log("[AvatarResourceManager] res load complete!");
                SceneManager.LoadScene("AvatarScene");
            });
#else
            SceneManager.LoadScene("AvatarScene");
#endif
        }

        public async void FetchAvatarResource(Action action = null)
        {
            var categoryList = new List<Category>() { Category.Hair, Category.Legs, Category.Feet, Category.Chest, Category.Set };
            var token = GetAccessToken();
            string appId = TokenRepo.Instance.GetToken().accessTokenDictionary.GetTypedValue<string>(JwtTokenConstants.app);

            var platform = ResourceSettingSO.Instance.Platform;

            var apptask = FetchAppAvatarResources(AvatarConstant.MAIN_CATEGORY, categoryList, platform, appId, (result =>
            {
                avatarResources = result;
            }));
            var publictask = FetchAppAvatarResources(AvatarConstant.MAIN_CATEGORY, categoryList, platform, AvatarConstant.PUBLIC_RESOURCE_APPID, (result =>
            {
                publicResources = result;
            }));

            await UniTask.WhenAll(apptask, publictask);
            
            action?.Invoke();
        }

        public void OnClickScene()
        {
            SceneManager.LoadScene("MultiAvatarScene");
        }

        public async void FetchSaveAvatarResources(
            UserAvatar saveRecipeExtensions,
            string token,
            Action<Dictionary<Category, List<AvatarResource>>> onComplete
         )
        {
            Dictionary<Category, List<AvatarResource>> userSaveAvatarResources = new Dictionary<Category, List<AvatarResource>>();

            var catalogDownloadTasks = new List<UniTask>();

            foreach (var slot in saveRecipeExtensions.slots)
            {
                var avatarResource = new AvatarResource();
                var resInfo = slot.assetResourceInfo[0];

                catalogDownloadTasks.Add(FetchCatalogDownloadUriAsync(avatarResource, token, resInfo));

                avatarResource.id = slot.itemId;
                avatarResource.subCategory = slot.slot;
                avatarResource.resources = new List<Resource>() { resInfo };

                userSaveAvatarResources[CategoryHelper.GetCategoryFromString(slot.slot)] = new List<AvatarResource>() { avatarResource };
            }
            
            await UniTask.WhenAll(catalogDownloadTasks);
            await UniTask.WhenAll(saveRecipeExtensions.slots.Select(async slot =>
            {
                var avatarResource = userSaveAvatarResources[CategoryHelper.GetCategoryFromString(slot.slot)][0];
                avatarResource.thumbnailDownLoadUri = await avatarResourceService.FetchThumbnailDownLoadUri(token, slot.imageUri);
            }));

            onComplete?.Invoke(userSaveAvatarResources);
        }
        private async UniTask FetchCatalogDownloadUriAsync(AvatarResource avatarResource, string token, Resource resInfo)
        {
            resInfo.catalogDownloadUri = await avatarResourceService.FetchCatalogDownLoadUri(resInfo.catalogUri);
        }

        public async UniTask FetchAppAvatarResources(string mainCategory, List<Category> subCategoryList, Platform platform, string appId, Action<Dictionary<Category, List<AvatarResource>>> onComplete)
        {
            string platformString = platform.ToString();

            Dictionary<Category, List<AvatarResource>> result = new Dictionary<Category, List<AvatarResource>>();
            
            foreach (var subCategory in subCategoryList)
            {
                foreach (var task in await UniTask.WhenAll(FetchAvatarResourcesAsync(subCategory, platform, mainCategory, appId)))
                {
                    result[subCategory] = task;
                }
            }

            onComplete.Invoke(result);
        }

        private async UniTask<List<AvatarResource>> FetchAvatarResourcesAsync(Category subCategory, Platform platform, string mainCategory, string appId)
        {
            string token = GetAccessToken();
            string platformString = platform.ToString();

            List<AvatarResource> resources = await avatarResourceService.FetchAvatarResources(mainCategory, subCategory.ToString(), platformString, appId);
            List<AvatarResource> temp = new List<AvatarResource>();

            List<UniTask> resTask = new();
            foreach (var resource in resources)
            {
                var task = FetchDownLoadUriAsync(token, resource);
                resTask.Add(task);
            }

            await UniTask.WhenAll(resTask);

            temp.AddRange(resources);

            return temp;
        }

        private async UniTask FetchDownLoadUriAsync(string token, AvatarResource resource)
        {
            if (resource.hidden) return;

            resource.thumbnailDownLoadUri = await avatarResourceService.FetchThumbnailDownLoadUri(token, resource.imageUri);

            foreach (var each in resource.resources)
            {
                each.catalogDownloadUri = await avatarResourceService.FetchCatalogDownLoadUri(each.catalogUri);
            }
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

        public bool IsResourceFullLoad() {
            return isResourceFullLoad;
        }
    }
}