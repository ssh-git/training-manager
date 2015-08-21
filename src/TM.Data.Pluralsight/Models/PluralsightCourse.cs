using System;
using System.Collections.Generic;
using TM.Shared;
using TM.Shared.Parse;

namespace TM.Data.Pluralsight
{
   public class PluralsightCourse : ICourseParseModel
   {
      public int Id { get; set; }

      public string Title { get; set; }
      public string SiteUrl { get; set; }
      public string UrlName { get; set; }

      public string Description { get; set; }

      public bool HasClosedCaptions { get; set; }
      public CourseLevel Level { get; set; }
      public CourseRating Rating { get; set; }
      public TimeSpan Duration { get; set; }
      public DateTime ReleaseDate { get; set; }

      public List<PluralsightCourseAuthor> CourseAuthors { get; set; }
      public PluralsightCategory Category { get; set; }

      #region Equality Comparers

      private sealed class PropertiesEqualityComparer : IEqualityComparer<PluralsightCourse>
      {
         public bool Equals(PluralsightCourse x, PluralsightCourse y)
         {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;

            return string.Equals(x.Title, y.Title) &&
                   string.Equals(x.SiteUrl, y.SiteUrl) &&
                   string.Equals(x.UrlName, y.UrlName) &&
                   string.Equals(x.Description, y.Description) &&
                   x.HasClosedCaptions == y.HasClosedCaptions &&
                   x.Level == y.Level && Equals(x.Rating, y.Rating) &&
                   x.Duration.Equals(y.Duration) &&
                   x.ReleaseDate.Equals(y.ReleaseDate);
         }

         public int GetHashCode(PluralsightCourse obj)
         {
            unchecked
            {
               var hashCode = (obj.Title != null ? obj.Title.GetHashCode() : 0);
               hashCode = (hashCode * 397) ^ (obj.SiteUrl != null ? obj.SiteUrl.GetHashCode() : 0);
               hashCode = (hashCode * 397) ^ (obj.UrlName != null ? obj.UrlName.GetHashCode() : 0);
               hashCode = (hashCode * 397) ^ (obj.Description != null ? obj.Description.GetHashCode() : 0);
               hashCode = (hashCode * 397) ^ obj.HasClosedCaptions.GetHashCode();
               hashCode = (hashCode * 397) ^ (int)obj.Level;
               hashCode = (hashCode * 397) ^ (obj.Rating != null ? obj.Rating.GetHashCode() : 0);
               hashCode = (hashCode * 397) ^ obj.Duration.GetHashCode();
               hashCode = (hashCode * 397) ^ obj.ReleaseDate.GetHashCode();
               return hashCode;
            }
         }
      }

      private static readonly IEqualityComparer<PluralsightCourse> PropertiesComparerInstance = new PropertiesEqualityComparer();

      public static IEqualityComparer<PluralsightCourse> PropertiesComparer
      {
         get { return PropertiesComparerInstance; }
      }

      #endregion
   }
}