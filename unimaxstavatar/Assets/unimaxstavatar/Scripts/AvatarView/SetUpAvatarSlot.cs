using System.Collections.Generic;
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


        public SetUpAvatarSlot(DynamicCharacterAvatar avatar, UMATextRecipe recipe, List<UMATextRecipe> list)
        {
            this.avatar = avatar;
            this.recipe = recipe;
            recipeList = list;
        }


        public void Execute()
        {
            previousRecipeList.AddRange(recipeList);
            RefreshCheck();

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

            SetAvatar();
        }

        public void SetAvatar()
        {
            avatar.ClearSlots();

            if (avatar.IsAvatarSaveData())
            {
                avatar.MaxstDoLoad();
            }
            
            avatar.DefaultTextRecipe?.ForEach(recipe =>
            {
                avatar.SetSlot(recipe);
            });

            recipeList?.ForEach(recipe =>
            {
                avatar.SetSlot(recipe);
            });
            avatar.BuildCharacter(true);
        }
    }
}