using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using TM.Shared;

namespace TM.Data
{
   [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
   [SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
   public class TrainingProviderAuthor : IAuthorUrlNameNaturalKey
   {
      public int TrainingProviderId { get; set; }
      public int AuthorId { get; set; }

      [Required, StringLength(250)]
      public string FullName { get; set; }

      [Required, StringLength((int)StringLengthConstraint.Url)]
      public string SiteUrl { get; set; }

      [Required, StringLength((int)StringLengthConstraint.UrlName)]
      public string UrlName { get; set; }

      public bool IsDeleted { get; set; }

      public virtual TrainingProvider TrainingProvider { get; set; }
      public virtual Author Author { get; set; }
      public virtual ICollection<CourseAuthor> AuthorCourses { get; set; }

      #region Equality Comparers

      private sealed class PropertiesEqualityComparer : IEqualityComparer<TrainingProviderAuthor>
      {
         public bool Equals(TrainingProviderAuthor x, TrainingProviderAuthor y)
         {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;

            return string.Equals(x.FullName, y.FullName) &&
                   string.Equals(x.SiteUrl, y.SiteUrl) &&
                   string.Equals(x.UrlName, y.UrlName) &&
                   x.IsDeleted == y.IsDeleted;
         }

         public int GetHashCode(TrainingProviderAuthor obj)
         {
            unchecked
            {
               var hashCode = (obj.FullName != null ? obj.FullName.GetHashCode() : 0);
               hashCode = (hashCode*397) ^ (obj.SiteUrl != null ? obj.SiteUrl.GetHashCode() : 0);
               hashCode = (hashCode*397) ^ (obj.UrlName != null ? obj.UrlName.GetHashCode() : 0);
               hashCode = (hashCode*397) ^ obj.IsDeleted.GetHashCode();
               return hashCode;
            }
         }
      }

      private static readonly IEqualityComparer<TrainingProviderAuthor> PropertiesComparerInstance = new PropertiesEqualityComparer();

      public static IEqualityComparer<TrainingProviderAuthor> PropertiesComparer
      {
         get { return PropertiesComparerInstance; }
      }

      #endregion
   }
}
