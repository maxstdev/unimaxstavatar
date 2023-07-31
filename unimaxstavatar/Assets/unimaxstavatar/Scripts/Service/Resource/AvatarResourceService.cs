using Cysharp.Threading.Tasks;
using Maxst.Resource;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TUSService;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;

namespace Maxst.Avatar
{
    public class AvatarResourceService
    {
        private const string APP_FOLDER_PREFIX = "MAXST_Gadgets/avatar";
        
        private TUSServiceClient client;

        public AvatarResourceService(TUSServiceClient client)
        {
            this.client = client;
        }

        public void SetListener(Action<int> OnUploadProgress, Action<string> completed, Action OnUploadError)
        {
            client.Init(OnUploadProgress, completed, OnUploadError);
        }

        public async UniTask AvatarSaveDataUpload(string token, string sub, string recipeString)
        {
            var filePath = GetFilePath();
            Debug.Log($"AvatarSaveDataUpload filePath : {filePath}");
            File.WriteAllText(filePath, recipeString);

            await StartTUSUpload(token, sub, filePath);
        }

        public async UniTask AvatarSaveExtensionsDataUpload(string token, string sub, string recipeString)
        {
            var filePath = GetSaveExtensionsFilePath();
            Debug.Log($"AvatarSaveExtensionsDataUpload filePath : {filePath}");
            File.WriteAllText(filePath, recipeString);

            await StartTUSUpload(token, sub, filePath);
        }
        private string GetFilePath()
        {
#if UNITY_EDITOR
            return $"{Application.dataPath}/{ResourceSettingSO.Instance.SaveFileName}{ResourceSettingSO.Instance.Ext}";
#else
            return $"{Application.persistentDataPath}/{ResourceSettingSO.Instance.SaveFileName}{ResourceSettingSO.Instance.Ext}";
#endif
        }

        private string GetSaveExtensionsFilePath()
        {
#if UNITY_EDITOR
            return $"{Application.dataPath}/{ResourceSettingSO.Instance.SaveExtensionsFileName}{ResourceSettingSO.Instance.Ext}";
#else
            return $"{Application.persistentDataPath}/{ResourceSettingSO.Instance.SaveExtensionsFileName}{ResourceSettingSO.Instance.Ext}";
#endif
        }

        public async UniTask StartTUSUpload(string token, string sub, string filePath)
        {
            Debug.Log($"[TusUploadHelper] StartTUSUpload : StartTUSUpload");
            Debug.Log($"[TusUploadHelper] select filePath : {filePath}");
            await client.StartTUSUpload(token, sub, filePath, true);
        }

        public async UniTask<Container> FetchSubContainer(string token, string sub)
        {
            TaskCompletionSource<Container> container = new();
            //var setting = ResourceSettingSO.Instance;

            AvatarService.Instance.FetchPublicContainer($"Bearer {token}", $"/{sub}/")
                .ObserveOn(Scheduler.MainThread)
                .Subscribe(data =>
                {
                    Debug.Log($"[AvatarResourceService] FetchSubContainer data : {data}");

                    container.TrySetResult(data);
                },
                error =>
                {
                    Debug.Log(error);
                    Debug.Log(error.Message);

                    container.TrySetException(error);
                    container.SetCanceled();
                });

            return await container.Task;
        }

        public async UniTask<ContainerMeta> FetchContainerMetaList(string token, string clientId, string slot, Type type)
        {
            TaskCompletionSource<ContainerMeta> containerMeta = new();
            var setting = ResourceSettingSO.Instance;

            AvatarService.Instance.FetchCategoryContainerMetaList($"Bearer {token}", clientId, APP_FOLDER_PREFIX, slot, type.ToString())
                .ObserveOn(Scheduler.MainThread)
                .Subscribe(data =>
                {
                    Debug.Log($"[AvatarResourceService] FetchContainerMetaList data : {data}");

                    containerMeta.TrySetResult(data);
                },
                error =>
                {
                    Debug.Log(error);
                    Debug.Log(error.Message);

                    containerMeta.TrySetException(error);
                    containerMeta.SetCanceled();
                });
            return await containerMeta.Task;
        }

