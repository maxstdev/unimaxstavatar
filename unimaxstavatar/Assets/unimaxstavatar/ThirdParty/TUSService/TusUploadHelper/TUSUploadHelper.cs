using System;
using System.IO;
using TusDotNetClient;
using UnityEngine;

namespace TUSService
{
    public class TUSUploadHelper : TusClient
    {
        //private static readonly string baseURL = "https://alpha-asset.maxverse.io";
        //private static readonly string container = "avatar";

        private ResourceSettingSO setting = ResourceSettingSO.Instance;

        // private static readonly string FILENAME = "save_recipe";
        // private static readonly string uploadEndPoint = baseURL + "/" + asset +"/" + "tusUpload";

        private string sub;
        private string token;
        private string filePath;
        
        private Action<int> progressed;
        private Action<string> completed;
        private Action<Error> error;
        private string uploadStorageKey;

        public enum Error
        {
            EmptryUploadStoragePath,
            EmptyFilepath
        }

        public TUSUploadHelper(string token)
        {
            this.token = token;
        }

        public TUSUploadHelper SetSub(string sub) {
            this.sub = sub;
            return this;
        }

        public TUSUploadHelper SetFilePath(string filePath)
        {
            this.filePath = filePath;
            return this;
        }

        public TUSUploadHelper SetProgressAction(Action<int> progressed)
        {
            this.progressed = progressed;
            return this;
        }

        public TUSUploadHelper SetCompletedAction(Action<String> completed)
        {
            this.completed = completed;
            return this;
        }

        public TUSUploadHelper SetErrorAction(Action<Error> error)
        {
            this.error = error;
            return this;
        }

        public TUSUploadHelper ChangeToken(string token)
        {
            this.token = token;
            return this;
        }

        public void Resume()
        {
            StartUpload(this.filePath);
        }

        private async void StartUpload(string filePath)
        {
            Debug.Log("[TUSUploadHelper] : StartUpload");

            if (string.IsNullOrEmpty(filePath))
            {
                error.Invoke(Error.EmptyFilepath);
                Debug.Log("[TUSUploadHelper] : filePath empty");
                return;
            }

            FileInfo fileInfo = new(filePath);
            Debug.Log($"file name : {fileInfo.Name}");

            var uploadEndPoint = $"{setting.BaseUrl}/{sub}/{setting.Tusupload}"; 
            //var uploadEndPoint = $"{setting.BaseUrl}{setting.Container}{setting.Platform}/{setting.Tusupload}"; 

            Debug.Log($"[TUSUploadHelper] fileInfo : {fileInfo}");
            Debug.Log($"[TUSUploadHelper] filePath : {filePath}");
            Debug.Log($"[TUSUploadHelper] uploadEndPoint : {uploadEndPoint}");

            AdditionalHeaders.Add("Authorization", "Bearer " + token);
            AdditionalHeaders.Add("filename", fileInfo.Name);
            
            var uploadStoragePath = await CreateAsync(uploadEndPoint, fileInfo, new (string, string)[0]);
            string uploadStorageEndPoint = setting.BaseUrl + uploadStoragePath;
            
            Debug.Log($"[TUSUploadHelper] uploadStorageEndPoint : {uploadStorageEndPoint}");

            var uploadOperation = UploadAsync(url: uploadStorageEndPoint, file: fileInfo, chunkSize: 5D);

            uploadOperation.Progressed -= UploadProgressed;
            uploadOperation.Progressed += UploadProgressed;

            await uploadOperation;
            
            return;
        }

        private void UploadProgressed(long bytesTransferred, long bytesTotal)
        {
            //Debug.Log($"Transfer : {bytesTransferred} FileSize : {bytesTotal}");

            var uploadPercent = (int)Math.Round((double)(100 * bytesTransferred) / bytesTotal);

            progressed.Invoke(uploadPercent);

            //progressed.OnNext(uploadPercent);

            if (bytesTransferred == bytesTotal)
            {
                //Debug.Log("Upload Complete!!");
                completed.Invoke(uploadStorageKey);
            }
        }
    }
}
