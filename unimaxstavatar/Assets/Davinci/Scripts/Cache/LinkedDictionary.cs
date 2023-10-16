using System.Collections.Generic;
using System.Threading;

namespace UnityImageLoader.Cache
{
	public class LinkedDictionary<K, V>
	{
		private readonly ReaderWriterLockSlim lockslim;
		private readonly IDictionary<K, V> dictionary;
		private readonly LinkedList<K> linkedList;


		public LinkedDictionary()
		{
			lockslim = new ReaderWriterLockSlim();
			dictionary = new Dictionary<K, V>();
			linkedList = new LinkedList<K>();
		}


		public void Set(K key, V value)
		{
			lockslim.EnterWriteLock();
			try
			{
				if (key == null)
				{
					return;
				}

				if (linkedList.Contains(key))
				{
					linkedList.Remove(key);
					dictionary.Remove(key);
				}
				dictionary.Add(key, value);
				linkedList.AddFirst(key);
			}
			finally
			{
				lockslim.ExitWriteLock();
			}
		}

		public K GetTailKey()
		{

			return linkedList.Last.Value;
		}

		public bool TryGet(K key, out V value)
		{
			lockslim.EnterUpgradeableReadLock();
			try
			{
				return dictionary.TryGetValue(key, out value);
			}
			finally
			{
				lockslim.ExitUpgradeableReadLock();
			}
		}

		public bool TryGetAndMoveFirst(K key, out V value)
		{
			lockslim.EnterUpgradeableReadLock();
			try
			{
				bool b = dictionary.TryGetValue(key, out value);
				if (b)
				{
					lockslim.EnterWriteLock();
					try
					{
						linkedList.Remove(key);
						linkedList.AddFirst(key);
					}
					finally
					{
						lockslim.ExitWriteLock();
					}
				}
				return b;
			}
			finally
			{
				lockslim.ExitUpgradeableReadLock();
			}
		}

		public void Remove(K key)
		{
			lockslim.EnterWriteLock();
			try
			{
				if (key == null)
				{
					return;
				}

				if (linkedList.Contains(key))
				{
					linkedList.Remove(key);
					dictionary.Remove(key);
				}

			}
			finally
			{
				lockslim.ExitWriteLock();
			}
		}

		public void RemoveLast()
		{
			lockslim.EnterWriteLock();
			try
			{
				dictionary.Remove(linkedList.Last.Value);
				linkedList.RemoveLast();

			}
			finally
			{
				lockslim.ExitWriteLock();
			}
		}

		public void RemoveAll()
		{
			lockslim.EnterWriteLock();
			try
			{
				dictionary.Clear();
				linkedList.Clear();
			}
			finally
			{
				lockslim.ExitWriteLock();
			}
		}


		public int Count
		{
			get
			{
				return linkedList.Count;
			}
		}
	}
}