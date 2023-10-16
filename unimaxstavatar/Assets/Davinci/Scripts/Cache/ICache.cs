namespace UnityImageLoader.Cache
{
	public interface ICache<V>
	{
		void Set(string key, V value);
		V Get(string key);
		void Release(string url);
	}
}