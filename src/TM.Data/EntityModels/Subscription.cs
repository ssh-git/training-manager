using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using TM.Shared;

namespace TM.Data
{
   [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
   [SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
   public class Subscription
   {
      public string UserId { get; set; }
      public int CourseId { get; set; }

      public LearningState State { get; set; }

      public DateTime? AddDate { get; set; }
      public DateTime? StartDate { get; set; }
      public DateTime? FinishDate { get; set; }

      
      public byte? OwnRating { get; set; }
     
      [StringLength(1000)]
      public string Comment { get; set; }

      public bool IsDeleted { get; set; }

      public byte[] RowVersion { get; set; }

      public virtual Course Course { get; set; }
   }
}
