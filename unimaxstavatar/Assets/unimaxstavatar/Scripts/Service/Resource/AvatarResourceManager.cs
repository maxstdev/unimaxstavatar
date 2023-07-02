using Cysharp.Threading.Tasks;
using Maxst.Passport;
using Maxst.Resource;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Maxst.Avatar
{
    public class AvatarResourceManager : MonoBehaviour
    {
        AvatarResourceService avatarResourceService;

        [SerializeField]
        private string sceneName = "AvatarScene";

        private string catalogJsonPath;
        private string saveRecipe;

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

        public async UniTask AvatarSaveDataListener(string recipeString)
        {
            string sub = TokenRepo.Instance.GetJwtTokenBody().sub;

            Debug.Log($"[AvatarResourceManager] AvatarSaveDataListener recipeString : {recipeString}");
            Debug.Log($"[AvatarResourceManager] AvatarSaveDataListener token sub : {sub}");

            await avatarResourceService.AvatarSaveDataUpload(GetAccessToken(), sub, recipeString);
        }

        public void OnClickRecipe()
        {
            DownLoadSaveRecipe();
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

        public void OnClickAvatar()
        {
            try
            {
                FetchCatalogJsonPath(() => {
                    SceneManager.LoadScene(sceneName);
                });
            }
            catch (Exception e)
            {
                Debug.Log($"[AvatarResourceManager] FetchCatalogJsonPath Exception : {e}");
            }
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

        public string GetCatalogJsonPath()
        {
            return catalogJsonPath;
        }

        public string GetSaveRecipe()
        {
            return saveRecipe;
        }
    }
}