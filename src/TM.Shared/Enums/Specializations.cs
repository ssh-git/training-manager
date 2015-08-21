using System;
using System.ComponentModel.DataAnnotations;

namespace TM.Shared
{
   [Flags]
   public enum Specializations:byte
   {
      None = 0,

      [Display(Name = "Software Developer")]
      SoftwareDeveloper = 1 << 0,

      [Display(Name = "IT Administrator")]
      ITAdministrator = 1 << 1,

      [Display(Name = "Creative Professional")]
      CreativeProfessional = 1 << 2,

      AllSpecializations = SoftwareDeveloper | ITAdministrator | CreativeProfessional
   }
}
