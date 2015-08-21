using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace TM.UI.MVC.Areas.Admin.ViewModels
{
   [SuppressMessage("ReSharper", "LocalizableElement")]
   public class ApplicationRoleViewModel
   {
      public string Id { get; set; }

      [Required(AllowEmptyStrings = false, ErrorMessage = "You must enter a {0} for the Role.")]
      [StringLength(256, ErrorMessage = "The {0} must be {1} characters or shorter. ")]
      [Display(Name = "Role Name")]
      public string Name { get; set; }
   }
}