        public async UniTask<ContainMeta> FetchContainerMeta(string token, string clientId, string slot, Type type)
        {
            TaskCompletionSource<ContainMeta> container = new();
            var setting = ResourceSettingSO.Instance;

            AvatarService.Instance.FetchContainerMeta($"Bearer {token}", clientId, APP_FOLDER_PREFIX, slot, type.ToString())
                .ObserveOn(Scheduler.MainThread)
                .Subscribe(data =>
                {
                    Debug.Log($"[AvatarResourceService] FetchCategoryContainerMeta data : {data}");

                    container.TrySetResult(data);
                },
                error =>
                {
                    Debug.Log(error);
                    Debug.Log(error.Message);

                    container.TrySetException(error);
                    container.SetCanceled();
                });
            return await container.Task;
        }

        public async UniTask<ContainerMeta> FetchCustomMeta(string token, string custom)
        {
            TaskCompletionSource<ContainerMeta> ContainerMeta = new();
            var setting = ResourceSettingSO.Instance;

            AvatarService.Instance.FetchCustomMeta($"Bearer {token}", custom, Type.metaList.ToString())
                .ObserveOn(Scheduler.MainThread)
                .Subscribe(data =>
                {
                    Debug.Log($"[AvatarResourceService] FetchPublicContainer data : {data}");

                    ContainerMeta.TrySetResult(data);
                },
                error =>
                {
                    Debug.Log(error);
                    Debug.Log(error.Message);

                    ContainerMeta.TrySetException(error);
                    ContainerMeta.SetCanceled();
                });
            return await ContainerMeta.Task;
        }
        public async UniTask<Container> FetchPublicContainer(string token)
        {
            TaskCompletionSource<Container> container = new();
            var setting = ResourceSettingSO.Instance;

            AvatarService.Instance.FetchPublicContainer($"Bearer {token}", $"{setting.Container}{setting.Platform}/")
                .ObserveOn(Scheduler.MainThread)
                .Subscribe(data =>
                {
                    Debug.Log($"[AvatarResourceService] FetchPublicContainer data : {data}");

                    container.TrySetResult(data);
                },
                error =>
                {
                    Debug.Log(error);
                    Debug.Log(error.Message);

                    container.TrySetException(error);
                    container.SetCanceled();
                });
            return await container.Task;
        }

        public async UniTask FileDownload(string token, string uri, Action<string> action)
        {
            using UnityWebRequest request = UnityWebRequest.Get(uri);

            request.SetRequestHeader("token", "Bearer " + token);

            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Request successful. Response: {request.downloadHandler.text}");
                action.Invoke(request.downloadHandler.text);
            }
            else
            {
                Debug.LogError($"Request failed. Error: {request.error}");
            }
        }

        public async UniTask<Contain> FetchSaveRecipeContain(string token, Container subContainer, bool isSaveRecipeExtensions = false)
        {
            var setting = ResourceSettingSO.Instance;

            string SaveFileName = isSaveRecipeExtensions ? $"{setting.SaveExtensionsFileName}{setting.Ext}" : $"{setting.SaveFileName}{setting.Ext}";
            string tempUri = subContainer?.resources.Count != 0 ? subContainer.resources.FirstOrDefault(item => item.name.Contains(SaveFileName)).uri : null;

            TaskCompletionSource<Contain> Contain = new();

            AvatarService.Instance.FetchContain($"Bearer {token}", tempUri)
                .ObserveOn(Scheduler.MainThread)
                .Subscribe(data =>
                {
                    Debug.Log($"[AvatarResourceService] FetchSaveRecipeContain data : {data}");

                    Contain.TrySetResult(data);
                },
                error =>
                {
                    Debug.Log(error);
                    Debug.Log(error.Message);

                    Contain.TrySetException(error);
                    Contain.SetCanceled();
                });
            return await Contain.Task;
        }

        public async UniTask<Contain> FetchResContain(string token, Container publicContainer)
        {
            var setting = ResourceSettingSO.Instance;
            string catalogJsonName = $"{setting.CatalogJsonFileName}{setting.Ext}";
            string tempUri = publicContainer?.resources.Count != 0 ? publicContainer.resources.FirstOrDefault(item => item.name.Contains(catalogJsonName)).uri : null;

            TaskCompletionSource<Contain> Contain = new();

            AvatarService.Instance.FetchContain($"Bearer {token}", tempUri)
                .ObserveOn(Scheduler.MainThread)
                .Subscribe(data =>
                {
                    Debug.Log($"[AvatarResourceService] FetchResContain data : {data}");

                    Contain.TrySetResult(data);
                },
                error =>
                {
                    Debug.Log(error);
                    Debug.Log(error.Message);

                    Contain.TrySetException(error);
                    Contain.SetCanceled();
                });
            return await Contain.Task;
        }
    }
}

