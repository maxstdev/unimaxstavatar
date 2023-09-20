using System;

namespace Maxst.Avatar
{
    public enum Category { Chest, Hair, Legs, Feet }

    public static class CategoryHelper
    {
        public static Category GetCategoryFromString(string categoryString)
        {
            if (Enum.TryParse<Category>(categoryString, out Category category))
            {
                return category;
            }
            else
            {
                throw new ArgumentException("[CategoryHelper] : Invalid Category string!");
            }
        }
    }
}
