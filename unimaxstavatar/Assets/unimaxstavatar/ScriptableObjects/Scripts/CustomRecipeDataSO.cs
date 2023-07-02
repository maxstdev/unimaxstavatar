using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Maxst.Avatar
{
    [CreateAssetMenu(fileName = "CustomRecipeData", menuName = "ScriptableObjects/CustomRecipeDataSO")]
    public class CustomRecipeDataSO : ScriptableObject
    {
        [SerializeField] private List<UMA.UMATextRecipe> recipeList;

        public List<UMA.UMATextRecipe> GetRecipeList
        {
            get
            {
                if (recipeList == null)
                {
                    return recipeList = new List<UMA.UMATextRecipe>();
                }

                return recipeList;
            }
        }
    }
}