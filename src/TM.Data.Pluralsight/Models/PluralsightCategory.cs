using System.Collections.Generic;
using System.Diagnostics;
using TM.Shared.Parse;

namespace TM.Data.Pluralsight
{
   [DebuggerDisplay("UrlName = {UrlName}; Title = {Title}; LogoUrl= {LogoUrl}")]
   public class PluralsightCategory : ICategoryParseModel
   {
      public int Id { get; set; }

      public string Title { get; set; }
      public string UrlName { get; set; }
      public string LogoUrl { get; set; }
      public string LogoFileName { get; set; }

      public List<PluralsightCourse> Courses { get; set; }


      #region Equality Comparers

      private sealed class PropertiesEqualityComparer : IEqualityComparer<PluralsightCategory>
      {
         public bool Equals(PluralsightCategory x, PluralsightCategory y)
         {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;

            return string.Equals(x.Title, y.Title) &&
                   string.Equals(x.UrlName, y.UrlName) &&
                   string.Equals(x.LogoUrl, y.LogoUrl) &&
                   string.Equals(x.LogoFileName, y.LogoFileName);
         }

         public int GetHashCode(PluralsightCategory obj)
         {
            unchecked
            {
               var hashCode = (obj.Title != null ? obj.Title.GetHashCode() : 0);
               hashCode = (hashCode * 397) ^ (obj.UrlName != null ? obj.UrlName.GetHashCode() : 0);
               hashCode = (hashCode * 397) ^ (obj.LogoUrl != null ? obj.LogoUrl.GetHashCode() : 0);
               hashCode = (hashCode * 397) ^ (obj.LogoFileName != null ? obj.LogoFileName.GetHashCode() : 0);
               return hashCode;
            }
         }
      }

      private static readonly IEqualityComparer<PluralsightCategory> PropertiesComparerInstance = new PropertiesEqualityComparer();

      public static IEqualityComparer<PluralsightCategory> PropertiesComparer
      {
         get { return PropertiesComparerInstance; }
      }

      #endregion
   }
}

