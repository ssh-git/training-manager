using System.ComponentModel.DataAnnotations;

namespace TM.Shared
{
   public enum LearningState :byte
   {
      Planned=1,
      [Display(Name = "In progress")]
      InProgress,
      Finished
   }
}