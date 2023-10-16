using System;
using UnityEngine;
using UnityImageLoader.Cache;

public interface DavinciDelegate
{
	void DownloaderTexture(string url, Action<SharedTexture> complete, Action<string> error, Action<int> progress);
	void DownloaderFile(string url, Action<SharedTexture> complete, Action<string> error, Action<int> progress);
	void ImageLoader(SharedTexture sharedTexture);
	void OnProgressChange(int progress);
	void OnErrorMessage(string message);
}
