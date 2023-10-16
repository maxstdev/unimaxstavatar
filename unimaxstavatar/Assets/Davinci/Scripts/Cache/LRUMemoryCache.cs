using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityImageLoader.Cache
{
	public class LRUMemoryCache : AbstractCache<SharedTexture>, ICache<SharedTexture>
	{
		

		public LRUMemoryCache(long capacity) : base(capacity)
		{
			
		}

		void ICache<SharedTexture>.Set(string url, SharedTexture sharedTexture)
		{
			if (sharedTexture == null)
			{
				return;
			}

			if (linkedDictionary.TryGet(url, out SharedTexture previous))
			{
				previous.Set(null);
				Size -= ToSize(previous);
			}

			Size += ToSize(sharedTexture);
			linkedDictionary.Set(url, sharedTexture);

			TrimToSize();
		}

		SharedTexture ICache<SharedTexture>.Get(string url)
		{
			if (linkedDictionary.TryGet(url, out SharedTexture sharedTexture))
			{
				return sharedTexture;
			}
			return null;
		}

		void ICache<SharedTexture>.Release(string url)
		{
			if (linkedDictionary.TryGet(url, out SharedTexture sharedTexture))
			{
				if (sharedTexture.GetRefCount() < 1)
				{
					Size -= ToSize(sharedTexture);
					linkedDictionary.Remove(url);
					//Debug.Log($"LRUMemoryCache, Size : {Size}, url : {url}");
				}
			}
		}

		public override SharedTexture Hit(string url)
		{
			if (linkedDictionary.TryGetAndMoveFirst(url, out SharedTexture sharedTexture))
			{
				return sharedTexture;
			}
			return null;
		}

		public override int ToSize(SharedTexture sharedTexture)
		{
			return sharedTexture?.Size ?? 0;
		}

		public override void OnDrop(SharedTexture sharedTexture)
		{
			sharedTexture.Set(null);
		}
	}
}
