using System.Collections.Generic;

namespace TM.Shared.Parse
{
   public interface ICategoriesParseResult<TCategoryParseModel>
      where TCategoryParseModel : ICategoryParseModel
   {
      Dictionary<ICategoryUrlNameNaturalKey, TCategoryParseModel> CategoryContainer { get; }
   }
}