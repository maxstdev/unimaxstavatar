/*==============================================================================
Copyright 2017 Maxst, Inc. All Rights Reserved.
==============================================================================*/

using UnityEditor;
using UnityEditor.Callbacks;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

// Unity Xcode Project Document.
// https://docs.unity3d.com/ScriptReference/iOS.Xcode.PBXProject.html
public class PostProcessBuild
{
    [PostProcessBuildAttribute(50)]
    public static void ChangeXcodePlist(BuildTarget buildTarget, string pathToBuiltProject)
    {
        if ( buildTarget == BuildTarget.iOS )
        {
#if UNITY_IOS
            // Plist File Setting.
            string plistPath = pathToBuiltProject + "/Info.plist";
            PlistDocument plist = new PlistDocument();
            plist.ReadFromFile(plistPath);

            PlistElementDict rootDict = plist.root;
            
            var schemeArray = rootDict.CreateArray("LSApplicationQueriesSchemes");
            schemeArray.AddString("com.maxst.maxlogin");

            plist.WriteToFile(plistPath);
#endif
        }
    }
}
