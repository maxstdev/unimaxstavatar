using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using UMA;
using UMA.CharacterSystem;

namespace Maxst.Avatar
{
    public class SetUpAvatarSlot : ICommand
    {
        private DynamicCharacterAvatar avatar;
        private UMATextRecipe recipe;
        private List<UMATextRecipe> recipeList;
        private List<UMATextRecipe> previousRecipeList = new List<UMATextRecipe>();
        private List<UMATextRecipe> executeList;

        public SetUpAvatarSlot(DynamicCharacterAvatar avatar, UMATextRecipe recipe, List<UMATextRecipe> list)
        {
            this.avatar = avatar;
            this.recipe = recipe;
            recipeList = list;
        }

        private void MakeExecuteList()
        {
            NakedCheker nakedchecker = new NakedCheker();

            executeList = new List<UMATextRecipe>();
            foreach (var recipe in nakedchecker.GetNakedList(recipeList))
            {
                if (recipe.nakedstatus == SlotStatus.Hide)
                {
                    continue;
                }

                executeList.Add(recipe);
            }
        }

        public void Execute()
        {
            previousRecipeList.AddRange(recipeList);
            RefreshCheck();

            MakeExecuteList();

            SetAvatar();
        }

        private void RefreshCheck()
        {
            if (recipe == null)
            {
                recipeList.Clear();
            }
            else
            {
                recipeList.Add(recipe);
            }
        }

        public void Undo()
        {
            recipeList.Clear();
            recipeList.AddRange(previousRecipeList);

            MakeExecuteList();

            SetAvatar();
        }

        public void SetAvatar()
        {
            avatar.ClearSlots();

            executeList?.ForEach(recipe =>
            {
                avatar.SetSlot(recipe);
            });
            avatar.BuildCharacter(true);
        }
    }
}