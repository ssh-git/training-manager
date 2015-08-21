using System.Collections.Generic;
using System.Diagnostics;
using TM.Shared.Parse;

namespace TM.Data.Pluralsight
{
   [DebuggerDisplay("Full Name = {FullName}; Url = {SiteUrl}, Name = {UrlName}")]
   public class PluralsightAuthor : IAuthorParseModel
   {
      public int Id { get; set; }

      public string FullName { get; set; }

      public string SiteUrl { get; set; }

      public string UrlName { get; set; }

      #region Equality Comparers

      private sealed class PropertiesEqualityComparer : IEqualityComparer<PluralsightAuthor>
      {
         public bool Equals(PluralsightAuthor x, PluralsightAuthor y)
         {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;

            return string.Equals(x.FullName, y.FullName) &&
                   string.Equals(x.SiteUrl, y.SiteUrl) &&
                   string.Equals(x.UrlName, y.UrlName);
         }

         public int GetHashCode(PluralsightAuthor obj)
         {
            unchecked
            {
               var hashCode = (obj.FullName != null ? obj.FullName.GetHashCode() : 0);
               hashCode = (hashCode * 397) ^ (obj.SiteUrl != null ? obj.SiteUrl.GetHashCode() : 0);
               hashCode = (hashCode * 397) ^ (obj.UrlName != null ? obj.UrlName.GetHashCode() : 0);
               return hashCode;
            }
         }
      }

      private static readonly IEqualityComparer<PluralsightAuthor> PropertiesComparerInstance = new PropertiesEqualityComparer();

      public static IEqualityComparer<PluralsightAuthor> PropertiesComparer
      {
         get { return PropertiesComparerInstance; }
      }

      #endregion
   }
}
