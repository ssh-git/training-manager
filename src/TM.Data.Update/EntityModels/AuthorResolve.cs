using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace TM.Data.Update
{
   [SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
   [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
   public class AuthorResolve
   {
      public int Id { get; set; }
      public int UpdateEventId { get; set; }
      public int CourseId { get; set; }
      public int TrainingProviderId { get; set; }

      [Required]
      [StringLength(400)]
      public string AuthorFullName { get; set; }

      [StringLength(400)]
      public string AuthorSiteUrl { get; set; }

      [StringLength(400)]
      public string AuthorUrlName { get; set; }
      public bool IsAuthorCoAuthor { get; set; }

      public int? ResolvedAuthorId { get; set; }

      public ResolveState ResolveState { get; set; }
      public ProblemType ProblemType { get; set; }

      public virtual UpdateEvent UpdateEvent { get; set; }
      public virtual Course Course { get; set; }
      public virtual TrainingProvider TrainingProvider { get; set; }

      public virtual Author ResolvedAuthor { get; set; }

      #region Equality Comparers

      private sealed class PropertiesEqualityComparer : IEqualityComparer<AuthorResolve>
      {
         public bool Equals(AuthorResolve x, AuthorResolve y)
         {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;

            return string.Equals(x.AuthorFullName, y.AuthorFullName) &&
                   string.Equals(x.AuthorSiteUrl, y.AuthorSiteUrl) &&
                   string.Equals(x.AuthorUrlName, y.AuthorUrlName) &&
                   x.IsAuthorCoAuthor == y.IsAuthorCoAuthor &&
                   x.ResolveState == y.ResolveState &&
                   x.ProblemType == y.ProblemType;
         }

         public int GetHashCode(AuthorResolve obj)
         {
            unchecked
            {
               var hashCode = (obj.AuthorFullName != null ? obj.AuthorFullName.GetHashCode() : 0);
               hashCode = (hashCode * 397) ^ (obj.AuthorSiteUrl != null ? obj.AuthorSiteUrl.GetHashCode() : 0);
               hashCode = (hashCode * 397) ^ (obj.AuthorUrlName != null ? obj.AuthorUrlName.GetHashCode() : 0);
               hashCode = (hashCode * 397) ^ obj.IsAuthorCoAuthor.GetHashCode();
               hashCode = (hashCode * 397) ^ (int)obj.ResolveState;
               hashCode = (hashCode * 397) ^ (int)obj.ProblemType;
               return hashCode;
            }
         }
      }

      private static readonly IEqualityComparer<AuthorResolve> PropertiesComparerInstance = new PropertiesEqualityComparer();

      public static IEqualityComparer<AuthorResolve> PropertiesComparer
      {
         get { return PropertiesComparerInstance; }
      }

      #endregion
   }
}
