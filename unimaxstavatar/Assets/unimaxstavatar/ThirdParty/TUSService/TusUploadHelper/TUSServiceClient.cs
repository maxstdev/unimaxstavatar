using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace TUSService
{
    public class TUSServiceClient
    {
        private string filePath = "";

        public Action<int> OnUploadProgress;
        public Action<string> completed;
        public Action OnUploadError;
        
        public void Init(Action<int> OnUploadProgress, Action<string> completed, Action OnUploadError)
        {
            this.completed = completed;
            this.OnUploadProgress = OnUploadProgress;
            this.OnUploadError = OnUploadError;
        }

        public async UniTask<string> StartTUSUpload(string token, string sub, string filePath, bool isShowUploadPrecent)
        {
            UniTaskCompletionSource<string> uniTaskCompletionSource = new();

            int debouncePercent = -1;
            this.filePath = filePath;
            new TUSUploadHelper(token)
                .SetSub(sub)
                .SetFilePath(filePath)
                .SetProgressAction(
                (percent) =>
                {
                    if (debouncePercent != percent && isShowUploadPrecent)
                    {
                        debouncePercent = percent;
                        OnUploadProgress?.Invoke(percent);
                    }
                })
                .SetErrorAction((error) =>
                {
                    Debug.Log(error);
                    OnUploadError?.Invoke();
                    uniTaskCompletionSource.TrySetException(new Exception(error.ToString()));
                })
                .SetCompletedAction(
                (key) =>
                {
                    Debug.Log("Completed!! return Key : " + key);
                    completed?.Invoke(key);
                    uniTaskCompletionSource.TrySetResult(key);
                })
                .Resume();

            return await uniTaskCompletionSource.Task;
        }
    }
}
