using System.ComponentModel.DataAnnotations;

namespace TM.Data.Update
{
   public enum ProblemType:byte
   {
      [Display(Name = "Url Is Null")]
      AuthorUrlIsNull = 1,

      [Display(Name = "Fullnamesake")]
      AuthorIsFullnamesake
   }
}