using System.IO;
using System;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityImageLoader.Cache
{
	public class LRUDiscCache : AbstractCache<DavinciFile>, ICache<DavinciFile>
	{
		public LRUDiscCache(long capacity) : base(capacity)
		{

		}

		void ICache<DavinciFile>.Set(string path, DavinciFile davinciFile)
		{
			if (davinciFile == null)
			{
				return;
			}

			if (linkedDictionary.TryGet(path, out DavinciFile previous))
			{
				previous.Set(null);
				Size -= ToSize(previous);
			}

			Size += ToSize(davinciFile);
			linkedDictionary.Set(path, davinciFile);

			previous?.OnChanged?.Invoke(davinciFile);

			TrimToSize();
		}

		DavinciFile ICache<DavinciFile>.Get(string path)
		{
			if (linkedDictionary.TryGetAndMoveFirst(path, out DavinciFile davinciFile))
			{
				if (File.Exists(davinciFile.Path))
				{
					return davinciFile;
				}
				else
				{
					davinciFile.Release();
					return null;
				}
			}
			return null;
		}

		void ICache<DavinciFile>.Release(string path)
		{
			if (linkedDictionary.TryGet(path, out DavinciFile davinciFile))
			{
				Size -= ToSize(davinciFile);
				linkedDictionary.Remove(path);
			}
		}

		public async void OnPrepared(string folder)
		{
			await Task.Yield();
			lockslim.EnterWriteLock();
			try
			{
				if (!Directory.Exists(folder))
				{
					return;
				}

				var files = Directory.GetFiles(folder);
				foreach (var file in files)
				{
					var davinciFile = new DavinciFile(file, (int)new FileInfo(file).Length);
					Size += ToSize(davinciFile);
					linkedDictionary.Set(file, davinciFile);
				}
			}
			catch (Exception ex)
			{
				Debug.LogWarning(ex);
			}
			finally
			{
				lockslim.ExitWriteLock();
			}
			TrimToSize();
		}

		public override DavinciFile Hit(string path)
		{
			if (!File.Exists(path))
			{
				(this as ICache<DavinciFile>).Release(path);
				return null;
			}

			if (!linkedDictionary.TryGetAndMoveFirst(path, out var result))
			{
				result = new DavinciFile(path, (int)new FileInfo(path).Length);
				result.Set(this);
			}
			return result;
		}

		public override int ToSize(DavinciFile davinciFile)
		{
			return davinciFile?.Size ?? 0;
		}

		public override void OnDrop(DavinciFile davinciFile)
		{
			davinciFile.Release();
			davinciFile.Delete();
			davinciFile.Set(null);
		}
	}
}
