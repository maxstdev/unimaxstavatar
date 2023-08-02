using Cysharp.Threading.Tasks;
using Maxst.Passport;
using Maxst.Resource;
using Maxst.Token;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Maxst.Avatar
{
    public class AvatarResourceManager : MonoBehaviour
    {
        AvatarResourceService avatarResourceService;

        [SerializeField]
        private string sceneName = "AvatarScene";

        private List<ResourceMeta> catalogJsonMetaList = new List<ResourceMeta>();
        private List<ContainMeta> umaIdList = new List<ContainMeta>();
        private string catalogJsonPath;
        private string saveRecipe;
        private string saveRecipeExtensions;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            avatarResourceService = new(new());
            avatarResourceService.SetListener((uploadProgress) =>
            {
                Debug.Log($"[AvatarResourceManager] uploadProgress : {uploadProgress}");
            },
            (key) =>
            {
                Debug.Log($"[AvatarResourceManager] OnComplete : {key}");
            },
            () =>
            {
                Debug.Log($"[AvatarResourceManager] OnUploadError");
            }
            );
        }

        public async void AvatarSaveDataListener(string recipeString)
        {
            string sub = TokenRepo.Instance.GetJwtTokenBody().sub;

            Debug.Log($"[AvatarResourceManager] AvatarSaveDataListener recipeString : {recipeString}");
            Debug.Log($"[AvatarResourceManager] AvatarSaveDataListener token sub : {sub}");

            await avatarResourceService.AvatarSaveDataUpload(GetAccessToken(), sub, recipeString);
        }

        public async void AvatarSaveExtensionsDataListener(string recipeString)
        {
            string sub = TokenRepo.Instance.GetJwtTokenBody().sub;

            Debug.Log($"[AvatarResourceManager] AvatarSaveExtensionsDataListener recipeString : {recipeString}");
            Debug.Log($"[AvatarResourceManager] AvatarSaveExtensionsDataListener token sub : {sub}");

            await avatarResourceService.AvatarSaveExtensionsDataUpload(GetAccessToken(), sub, recipeString);
        }

        public void OnClickRecipe()
        {
#if MAXST_SAVE_RECIPE_EXTENSIONS
            DownLoadSaveRecipeExtensions();
#else
            DownLoadSaveRecipe();
#endif
        }

        public async void DownLoadSaveRecipeExtensions()
        {
            try
            {
                string token = GetAccessToken();
                string sub = TokenRepo.Instance.GetJwtTokenBody().sub;
                Container subContainer = await avatarResourceService.FetchSubContainer(token, sub);
                Contain subContain = await avatarResourceService.FetchSaveRecipeContain(token, subContainer, true);

                await avatarResourceService.FileDownload(token, subContain.uri, (json) =>
                {
                    Debug.Log($"[AvatarResourceManager] DownLoadSaveRecipeExtensions : {json}");
                    saveRecipeExtensions = json;
                    LoadSaveRecipeExtensions();
                });
            }
            catch (Exception e)
            {
                Debug.Log($"[AvatarResourceManager] DownLoadSaveRecipeExtensions Exception : {e}");
            }
        }

        public async void DownLoadSaveRecipe()
        {
            try
            {
                string token = GetAccessToken();
                string sub = TokenRepo.Instance.GetJwtTokenBody().sub;
                Container subContainer = await avatarResourceService.FetchSubContainer(token, sub);
                Contain subContain = await avatarResourceService.FetchSaveRecipeContain(token, subContainer);

                await avatarResourceService.FileDownload(token, subContain.uri, (json) =>
                {
                    Debug.Log($"[AvatarResourceManager] DownLoadSaveRecipe : {json}");
                    saveRecipe = json;
                });
            }
            catch (Exception e)
            {
                Debug.Log($"[AvatarResourceManager] DownLoadSaveRecipe Exception : {e}");
            }
        }

        private void LoadSaveRecipeExtensions()
        {
            var token = GetAccessToken();
            var clientId = GetJwtTokenBody().azp;
            var platform = Platform.StandaloneWindows64;

            SaveRecipeExtensions saveRecipeExtensionsObj = JsonUtility.FromJson<SaveRecipeExtensions>(saveRecipeExtensions);

            saveRecipeExtensionsObj.wardrobePaths.ForEach(each =>
            {
                SetSaveRecipeCatalogJsonPaths(token, each.clientId, platform.ToString(), each.slot, each.recipe);
            });
        }

        public void OnClickAvatar()
        {
            try
            {
                FetchCatalogJsonPath(() =>
                {
                    //SceneManager.LoadScene(sceneName);

                    var categoryList = new List<string>() { Category.Hair.ToString(), Category.Legs.ToString(), Category.Feet.ToString(), Category.Chest.ToString() };
                    var token = GetAccessToken();
                    var clientId = GetJwtTokenBody().azp;
                    var platform = Platform.StandaloneWindows64;

                    SetCategoryCatalogJsonPaths(token, clientId, platform, categoryList, () =>
                    {
                        SceneManager.LoadScene(sceneName);
                    });
                });
            }
            catch (Exception e)
            {
                Debug.Log($"[AvatarResourceManager] FetchCatalogJsonPath Exception : {e}");
            }
        }

        private async void SetSaveRecipeCatalogJsonPaths(string token, string clientId, string platform, string category, string avatar_resource_name, Action onComplete = null)
        {
            var key = avatar_resource_name.Substring(0, avatar_resource_name.Length - 7);

            ContainerMeta metaList = await FetchCategoryContainerMetaList(token, clientId, category);
            if (metaList.contains != null)
            {
                ContainMeta item = metaList.contains
                    .SingleOrDefault(contain =>
                    {
                        if (contain.extension != null)
                        {
                            contain.extension.TryGetValue("avatar_resource_name", out string slotRecipe);
                            if (slotRecipe == null) return false;

                            if (RemoveExt(slotRecipe).Equals(key)) Debug.Log($"[AvatarResourceManager] key : {key}");

                            return RemoveExt(slotRecipe).Equals(key);
                        }
                        else return false;
                    });

                Debug.Log($"[AvatarResourceManager] item : {item}");

                if (item != null)
                {
                    /*var saveRecipeCatalogJsonMeta = metaList.resources
                        .Where(resource => resource.type.Equals("Catalog"))
                        *//*.Where(resource =>
                        {
                            var temp = resource.parents.Split("/");
                            var length = temp.Length;
                            return temp[length - 2].Equals(platform.ToString());
                        })*//*
                        .ToList();*/

                    metaList.resources.ForEach(resource => {
                        resource.extension.TryGetValue("avatar_resource_name", out string avatar_resource_name);
                        if (avatar_resource_name != null && RemoveExt(avatar_resource_name).Equals(key)) {
                            catalogJsonMetaList.Add(resource);
                        }
                    });
                }
            }
            
            onComplete?.Invoke();
        }

        private string RemoveExt(string fileNameWithExt)
        {
            int lasttIndex = fileNameWithExt.LastIndexOf('.');
            if (lasttIndex >= 0)
            {
                return fileNameWithExt.Substring(0, lasttIndex);
            }

            return fileNameWithExt;
        }

        private async void SetCategoryCatalogJsonPaths(string token, string clientId, Platform platform, List<string> categoryList, Action onComplete = null)
        {
            List<UniTask<ContainerMeta>> tasks = categoryList
                .Select(category => FetchCategoryContainerMetaList(token, clientId, category))
                .ToList();

            ContainerMeta[] metaList = await UniTask.WhenAll(tasks);

            if (metaList != null)
            {
                /*
                  umaIdList.AddRange(metaList
                    .SelectMany(each => each.contains)
                    .Where(each => each.extension.ContainsKey("avatar_resource_name"))
                    .ToList());
                */

                var temp = metaList
                                .Where(meta => meta.resources != null)
                                .SelectMany(metaList => metaList.resources)
                                .Where(resource => resource.type.Equals("Catalog"))
                                .ToList();

                catalogJsonMetaList.AddRange(temp
                                /*.Where(resource =>
                                {
                                    var temp = resource.parents.Split("/");
                                    var length = temp.Length;
                                    return temp[length - 2].Equals(platform.ToString());
                                })*/);
            }

            onComplete?.Invoke();
        }

        private async UniTask<string> FetchCategoryCatalogJsonPath(string custom)
        {
            var catalogJson = ResourceSettingSO.Instance.CatalogJsonFileName;
            var ext = ResourceSettingSO.Instance.Ext;

            var containerMeta = await FetchCustomMeta(custom);
            var resource = containerMeta.resources
                .SingleOrDefault(res => res.originalFileName.Equals($"{catalogJson}{ext}"));
            return (resource != null && !String.IsNullOrEmpty(resource.dataUrl)) ? resource.dataUrl : null;
        }

        public async UniTask<ContainerMeta> FetchCustomMeta(string custom)
        {
            string token = GetAccessToken();
            var result = await avatarResourceService.FetchCustomMeta(token, custom);
            return result;
        }

        public async UniTask<ContainerMeta> FetchCategoryContainerMetaList(string token, string clientId, string category)
        {
            var result = await avatarResourceService.FetchContainerMetaList(token, clientId, category, Type.all);
            return result;
        }

        public async UniTask<ContainMeta> FetchContainerMeta(string slot, Action<ContainMeta> onComplete = null)
        {
            string token = GetAccessToken();
            var result = await avatarResourceService.FetchContainerMeta(token, TokenRepo.Instance.GetJwtTokenBody().azp, slot, Type.meta);
            onComplete?.Invoke(result);
            return result;
        }

        public async void FetchCatalogJsonPath(Action onComplete)
        {
            string token = GetAccessToken();
            Container publicContainer = await avatarResourceService.FetchPublicContainer(token);
            Contain contain = await avatarResourceService.FetchResContain(token, publicContainer);
            catalogJsonPath = contain.uri;
            onComplete.Invoke();
        }

        private string GetAccessToken()
        {
            return TokenRepo.Instance.GetToken().accessToken;
        }
        private JwtTokenBody GetJwtTokenBody()
        {
            return TokenRepo.Instance.GetJwtTokenBody();
        }

        public string GetCatalogJsonPath()
        {
            return catalogJsonPath;
        }
        public List<ResourceMeta> GetCatalogJsonMetaList()
        {
            return catalogJsonMetaList;
        }

        public List<ContainMeta> GetUmaIdList()
        {
            return umaIdList;
        }

        public string GetSaveRecipe()
        {
            return saveRecipe;
        }
        public string GetSaveRecipeExtensions()
        {
            return saveRecipeExtensions;
        }
    }
}