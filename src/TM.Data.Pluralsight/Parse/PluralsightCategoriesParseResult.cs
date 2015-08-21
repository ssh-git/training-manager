using System.Collections.Generic;
using TM.Shared;
using TM.Shared.Parse;

namespace TM.Data.Pluralsight
{
   internal class PluralsightCategoriesParseResult : ICategoriesParseResult<PluralsightCategory>
   {
      public PluralsightCategoriesParseResult(Dictionary<ICategoryUrlNameNaturalKey, PluralsightCategory> categoryContainer)
      {
         CategoryContainer = categoryContainer;
      }

      public Dictionary<ICategoryUrlNameNaturalKey, PluralsightCategory> CategoryContainer { get;  private set; }
   }
}