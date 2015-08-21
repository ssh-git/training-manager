using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace TM.Data.Update
{
   [SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
   public class UpdateEvent
   {
      public UpdateEvent()
      {

      }

      [SuppressMessage("ReSharper", "DoNotCallOverridableMethodsInConstructor")]
      public UpdateEvent(int trainingProviderId, string description, DateTime startedOn)
      {
         TrainingProviderId = trainingProviderId;
         Description = description;
         StartedOn = startedOn;
         Added = new Statistic();
         Deleted = new Statistic();
         Modified = new Statistic();
         CategoriesUpdates = new List<CategoryUpdate>();
         CoursesUpdates = new List<CourseUpdate>();
         AuthorsUpdates = new List<AuthorUpdate>();
         UpdateResult = UpdateResult.None;
      }

      public int Id { get; set; }

      [Required]
      public int TrainingProviderId { get; set; }

      [Required]
      [StringLength(200)]
      public string Description { get; set; }
      
      public DateTime StartedOn { get; set; }
      
      public DateTime? EndedOn { get; set; }
      
      public UpdateResult UpdateResult { get; set; }
      
      public string ErrorData { get; set; }

      public Statistic Added { get; set; }
      public Statistic Deleted { get; set; }
      public Statistic Modified { get; set; }

      public virtual TrainingProvider TrainingProvider { get; set; }
      public virtual ICollection<CategoryUpdate> CategoriesUpdates { get; set; }
      public virtual ICollection<CourseUpdate> CoursesUpdates { get; set; }
      public virtual ICollection<AuthorUpdate> AuthorsUpdates { get; set; }
   }
}
