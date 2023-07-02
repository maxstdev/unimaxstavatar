using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maxst.Avatar
{
    public enum AvatarWardrobeSlot
    {
        None,
        Face,
        Glasses,
        Hair,
        Complexion,
        Eyebrows,
        Beard,
        Ears,
        Helmet,
        Shoulders,
        Chest,
        Arms,
        Hands,
        Waist,
        Legs,
        Feet,
        Nose
    }

    public enum WardrobeSlotKoreanName
    {
        None,
        얼굴형,
        안경,
        헤어스타일,
        안색,
        눈썹,
        수염,
        귀,
        모자,
        어깨,
        상의,
        팔,
        손,
        허리,
        하의,
        신발,
        코
    }

    public enum AvatarPatial
    {
        Face,
        Body,
        Leg,
        Feet
    }

    [CreateAssetMenu(fileName = "WardrobeSlotResource", menuName = "ScriptableObjects/WardrobeSlotSO")]
    public class WardrobeSlotSO : ScriptableObject
    {
        [SerializeField] private SerializeEnumDictionary<AvatarWardrobeSlot, AvatarPatial> selectRaceDataWardrobeName = new();
        [SerializeField] private SerializeEnumDictionary<AvatarWardrobeSlot, WardrobeSlotKoreanName> koreanName = new();

        public AvatarPatial GetAvatarPatialName(string key)
        {
            AvatarPatial value;

            if (selectRaceDataWardrobeName.TryGetValue(key.ToString(), out value))
            {
                return value;
            }
            return AvatarPatial.Body;
        }

        public string GetKoreanName(string key)
        {
            WardrobeSlotKoreanName value;

            if (koreanName.TryGetValue(key.ToString(), out value))
            {
                return value.ToString();
            }

            return key.ToString();
        }
    }
}