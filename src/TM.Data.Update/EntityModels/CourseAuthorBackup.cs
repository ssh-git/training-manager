using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace TM.Data.Update
{
   [SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
   [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
   public class CourseAuthorBackup
   {
      public int UpdateEventId { get; set; }
      public int CourseId { get; set; }
      public int AuthorId { get; set; }
      public int TrainingProviderId { get; set; }

      public bool? IsAuthorCoAuthor { get; set; }

      public OperationType OperationType { get; set; }

      public virtual TrainingProviderAuthor TrainingProviderAuthor { get; set; }
      public virtual CourseBackup CourseBackup { get; set; }

      #region Equality Comparers

      private sealed class PropertiesEqualityComparer : IEqualityComparer<CourseAuthorBackup>
      {
         public bool Equals(CourseAuthorBackup x, CourseAuthorBackup y)
         {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;

            return x.IsAuthorCoAuthor == y.IsAuthorCoAuthor && x.OperationType == y.OperationType;
         }

         public int GetHashCode(CourseAuthorBackup obj)
         {
            unchecked
            {
               return (obj.IsAuthorCoAuthor.GetHashCode() * 397) ^ (int)obj.OperationType;
            }
         }
      }

      private static readonly IEqualityComparer<CourseAuthorBackup> PropertiesComparerInstance = new PropertiesEqualityComparer();

      public static IEqualityComparer<CourseAuthorBackup> PropertiesComparer
      {
         get { return PropertiesComparerInstance; }
      }

      #endregion
   }
}
