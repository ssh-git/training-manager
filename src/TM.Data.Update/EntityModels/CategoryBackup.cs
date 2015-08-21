using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using TM.Shared;

namespace TM.Data.Update
{
   [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
   [SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
   public class CategoryBackup
   {
      public int UpdateEventId { get; set; }
      public int CategoryId { get; set; }

      [StringLength(100)]
      public string Title { get; set; }

      [StringLength((int)StringLengthConstraint.Url)]
      public string LogoUrl { get; set; }

      [StringLength((int)StringLengthConstraint.UrlName)]
      public string LogoFileName { get; set; }

      public virtual CategoryUpdate CategoryUpdate { get; set; }
   }
}