using System.Threading;
using UnityEngine;

namespace UnityImageLoader.Cache
{
	public abstract class AbstractCache<T>
	{
		public const long MINIMUM_CAPACITY = 1024 * 1024 * 16;

		public readonly ReaderWriterLockSlim lockslim;
		protected readonly LinkedDictionary<string, T> linkedDictionary;

		public long Size { get; protected set; } = 0;
		protected long Capacity { get; private set; } = MINIMUM_CAPACITY;

		protected AbstractCache() : this(MINIMUM_CAPACITY)
		{
		}

		protected AbstractCache(long capacity)
		{
			SetCapacity(capacity);
			linkedDictionary = new LinkedDictionary<string, T>();
			lockslim = new ReaderWriterLockSlim();
		}

		public void SetCapacity(long capacity)
		{
			Capacity = System.Math.Max(MINIMUM_CAPACITY, capacity);
		}

		public void Clear()
		{
			linkedDictionary.RemoveAll();
			Size = 0;
		}

		public abstract int ToSize(T t);

		public abstract T Hit(string url);

		public abstract void OnDrop(T t);

		protected void TrimToSize()
		{
			while (true)
			{
				lockslim.EnterWriteLock();
				try
				{
					if (linkedDictionary.Count == 0)
					{
						break;
					}

					if (Size <= Capacity)
					{
						break;
					}

					string tailKey = linkedDictionary.GetTailKey();
					if (linkedDictionary.TryGet(tailKey, out T tailValue))
					{
						Size -= ToSize(tailValue);
						linkedDictionary.RemoveLast();
						OnDrop(tailValue);
					}
				}
				finally
				{
					lockslim.ExitWriteLock();
				}
			}
		}
	}
}

