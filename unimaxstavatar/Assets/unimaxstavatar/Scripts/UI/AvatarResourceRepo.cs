using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Maxst.Passport;
using Maxst.Resource;
using Maxst.Token;
using MaxstUtils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using UMA;
using UMA.CharacterSystem;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Maxst.Avatar
{
    public class AvatarResourceRepo : MaxstUtils.Singleton<AvatarResourceRepo>
    {
        public List<long> defaultSlotIds = new List<long>();

        private Dictionary<long, OverlayDataAsset> OverlayDataAsset = new();
        private Dictionary<long, UMATextRecipe> UMATextRecipe = new();
        private Dictionary<long, RaceData> RaceData = new();
        private Dictionary<long, UmaTPose> UmaTPose = new();
        private Dictionary<long, UMAWardrobeRecipe> UMAWardrobeRecipe = new();
        private Dictionary<long, DynamicUMADnaAsset> DynamicUMADnaAsset = new();
        private Dictionary<long, DynamicDNAConverterController> DynamicDNAConverterController = new();
        private Dictionary<long, UnityEngine.Object> SlotData = new();
        private Dictionary<long, Texture2D> Texture2D = new();

        public Dictionary<string, string> resAppIdDict = new Dictionary<string, string>();

        private Passport.Token token;

        public Event<List<AvatarResource>> loadSlotListEvent = new(new());

        public Event<AvatarResource> loadAvatarResourceAddressableEvent = new();

        public Event<UserAvatar> loadUserAvatarRecipeEvent = new(new());
        public Event<UserAvatar> loadUserAvatarResourceDoneEvent = new();
        
        public Event<UserAvatar, AvatarResource> loadUserAvatarResourceAddressableEvent = new();

        public Event<AvatarResource, DownloadStatus> resourceDownloadStatusEvent = new();

        private List<string> avatarUpdateHistoryList = new List<string>();

        public void Init()
        {
            Addressables.WebRequestOverride = SetHeader;
            token = TokenRepo.Instance.GetToken();
        }

        private void SetHeader(UnityWebRequest unityWebRequest)
        {
            Debug.Log($"ModifyWebRequest Before {unityWebRequest.uri}");
            unityWebRequest.uri = new Uri(unityWebRequest.uri.ToString());
            unityWebRequest.SetRequestHeader("token", $"Bearer {token.accessToken}");
            Debug.Log($"ModifyWebRequest end {unityWebRequest}");
        }

        public void SetToken(Passport.Token token)
        {
            this.token = token;
        }

        public SaveRecipeExtensions GetSaveRecipeExtensions(DynamicCharacterAvatar avatar)
        {
            var saveRecipeString = avatar.MaxstDoSave(false);
            var saveRecipeExtensions = new SaveRecipeExtensions();
            saveRecipeExtensions.SetSaveRecipeString(saveRecipeString);
            return saveRecipeExtensions;
        }

        public async UniTask<string> PostSaveRecipeExtensions(SaveRecipeExtensions saveRecipeExtension)
        {
            TaskCompletionSource<string> taskCompletionSource = new();

            AvatarService.Instance.PostUserSaveRecipe($"Bearer {token.accessToken}", saveRecipeExtension)
                .ObserveOn(Scheduler.MainThread)
                .Subscribe(data =>
                {
                    Debug.Log($"[AvatarResourceRepo] FetchSaveRecipeExtensions data : {data}");

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

        private async UniTask<UserAvatar> FetchSaveRecipeExtensions(IObservable<UserAvatar> ob)
        {
            TaskCompletionSource<UserAvatar> taskCompletionSource = new();
            ob.Subscribe(data =>
            {
                Debug.Log($"[AvatarResourceRepo] FetchSaveRecipeExtensions data : {data}");
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

        public void LoadDefaultAvatarResource(List<AvatarResource> list)
        {
            var DEFAULT_CATEGORYS = new List<Category>() { Category.Hair, Category.Chest, Category.Legs, Category.Feet };
            var tasks = new List<UniTask>();

            foreach (var category in DEFAULT_CATEGORYS)
            {
                AvatarResource res = list.Where(item => item.subCategory.Equals(category.ToString())).FirstOrDefault();

                if (res != null)
                {
                    defaultSlotIds.Add(res.id);
                    tasks.Add(LoadAvatarResource(res));
                }
            }

            UniTask.WhenAll(tasks);
        }

        public async UniTask LoadSaveRecipeExtensions(Platform platform)
        {
            var ob = AvatarService.Instance.FetchUserSaveRecipe($"Bearer {token.accessToken}", platform.ToString())
                .ObserveOn(Scheduler.ThreadPool);

            var data = await FetchSaveRecipeExtensions(ob);
            data.id = token.idTokenDictionary.GetTypedValue<string>(JwtTokenConstants.sub);

            loadUserAvatarRecipeEvent.Post(data);
        }

        private Resource GetPlatformResource(UserAvatar.Slot slot)
        {
            return slot
                    .assetResourceInfo
                    .SingleOrDefault(item => item.os.Equals(ResourceSettingSO.Instance.Platform.ToString()));
        }

        public async UniTask LoadAvatarResource(UserAvatar userAvatar, bool isHighPriority = false)
        {
            List<UniTask> downloadTasks = new List<UniTask>();

            userAvatar.slots?.ForEach(slot =>
            {
                AvatarResource avatarResource = new();

                var resInfo = GetPlatformResource(slot);
                avatarResource.id = slot.itemId;
                avatarResource.subCategory = slot.slot;
                avatarResource.resources = new List<Resource> { resInfo };

                var downloadTask = DownLoadResourceUrl<CatalogDownLoadUri>(resInfo.catalogUri)
                                        .ContinueWith(async downloadUri =>
                                        {
                                            avatarResource.resources[0].catalogDownloadUri = downloadUri;
                                            resInfo.catalogDownloadUri = downloadUri;

                                            await LoadAvatarAddressable(avatarResource, isHighPriority);

                                            loadUserAvatarResourceAddressableEvent.Post(userAvatar, avatarResource);
                                        });

                downloadTasks.Add(downloadTask);
            });

            if (userAvatar.slots != null)
            {
                await UniTask.WhenAll(downloadTasks);

                loadUserAvatarResourceDoneEvent.Post(userAvatar);
            }
        }

        public void LoadDummyUsers()
        {
            //var list = await DownLoadResourceUrl<List<UserAvatar>>(DUMMY_URL, false);
            var jsonFile = Resources.Load<TextAsset>("recipes");
            if (jsonFile != null)
            {
                List<UserAvatar> list = JsonConvert.DeserializeObject<List<UserAvatar>>(jsonFile.text);
                list.ForEach(userAvatar =>
                {
                    loadUserAvatarRecipeEvent.Post(userAvatar);
                });
            }
            else
            {
                Debug.LogError("JSON load error");
            }
        }

        public async UniTask LoadResourceThumbnail(AvatarResource avatarResource, Action<ThumbnailDownLoadUri> action = null)
        {
            var imageUri = avatarResource.imageUri;
            avatarResource.thumbnailDownLoadUri = await DownLoadResourceUrl<ThumbnailDownLoadUri>(imageUri);
            action?.Invoke(avatarResource.thumbnailDownLoadUri);
        }

        public async UniTask LoadAvatarResource(AvatarResource avatarResource, bool isHighPriority = false)
        {
            var resInfo = avatarResource.resources[0];
            resInfo.catalogDownloadUri = await DownLoadResourceUrl<CatalogDownLoadUri>(resInfo.catalogUri);
            LoadAvatarAddressable(avatarResource, isHighPriority).Forget();
        }

        public async UniTask LoadAllSlotList(string mainCategory, string platformString, List<string> appIds)
        {
            var temp = new List<AvatarResource>();

            foreach (var appId in appIds)
            {
                var task = AvatarService.Instance.FetchAllAvatarResources($"Bearer {token.accessToken}", mainCategory, platformString, appId)
                    .ObserveOn(Scheduler.ThreadPool);

                List<AvatarResource> result = await FetchSlotList(task);

                temp.AddRange(result);
            }

            loadSlotListEvent.Post(temp);
        }
        public async UniTask LoadSlotList(string mainCategory, string subCategory, string platformString, string appId)
        {
            var ob = AvatarService.Instance.FetchAvatarResources($"Bearer {token.accessToken}", mainCategory, subCategory, platformString, appId)
                    .ObserveOn(Scheduler.ThreadPool);

            var result = await FetchSlotList(ob);
            loadSlotListEvent.Post(result);
        }

        private async UniTask<List<AvatarResource>> FetchSlotList(IObservable<List<AvatarResource>> ob)
        {
            TaskCompletionSource<List<AvatarResource>> taskCompletionSource = new();
            ob.Subscribe(data =>
            {
                Debug.Log($"[AvatarResourceRepo] SlotList data : {data}");
                taskCompletionSource.TrySetResult(data);
            },
            error =>
            {
                Debug.Log(error);
                Debug.Log(error.Message);
            });
            return await taskCompletionSource.Task;
        }

        public async UniTask<T> DownLoadResourceUrl<T>(string uri, bool isSetToken = true) where T : class
        {
            TaskCompletionSource<T> taskCompletionSource = new();

            using UnityWebRequest request = UnityWebRequest.Get(uri);

            if (isSetToken) request.SetRequestHeader("Authorization", "Bearer " + token.accessToken);

            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Request successful. Response: {request.downloadHandler.text}");
                var result = request.downloadHandler.text.Trim();
                taskCompletionSource.TrySetResult(JsonConvert.DeserializeObject<T>(result));
            }
            else
            {
                Debug.LogError($"Request failed. Error: {request.error}");
            }

            return await taskCompletionSource.Task;
        }

        private bool IsResourceLoad(long itemId)
        {
            return UMAAssetIndexer.Instance.GetAllAssets<UMAWardrobeRecipe>()
                .Any(item => item != null && item.name == itemId.ToString());
        }

        private async UniTask LoadAvatarAddressable(AvatarResource avatarResource, bool isHighPriority = false)
        {
            string catalogPath = avatarResource.resources[0].catalogDownloadUri.uri;

            if (IsResourceLoad(avatarResource.id))
            {
                PostLoadAvatarResourceAddressableEvent(avatarResource);
                return;
            }

            var id = avatarResource.id;

            List<string> keys = new List<string>();

            Debug.Log($"loadCatalogPath : {catalogPath}");
            var content = await Addressables.LoadContentCatalogAsync(catalogPath, true).Task;

            foreach (var each in content.Keys)
            {
                keys.Add(each.ToString());
            }

            var locations = Addressables.LoadResourceLocationsAsync(keys, Addressables.MergeMode.Union);
            await locations.Task;

            var containLocationKey = new List<string>();

            foreach (var location in locations.Result)
            {
                var locationKey = location.PrimaryKey;

                if (keys.Contains(locationKey))
                {
                    containLocationKey.Add(locationKey);
                }
            }

            var recipeOp = UMAAssetIndexer.Instance.LoadLabelList(containLocationKey, true, id.ToString(), (downloadStatus) =>
            {
                resourceDownloadStatusEvent.Post(avatarResource, downloadStatus);
            });

            await recipeOp;

            if (recipeOp.Result != null)
            {
                Recipes_Loaded(avatarResource, recipeOp.Result, catalogPath);
                PostLoadAvatarResourceAddressableEvent(avatarResource);
            }
        }

        private void Recipes_Loaded(AvatarResource avatarResource, IList<UnityEngine.Object> obj, string path)
        {
            var resId = avatarResource.id;
            var uniqueObject = new HashSet<UnityEngine.Object>();

            if (obj == null)
            {
                return;
            }

            foreach (var ob in obj)
            {
                if (!uniqueObject.Contains(ob))
                {
                    uniqueObject.Add(ob);
                }
            }

            foreach (var Each in uniqueObject)
            {
                if (Each != null && !Each.name.ToLower().Contains(ExceptionKeyword.placeholder.ToString()))
                {
                    switch (Each)
                    {
                        case UMAWardrobeRecipe umaWardrobeRecipe:
                            AddAvatarResDict(resId, UMAWardrobeRecipe, umaWardrobeRecipe);
                            break;
                        case OverlayDataAsset overlayDataAsset:
                            AddAvatarResDict(resId, OverlayDataAsset, overlayDataAsset);
                            UMAAssetIndexer.Instance.ProcessNewItem(Each, true, true, resId.ToString());
                            break;
                        case UMATextRecipe umaTextRecipe:
                            AddAvatarResDict(resId, UMATextRecipe, umaTextRecipe);
                            break;
                        case RaceData raceData:
                            AddAvatarResDict(resId, RaceData, raceData);
                            break;
                        case UmaTPose umaTPose:
                            AddAvatarResDict(resId, UmaTPose, umaTPose);
                            break;
                        case DynamicUMADnaAsset dynamicUMADnaAsset:
                            AddAvatarResDict(resId, DynamicUMADnaAsset, dynamicUMADnaAsset);
                            break;
                        case DynamicDNAConverterController dynamicDNAConverterController:
                            AddAvatarResDict(resId, DynamicDNAConverterController, dynamicDNAConverterController);
                            break;
                        case Texture2D texture2D:
                            AddAvatarResDict(resId, Texture2D, texture2D);
                            break;
                        case SlotDataAsset slotdata:
                            AddAvatarResDict(resId, SlotData, slotdata);
                            //SetRestAppIdDict(slotdata.slotName, path);

                            UMAAssetIndexer.Instance.ProcessNewItem(Each, true, true, resId.ToString());
                            break;
                    }
                }
            }
        }
        public string GetAvatarHistory() {
            var lastIndex = avatarUpdateHistoryList.Count() - 1;
            if (lastIndex < 0) return null;

            var temp = avatarUpdateHistoryList[lastIndex];
            
            avatarUpdateHistoryList.Remove(temp);

            return temp;
        }

        public int AvatarHistoryCount() {
            return avatarUpdateHistoryList.Count();
        }

        public void SetAvatarHistory(string recipe)
        {
            var lastIndex = avatarUpdateHistoryList.Count() - 1;

            if (lastIndex < 0)
            {
                avatarUpdateHistoryList.Add(recipe);
                return;
            }

            bool isDuplicated = avatarUpdateHistoryList[lastIndex].Equals(recipe);
            if (isDuplicated)
            {
                Debug.Log($"SetAvatarHistory isDuplicated");
            }
            else {
                avatarUpdateHistoryList.Add(recipe);
            }

            Debug.Log($"avatarUpdateHistoryList count :{avatarUpdateHistoryList.Count()}");
        }

        public void AvatarHistoryClear()
        {
            avatarUpdateHistoryList.Clear();
        }

        private void PostLoadAvatarResourceAddressableEvent(AvatarResource avatarResource)
        {
            loadAvatarResourceAddressableEvent.Post(avatarResource);
        }

        private void AddAvatarResDict<T>(long id, Dictionary<long, T> dict, T item)
        {
            if (!dict.TryGetValue(id, out T value))
            {
                dict[id] = item;
            }
        }
    }
}
