using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UMA;

#if UNITY_EDITOR

namespace Maxst.Resource
{

    public class RemoveAddressableUMA : EditorWindow
    {

        [MenuItem("CustomUMA/RemoveUMAAddressableDefine")]
        static void RemoveUMAAddressableDefine()
        {
            var defineSymbols = new HashSet<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup).Split(';'));

            if (defineSymbols.Contains("UMA_ADDRESSABLES"))
            {
                defineSymbols.Remove("UMA_ADDRESSABLES");
            }
            else
            {
                Debug.Log("Not UMA Define Symbol");
                return;
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, string.Join(";", defineSymbols));

            Debug.Log("Remove UMA_ADDRESSABLES \n Global Library Rebuild Form Project Button Click");
        }
    }
}
#endif
