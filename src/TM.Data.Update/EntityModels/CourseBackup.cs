using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TM.Shared;

namespace TM.Data.Update
{
   public class CourseBackup
   {
      public int UpdateEventId { get; set; }
      public int CourseId { get; set; }

      public int? CategoryId { get; set; }

      [StringLength(100)]
      public string Title { get; set; }

      [StringLength((int)StringLengthConstraint.Url)]
      public string SiteUrl { get; set; }

      [StringLength(3500)]
      public string Description { get; set; }
      
      public bool? HasClosedCaptions { get; set; }

      public byte? CourseLevel { get; set; }
      
      public TimeSpan? Duration { get; set; }

      public DateTime? ReleaseDate { get; set; }


      public virtual Category Category { get; set; }
      public virtual ICollection<CourseAuthorBackup> CourseAuthorBackups { get; set; } 
      public virtual CourseUpdate CourseUpdate { get; set; }
   }
}