using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using TM.Shared;

namespace TM.Data
{
   [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
   [SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
   public class Category : ICategoryUrlNameNaturalKey
   {
      public int Id { get; set; }

      public int TrainingProviderId { get; set; }

      [Required, StringLength(100)]
      public string Title { get; set; }

      [Required, StringLength((int)StringLengthConstraint.UrlName)]
      public string UrlName { get; set; }

      [StringLength((int)StringLengthConstraint.Url)]
      public string LogoUrl { get; set; }

      [StringLength((int)StringLengthConstraint.UrlName)]
      public string LogoFileName { get; set; }

      public bool IsDeleted { get; set; }


      public virtual TrainingProvider TrainingProvider { get; set; }
      public virtual ICollection<Course> Courses { get; set; }


      #region Equaility Comparers

      private sealed class PropertiesEqualityComparer : IEqualityComparer<Category>
      {
         public bool Equals(Category x, Category y)
         {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;

            return string.Equals(x.Title, y.Title) &&
                   string.Equals(x.UrlName, y.UrlName) &&
                   string.Equals(x.LogoUrl, y.LogoUrl) &&
                   string.Equals(x.LogoFileName, y.LogoFileName) &&
                   x.IsDeleted == y.IsDeleted;
         }

         public int GetHashCode(Category obj)
         {
            unchecked
            {
               var hashCode = (obj.Title != null ? obj.Title.GetHashCode() : 0);
               hashCode = (hashCode*397) ^ (obj.UrlName != null ? obj.UrlName.GetHashCode() : 0);
               hashCode = (hashCode*397) ^ (obj.LogoUrl != null ? obj.LogoUrl.GetHashCode() : 0);
               hashCode = (hashCode*397) ^ (obj.LogoFileName != null ? obj.LogoFileName.GetHashCode() : 0);
               hashCode = (hashCode*397) ^ obj.IsDeleted.GetHashCode();
               return hashCode;
            }
         }
      }

      private static readonly IEqualityComparer<Category> PropertiesComparerInstance = new PropertiesEqualityComparer();

      public static IEqualityComparer<Category> PropertiesComparer
      {
         get { return PropertiesComparerInstance; }
      }

      #endregion
   }
}
