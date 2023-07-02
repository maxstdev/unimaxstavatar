using System;
using System.Collections;
using System.Collections.Generic;
using MaxstUtils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Maxst.Avatar
{
    public enum SceneType
    {
        //login,
        //pc_login,

        loginView,
        selectView,
        avatarView,

        //uma_Addressables,
        //uma_DynamicDNAConverterControllerDemo,
    }

    [CreateAssetMenu(fileName = "SceneScriptableObjects",
        menuName = "ScriptableObjects/SceneScriptableObjects",
        order = 3)]
    public class SceneScriptableObjects : ScriptableObject
    {

        [SerializeField]
        private SerializeEnumDictionary<SceneType, SceneField> sceneDictionary
            = new SerializeEnumDictionary<SceneType, SceneField>();

        public SerializeEnumDictionary<SceneType, SceneField> SceneDictionary
        {
            get
            {
                return sceneDictionary;
            }
        }

        public void LoadScene(SceneType type)
        {
            SceneManager.LoadScene(sceneDictionary[type.ToString()].BuildIndex);
        }

        public SceneType FindSceneType(int buildIndex)
        {
            var ret = SceneType.selectView;
            foreach (var entry in sceneDictionary)
            {
                if (buildIndex == entry.Value.BuildIndex)
                {
                    ret = (SceneType)Enum.Parse(typeof(SceneType), entry.Key);
                    break;
                }
            }
            return ret;
        }
    }
}