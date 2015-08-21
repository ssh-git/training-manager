using System.ComponentModel.DataAnnotations;

namespace TM.UI.MVC.Models
{
   public class ChangePasswordViewModel
   {
      [Required(AllowEmptyStrings = false, ErrorMessage = "{0} is required and cannot be withespace")]
      [DataType(DataType.Password)]
      [Display(Name = "Current password")]
      public string OldPassword { get; set; }

      [Required(AllowEmptyStrings = false)]
      [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
      [DataType(DataType.Password)]
      [Display(Name = "New password")]
      public string NewPassword { get; set; }

      [DataType(DataType.Password)]
      [Display(Name = "Confirm new password")]
      [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
      public string ConfirmPassword { get; set; }
   }
}