using System.Collections.Generic;

namespace TM.Shared
{
   public interface IMediaPath
   {
      IDictionary<int, string> CategoriesLogoPath { get; }
      IDictionary<int, string> AuthorsLogoPath { get; }
      IDictionary<int, string> BadgesPath { get; }
   }
}