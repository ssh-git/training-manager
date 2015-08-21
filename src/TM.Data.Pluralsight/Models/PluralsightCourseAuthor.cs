using System.Collections.Generic;

namespace TM.Data.Pluralsight
{
   public class PluralsightCourseAuthor
   {
      public bool IsAuthorCoAuthor { get; set; }
      public bool HasFullnamesake { get; set; }

      public PluralsightCourse Course { get; set; }
      public PluralsightAuthor Author { get; set; }

      #region Equality Comparers

      private sealed class PropertiesEqualityComparer : IEqualityComparer<PluralsightCourseAuthor>
      {
         public bool Equals(PluralsightCourseAuthor x, PluralsightCourseAuthor y)
         {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;

            return x.IsAuthorCoAuthor == y.IsAuthorCoAuthor &&
                   x.HasFullnamesake == y.HasFullnamesake;
         }

         public int GetHashCode(PluralsightCourseAuthor obj)
         {
            unchecked
            {
               return (obj.IsAuthorCoAuthor.GetHashCode() * 397) ^ obj.HasFullnamesake.GetHashCode();
            }
         }
      }

      private static readonly IEqualityComparer<PluralsightCourseAuthor> PropertiesComparerInstance = new PropertiesEqualityComparer();

      public static IEqualityComparer<PluralsightCourseAuthor> PropertiesComparer
      {
         get { return PropertiesComparerInstance; }
      }

      #endregion
   }
}
