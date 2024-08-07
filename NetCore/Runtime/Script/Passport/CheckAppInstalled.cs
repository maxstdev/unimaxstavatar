using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class CheckAppInstalled
{
#if UNITY_IOS && !UNITY_EDITOR                           
    [DllImport("__Internal")]
    private static extern bool IsAppInstalled(string urlScheme);
#endif
    private const string UNITY_PLAYER_CLASS = "com.unity3d.player.UnityPlayer";
    public bool CheckIfAppInstalled(string urlScheme, string packageName)
    {
#if UNITY_IOS && !UNITY_EDITOR
        string scheme = urlScheme.Split("://")[0];
        return IsAppInstalled(scheme);
#elif UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaClass unityPlayer = new AndroidJavaClass(UNITY_PLAYER_CLASS);
        AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject packageManager = currentActivity.Call<AndroidJavaObject>("getPackageManager");
        AndroidJavaObject launchIntent = null;
        launchIntent = packageManager.Call<AndroidJavaObject>("getLaunchIntentForPackage", packageName);

        return launchIntent == null ? false : true;
#endif
        return false;
    }

}
