using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Maxst.Resource
{
    [Serializable]
    public class SaveRecipeExtensions
    {
        [JsonProperty("recipeStr")]
        [SerializeField]
        public string recipeStr;

        [JsonProperty("itemIds")]
        [SerializeField]
        public List<long> itemIds;

        public void SetSaveRecipeString(string recipeStr)
        {
            this.recipeStr = recipeStr;
            SetItemId();
        }

        private void SetItemId()
        {
            SaveRecipe recipe = JsonUtility.FromJson<SaveRecipe>(this.recipeStr);
            itemIds = new List<long>();
            recipe.wardrobeSet.ForEach(each =>
            {
                if (long.TryParse(each.recipe, out var id))
                {
                    itemIds.Add(id);
                }
            });
        }
    }
}
