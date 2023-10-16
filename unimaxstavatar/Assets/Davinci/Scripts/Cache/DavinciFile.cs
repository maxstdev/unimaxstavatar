using System;
using System.IO;
using UnityEngine;

namespace UnityImageLoader.Cache
{
	public class DavinciFile
	{
		public string Path { get; private set; }
		public int Size { get; private set; }
		public Action<DavinciFile> OnChanged { get; set; } = null;

		private ICache<DavinciFile> fileCache = null;

		public DavinciFile(string path, int size)
		{
			Path = path;
			Size = size;
		}

		public void Set(ICache<DavinciFile> fileCache)
		{
			this.fileCache = fileCache; 
			fileCache?.Set(Path, this);
		}

		public byte[] Load()
		{
			if (File.Exists(Path)) return File.ReadAllBytes(this.Path);
			else return null;
		}

		public void Save(byte[] data)
		{
			File.WriteAllBytes(Path, data);
		}

		public void Delete()
		{
			if (File.Exists(Path)) File.Delete(Path);
		}

		public void Release()
		{
			fileCache?.Release(Path);
		}
	}
}
