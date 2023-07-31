using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Maxst.Resource
{
    [Serializable]
    public class SaveRecipe
    {
        [JsonProperty("wardrobeSet")]
        public List<Wardrobe> wardrobeSet;
        /*
        [JsonProperty("packedRecipeType")]
        public string packedRecipeType;
        
        [JsonProperty("name")]
        public string name;
        
        [JsonProperty("race")]
        public string race;
        
        [JsonProperty("characterColors")]
        public string characterColors;
        
        [JsonProperty("raceAnimatorController")]
        public string raceAnimatorController;*/

        [Serializable]
        public class Wardrobe
        {
            [JsonProperty("slot")]
            public string slot;

            [JsonProperty("recipe")]
            public string recipe;
        }
    }
}