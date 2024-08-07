using Cysharp.Threading.Tasks;
using Maxst.Passport;
using Maxst.Token;
using System.Threading.Tasks;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Maxst.Avatar
{
    public class AvatarCapture : InjectorBehaviour
    {
        [DI(DIScope.singleton)] protected AvatarCustomViewModel avatarCustomViewModel { get; }

        [SerializeField] private RenderTexture renderTexture;
        [SerializeField] private RawImage testRawImage;

        private void Start()
        {
            avatarCustomViewModel.CaptureExecute.AddObserver(this, Capture);
        }
        public void Capture()
        {
            AsyncCapture().Forget();
        }

        public async UniTaskVoid AsyncCapture()
        {
            var accessToken = GetToken().accessToken;
            var uuid = GetToken().idTokenDictionary.GetTypedValue<string>(JwtTokenConstants.sub);
            var filename = "profileImage";

            var texture = InitTexture2D(renderTexture);
            byte[] pngFile = ConvertPNG(texture);

            var profile = await PutAvatarProfileDownLoadUrl(accessToken, uuid, filename);

            await PutProfilePNG(profile.uploadUrl, pngFile);

            await PatchAvatarProfile(accessToken, uuid, filename);

            RenderTexture.active = null;
        }

        private Texture2D InitTexture2D(RenderTexture rt)
        {
            Texture2D tex = new(rt.width, rt.height, TextureFormat.ARGB32, false, true);
            RenderTexture.active = rt;
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);

            return tex;
        }

        private byte[] ConvertPNG(Texture2D texture)
        {
            return texture.EncodeToPNG();
        }

        public async UniTask<AvatarProfile> PutAvatarProfileDownLoadUrl(string accessToken, string uuid, string filename)
        {
            TaskCompletionSource<AvatarProfile> taskCompletionSource = new();

            AvatarProfile avatarProfile = new AvatarProfile() { profileUuid = uuid, imageFileName = filename };

            ProfileService.Instance.PutAvatarProfileURL($"Bearer {accessToken}", avatarProfile)
                .ObserveOn(Scheduler.MainThread)
                .Subscribe(data =>
                {
                    Debug.Log($"[AvatarCapture] PutAvatarProfileDownLoadUrl data : {data}");
                    taskCompletionSource.TrySetResult(data);
                },
                error =>
                {
                    Debug.Log(error);
                    Debug.Log(error.Message);
                });

            return await taskCompletionSource.Task;
        }

        public async UniTask PutProfilePNG(string url, byte[] path)
        {
            using (UnityWebRequest webRequest = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPUT))
            {
                webRequest.uploadHandler = new UploadHandlerRaw(path);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "image/PNG");

                var asyncOperation = webRequest.SendWebRequest().ToUniTask();
                try
                {
                    await asyncOperation;

                    if (webRequest.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogError($"Error uploading image: {webRequest.error}");
                    }
                    else
                    {
                        Debug.Log($"Image uploaded successfully: {webRequest.downloadHandler.text}");
                    }
                }
                finally 
                { 
                    webRequest.Dispose(); 
                }
            }
        }

        public async UniTask<AvatarProfile> PatchAvatarProfile(string accessToken, string uuid, string filename)
        {
            TaskCompletionSource<AvatarProfile> taskCompletionSource = new();

            AvatarProfile avatarProfile = new AvatarProfile() { profileUuid = uuid, imageFileName = filename };

            ProfileService.Instance.PatchAvatarProfileURL($"Bearer {accessToken}", avatarProfile)
                .ObserveOn(Scheduler.MainThread)
                .Subscribe(data =>
                {
                    Debug.Log($"[AvatarCapture] PatchAvatarProfile data : {data}");
                    taskCompletionSource.TrySetResult(data);
                },
                error =>
                {
                    Debug.Log(error);
                    Debug.Log(error.Message);
                });

            return await taskCompletionSource.Task;
        }

        private Passport.Token GetToken()
        {
            return TokenRepo.Instance.GetToken();
        }
    }
}
