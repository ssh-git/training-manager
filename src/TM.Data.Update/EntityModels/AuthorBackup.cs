using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using TM.Shared;

namespace TM.Data.Update
{
   [SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
   [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
   public class AuthorBackup
   {
      public int UpdateEventId { get; set; }
      public int TrainingProviderId { get; set; }
      public int AuthorId { get; set; }

      [StringLength(250)]
      public string FullName { get; set; }

      [StringLength((int)StringLengthConstraint.Url)]
      public string SiteUrl { get; set; }

      public virtual AuthorUpdate AuthorUpdate { get; set; }
   }
}