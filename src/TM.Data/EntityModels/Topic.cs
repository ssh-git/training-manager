using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace TM.Data
{
   [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
   [SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
   public class Topic
   {
      public int Id { get; set; }

      public int ModuleId { get; set; }

      public byte Ordinal { get; set; }

      [Required, StringLength(150)]
      public string Title { get; set; }

      public TimeSpan Duration { get; set; }

      public bool IsDeleted { get; set; }

      public virtual Module Module { get; set; }
   }
}
