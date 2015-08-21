using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace TM.Data
{
   [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
   [SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
   public class CourseAuthor
   {
      public int CourseId { get; set; }
      public int AuthorId { get; set; }

      public int TrainingProviderId { get; set; }

      public bool IsAuthorCoAuthor { get; set; }

      public bool IsDeleted { get; set; }

      public virtual Course Course { get; set; }
      public virtual TrainingProviderAuthor TrainingProviderAuthor { get; set; }

      #region Equality Comparers

      private sealed class IdentityEqualityComparer : IEqualityComparer<CourseAuthor>
      {
         public bool Equals(CourseAuthor x, CourseAuthor y)
         {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;

            return x.CourseId == y.CourseId &&
                   x.AuthorId == y.AuthorId &&
                   x.TrainingProviderId == y.TrainingProviderId;

         }

         public int GetHashCode(CourseAuthor obj)
         {
            unchecked
            {
               var hashCode = obj.CourseId;
               hashCode = (hashCode*397) ^ obj.AuthorId;
               hashCode = (hashCode*397) ^ obj.TrainingProviderId;

               return hashCode;
            }
         }
      }

      private static readonly IEqualityComparer<CourseAuthor> IdentityComparerInstance = new IdentityEqualityComparer();

      public static IEqualityComparer<CourseAuthor> IdentityComparer
      {
         get { return IdentityComparerInstance; }
      }


      private sealed class PropertiesEqualityComparer : IEqualityComparer<CourseAuthor>
      {
         public bool Equals(CourseAuthor x, CourseAuthor y)
         {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;

            return x.IsAuthorCoAuthor == y.IsAuthorCoAuthor &&
                   x.IsDeleted == y.IsDeleted;
         }

         public int GetHashCode(CourseAuthor obj)
         {
            unchecked
            {
               return (obj.IsAuthorCoAuthor.GetHashCode()*397) ^ obj.IsDeleted.GetHashCode();
            }
         }
      }

      private static readonly IEqualityComparer<CourseAuthor> PropertiesComparerInstance = new PropertiesEqualityComparer();

      public static IEqualityComparer<CourseAuthor> PropertiesComparer
      {
         get { return PropertiesComparerInstance; }
      }

      #endregion
   }
}
