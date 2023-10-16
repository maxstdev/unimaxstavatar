using System;
using System.Threading;
using UnityEngine;

namespace UnityImageLoader.Cache
{
	public class SharedTexture
	{
		public Texture2D Texture { get; private set; }
		public int Size { get; private set; }
		public readonly string url;
		private int refCount = 0;
		private ICache<SharedTexture> memoryCache = null;
		private readonly WeakReference<DavinciFile> wpDavinciFile = new WeakReference<DavinciFile>(null);

		public SharedTexture(Texture2D texture, string url)
		{
			Texture = texture;
			this.url = url;
			refCount = 0;
			Size = Texture != null ? Texture.width * Texture.height * 4 : 0;
			//Debug.Log($"SharedTexture, set refCount : {refCount}, url : {url}");
		}

		~SharedTexture()
		{
			if (wpDavinciFile.TryGetTarget(out DavinciFile davinciFile))
			{
				davinciFile.OnChanged = null;
			}
		}

		public void Set(ICache<SharedTexture> memoryCache)
		{
			this.memoryCache = memoryCache;
			memoryCache?.Set(url, this);
		}

		public void SetReferenceFile(DavinciFile davinciFile)
		{
			wpDavinciFile.SetTarget(davinciFile);
			if (davinciFile != null)
			{
				davinciFile.OnChanged = OnChanged;
			}
		}

		public SharedTexture Retain()
		{
			if (Texture != null)
			{
				Interlocked.Increment(ref refCount);
				//Debug.Log($"SharedTexture, Retain refCount : {refCount}, url : {url}");
				
			}
			return this;
		}

		public void Release()
		{
			if (Texture != null)
			{
				if (Interlocked.Decrement(ref refCount) < 1)
				{
					Texture = null;
					memoryCache?.Release(url);
				}
				//Debug.Log($"SharedTexture, Decrement refCount : {refCount}, url : {url}");
			}
		}

		public int GetRefCount()
		{
			return refCount;
		}

		private void OnChanged(DavinciFile davinciFile)
		{
			SetReferenceFile(davinciFile);
			if (davinciFile != null && Texture != null)
			{
				Texture.LoadImage(davinciFile.Load());
			}
		}
	}
}
