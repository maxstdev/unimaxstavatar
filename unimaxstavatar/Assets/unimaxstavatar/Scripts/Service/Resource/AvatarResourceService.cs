using Cysharp.Threading.Tasks;
using Maxst.Resource;
using System.Collections.Generic;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;

namespace Maxst.Avatar
{
    public class AvatarResourceService
    {
        private Dictionary<Category, List<AvatarResource>> avatarResources;

        public AvatarResourceService()
        {
        }

        public async UniTask<string> PostSaveRecipeExtensions(string token, SaveRecipeExtensions saveRecipeExtension)
        {
            TaskCompletionSource<string> taskCompletionSource = new();
            
            AvatarService.Instance.PostUserSaveRecipe($"Bearer {token}", saveRecipeExtension)
                .ObserveOn(Scheduler.MainThread)
                .Subscribe(data =>
                {
                    Debug.Log($"[AvatarResourceService] FetchSaveRecipeExtensions data : {data}");

                    taskCompletionSource.TrySetResult(data);
                },
                error =>
                {
                    Debug.Log(error);
                    Debug.Log(error.Message);

                    taskCompletionSource.TrySetException(error);
                    taskCompletionSource.SetCanceled();
                });

            return await taskCompletionSource.Task;
        }

        public async UniTask<UserAvatar> FetchSaveRecipeExtensions(string token, string platformString)
        {
            TaskCompletionSource<UserAvatar> taskCompletionSource = new();

            AvatarService.Instance.FetchUserSaveRecipe($"Bearer {token}", platformString)
                .ObserveOn(Scheduler.MainThread)
                .Subscribe(data =>
                {
                    Debug.Log($"[AvatarResourceService] FetchSaveRecipeExtensions data : {data}");

                    taskCompletionSource.TrySetResult(data);
                },
                error =>
                {
                    Debug.Log(error);
                    Debug.Log(error.Message);

                    taskCompletionSource.TrySetException(error);
                    taskCompletionSource.SetCanceled();
                });

            return await taskCompletionSource.Task;
        }

        public async UniTask<List<AvatarResource>> FetchAvatarResources(string token, string mainCategory, string subCategory, string platformString, string appId)
        {
            TaskCompletionSource<List<AvatarResource>> avatarResources = new();
            var setting = ResourceSettingSO.Instance;

            AvatarService.Instance.FetchAvatarResources($"Bearer {token}", mainCategory, subCategory, platformString, appId)
                .ObserveOn(Scheduler.MainThread)
                .Subscribe(data =>
                {
                    Debug.Log($"[AvatarResourceService] FetchAvatarResources data : {data}");

                    avatarResources.TrySetResult(data);
                },
                error =>
                {
                    Debug.Log(error);
                    Debug.Log(error.Message);

                    //avatarResources.TrySetException(error);
                    //avatarResources.SetCanceled();
                    avatarResources.TrySetResult(new());
                });
            return await avatarResources.Task;
        }

        public async UniTask<CatalogDownLoadUri> FetchCatalogDownLoadUri(string token, string uri)
        {
            TaskCompletionSource<CatalogDownLoadUri> taskCompletionSource = new();

            using UnityWebRequest request = UnityWebRequest.Get(uri);

            request.SetRequestHeader("Authorization", "Bearer " + token);

            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Request successful. Response: {request.downloadHandler.text}");

                var result = request.downloadHandler.text;
                taskCompletionSource.TrySetResult(JsonUtility.FromJson<CatalogDownLoadUri>(result));

            }
            else
            {
                Debug.LogError($"Request failed. Error: {request.error}");
            }

            return await taskCompletionSource.Task;
        }

        public async UniTask<ThumbnailDownLoadUri> FetchThumbnailDownLoadUri(string token, string uri)
        {
            TaskCompletionSource<ThumbnailDownLoadUri> taskCompletionSource = new();

            using UnityWebRequest request = UnityWebRequest.Get(uri);

            request.SetRequestHeader("Authorization", "Bearer " + token);

            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Request successful. Response: {request.downloadHandler.text}");

                var result = request.downloadHandler.text;
                taskCompletionSource.TrySetResult(JsonUtility.FromJson<ThumbnailDownLoadUri>(result));

            }
            else
            {
                Debug.LogError($"Request failed. Error: {request.error}");
            }

            return await taskCompletionSource.Task;
        }
    }
}

