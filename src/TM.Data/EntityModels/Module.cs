using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace TM.Data
{
   [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
   [SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
   public class Module
   {
      public int Id { get; set; }
      public int CourseId { get; set; }
      public byte Ordinal { get; set; }

      [Required, StringLength(150)]
      public string Title { get; set; }

      public TimeSpan Duration { get; set; }

      [StringLength(2000)]
      public string Description { get; set; }

      public bool IsDeleted { get; set; }

      public virtual Course Course { get; set; }
      public virtual ICollection<Topic> Topics { get; set; }
   }
}
