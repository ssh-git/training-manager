using System.ComponentModel.DataAnnotations;
using TM.Shared;

namespace TM.UI.MVC.Models
{
   public class LoginViewModel
   {
      [Required]
      [StringLength(50, ErrorMessage = "The {0} must be less than {1} characters long.")]
      public string Nickname { get; set; }

      [Required]
      [DataType(DataType.Password)]
      [Display(Name = "Password")]
      public string Password { get; set; }

      [Display(Name = "Remember me?")]
      public bool RememberMe { get; set; }
   }

   public class DefaultAdminViewModel
   {
      [Required]
      [Display(Name = "Nickname")]
      [StringLength(50, ErrorMessage = "The {0} must be less than {1} characters long.")]
      public string UserName { get; set; }

      [Required]
      [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
      [DataType(DataType.Password)]
      public string Password { get; set; }

      [DataType(DataType.Password)]
      [Display(Name = "Confirm password")]
      [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
      public string ConfirmPassword { get; set; }
   }


   public class RegisterViewModel
   {
      public RegisterViewModel()
      {
         SpecializationsList = new SpecializationsListViewModel(Specializations.AllSpecializations);
      }

      [Required]
      public string Nickname { get; set; }

      [Required]
      [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
      [DataType(DataType.Password)]
      public string Password { get; set; }

      [DataType(DataType.Password)]
      [Display(Name = "Confirm password")]
      [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
      public string ConfirmPassword { get; set; }

      public SpecializationsListViewModel SpecializationsList{get; set; }
      
   }
}
