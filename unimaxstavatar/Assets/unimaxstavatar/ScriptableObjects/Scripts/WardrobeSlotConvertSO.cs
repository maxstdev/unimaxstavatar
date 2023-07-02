using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maxst.Avatar
{
    [Serializable]
    public class WardrobeSlotConvertData
    {
        public WardrobeSlotKoreanName koreanName;
        public Sprite defaultIcon;
        public Sprite selectIcon;
    }

    [CreateAssetMenu(fileName = "WardrobeSlotConvertResource", menuName = "ScriptableObjects/WardrobeSlotConvertSO")]
    public class WardrobeSlotConvertSO : ScriptableObject
    {
        [SerializeField] private SerializeEnumDictionary<AvatarWardrobeSlot, WardrobeSlotConvertData> convertData = new();
        [SerializeField] private SerializeEnumDictionary<AvatarWardrobeSlot, ViewType> viewType = new();
        public WardrobeSlotConvertData GetEachSlotData(string key)
        {
            WardrobeSlotConvertData value = new WardrobeSlotConvertData();

            if (convertData.TryGetValue(key.ToString(), out value))
            {
                return value;
            }

            return value;
        }

        public ViewType GetFaceBodyValue(string key)
        {
            ViewType value = ViewType.None;
            viewType.TryGetValue(key.ToString(), out value);
            return value;
        }
    }
}