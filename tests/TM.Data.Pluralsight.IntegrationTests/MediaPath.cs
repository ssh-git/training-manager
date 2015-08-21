using System.Collections.Generic;
using TM.Shared;

namespace TM.Data.Pluralsight.IntegrationTests
{
   public class MediaPath : IMediaPath
   {
      public IDictionary<int, string> CategoriesLogoPath
      {
         get
         {
            var paths = new Dictionary<int, string>();
            paths.Add(1,
               @".\..\..\Content\images\category\");
            return paths;
         }

      }

      public IDictionary<int, string> AuthorsLogoPath
      {
         get
         {
            var paths = new Dictionary<int, string>();
            paths.Add(1,
               @".\..\..\Content\images\authors\");
            return paths;
         }
      }

      public IDictionary<int, string> BadgesPath
      {
         get
         {
            var paths = new Dictionary<int, string>();
            paths.Add(1,
               @".\..\..\Content\images\badges\");
            return paths;
         }
      }
   }
}