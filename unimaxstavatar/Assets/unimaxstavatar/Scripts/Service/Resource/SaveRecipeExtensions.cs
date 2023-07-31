using Maxst.Token;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Maxst.Resource
{
    [Serializable]
    public class SaveRecipeExtensions
    {
        [JsonProperty("saveRecipeString")]
        [SerializeField]
        public string saveRecipeString;

        [JsonProperty("wardrobePaths")]
        [SerializeField]
        public List<WardrobePath> wardrobePaths;

        [Serializable]
        public class WardrobePath
        {
            [JsonProperty("slot")]
            public string slot;

            [JsonProperty("clientId")]
            public string clientId;

            [JsonProperty("recipe")]
            public string recipe;
        }

        public void SetSaveRecipeString(string saveRecipeString)
        {
            this.saveRecipeString = saveRecipeString;
        }

        public void SetSlotPath(string clientId, string beforeSaveRecipeString)
        {
            wardrobePaths = new List<WardrobePath>();
            SaveRecipe saveRecipe = JsonUtility.FromJson<SaveRecipe>(saveRecipeString);

            SaveRecipeExtensions beforeSaveRecipe = JsonUtility.FromJson<SaveRecipeExtensions>(beforeSaveRecipeString);

            wardrobePaths.AddRange(saveRecipe.wardrobeSet.Select(wardrobe =>
            {
                string storedClientId = beforeSaveRecipe?.wardrobePaths.FirstOrDefault(beforeWardrobe =>
                                beforeWardrobe.slot == wardrobe.slot && beforeWardrobe.recipe == wardrobe.recipe)?.clientId;

                var WardrobePath = new WardrobePath();
                WardrobePath.slot = wardrobe.slot;
                WardrobePath.recipe = wardrobe.recipe;
                WardrobePath.clientId = storedClientId == null ? clientId : storedClientId;

                return WardrobePath;
            }));
        }
        public void SetSlotPath(string clientId)
        {
            wardrobePaths = new List<WardrobePath>();
            SaveRecipe saveRecipe = JsonUtility.FromJson<SaveRecipe>(saveRecipeString);

            wardrobePaths.AddRange(saveRecipe.wardrobeSet.Select(wardrobe =>
            {
                var WardrobePath = new WardrobePath();
                WardrobePath.slot = wardrobe.slot;
                WardrobePath.recipe = wardrobe.recipe;
                WardrobePath.clientId = clientId;

                return WardrobePath;
            }));
        }
    }
}
