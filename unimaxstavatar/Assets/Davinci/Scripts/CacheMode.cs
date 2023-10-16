using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public enum CacheMode : int
{
	NonCache				= 0b0000000000000000,
	MemoryCache				= 0b0000000000000001,
	//FileCache				= 0b0000000000000010, //Not used
	MemoryAndFileCache		= 0b0000000000000011,
}


public static class CacheModeEx
{
	public static bool IsValid(this CacheMode cacheMode, CacheMode diff)
	{
		return (cacheMode.ToInt() & diff.ToInt()) != 0;
	}

	public static int ToInt(this CacheMode c)
	{
		return (int)c;
	}
}
