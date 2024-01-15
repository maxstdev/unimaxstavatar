using System.Collections.Generic;
using UMA;
using UMA.CharacterSystem;
using UnityEngine;

namespace Maxst.Avatar
{
    public class AssetAreaData
    {
        public string resId;
        public Sprite thumbnail;
        public string slotName;
        public string recipeString;
        public bool isSelect;
        public string thumbnailpath;
    }

    public class UMAAssetData
    {
        private List<string> hideWardrobeOptions = new List<string>()
    { "Underwear", "UnderwearBottom", "UnderwearTop", "UtilityWaistHide", "UtilitySleeveHide",  "UtilityPantsHide", "UtilityHairHide", "UtilityFeetSocksHide"};

        private Dictionary<string, List<UMATextRecipe>> currentRaceRecipes;
        private List<UMATextRecipe> currentSlotList;
        private string currentRacename;

        private List<string> GetInitializeWardrobeRace()
        {
            List<string> slotsFromAllRaces = new List<string>();

            List<RaceData> races = UMAAssetIndexer.Instance.GetAllAssets<RaceData>();

            foreach (RaceData rd in races)
            {
                if (rd == null)
                {
                    continue;
                }

                string race = rd.raceName;
                int i = 0;
                var recipes = UMAAssetIndexer.Instance.GetRecipes(race);
                foreach (string slot in recipes.Keys)
                {
                    if (!slotsFromAllRaces.Contains(slot) && !hideWardrobeOptions.Contains(slot))
                    {
                        slotsFromAllRaces.Insert(i, slot);
                        i++;
                    }
                }
            }
            return slotsFromAllRaces;
        }

        public List<string> GetWardrobeSlotList(string raceName)
        {
            List<string> wardrobeList = new List<string>();

            currentRacename = raceName;
            currentRaceRecipes = UMAAssetIndexer.Instance.GetRecipes(raceName);

            var slotAllRaces = GetInitializeWardrobeRace();

            foreach (var thisSlot in slotAllRaces)
            {
                if (currentRaceRecipes.ContainsKey(thisSlot))
                {
                    wardrobeList.Add(thisSlot);
                }
            }

            return wardrobeList;
        }

        public List<AssetAreaData> GetAssetData(string wardrobeslotName)
        {
            var assetdatas = new List<AssetAreaData>();

            currentRaceRecipes = UMAAssetIndexer.Instance.GetRecipes(currentRacename);
            if (currentRaceRecipes.ContainsKey(wardrobeslotName))
            {
                currentSlotList = new List<UMATextRecipe>(currentRaceRecipes[wardrobeslotName]);

                foreach (var slot in currentSlotList)
                {
                    string resId = slot.name;
                    string slotname = slot.DisplayValue != "" ? slot.DisplayValue : slot.name;
                    Sprite slotThumb = slot.GetWardrobeRecipeThumbFor(currentRacename);

                    assetdatas.Add(new AssetAreaData { 
                        resId = resId, 
                        slotName = slotname, 
                        thumbnail = slotThumb, 
                        recipeString = slot.recipeString 
                    });
                }
            }

            return assetdatas;
        }

        public UMATextRecipe GetTextRecipe(string slotName)
        {
            UMATextRecipe recipe = null;

            foreach (var slot in currentSlotList)
            {
                if (slotName == slot.name)
                {
                    recipe = slot;
                }
            }

            return recipe;
        }

    }
}