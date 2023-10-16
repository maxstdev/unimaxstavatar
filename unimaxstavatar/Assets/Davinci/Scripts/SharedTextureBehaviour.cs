using UnityEngine;
using UnityImageLoader.Cache;

public class SharedTextureBehaviour : MonoBehaviour
{
	private SharedTexture SharedTexture { get; set; } = null;

	~SharedTextureBehaviour()
	{
		SharedTexture?.Release();
	}

	public void Config(SharedTexture sharedTexture)
	{
		SharedTexture?.Release();
		SharedTexture = sharedTexture;
	}
}
