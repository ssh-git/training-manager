using System.ComponentModel.DataAnnotations;
using System.Web;

namespace TM.UI.MVC.Areas.Admin.ViewModels
{
   public class CategoryLogoFileUploadModel
   {
      [Required]
      public int? CategoryId { get; set; }
      [StringLength(70)]
      public string CategoryName { get; set; }

      [Required]
      public int? TrainingProviderId { get; set; }

      [StringLength(70)]
      public string TrainingProviderName { get; set; }

      [Required]
      public HttpPostedFileBase File { get; set; }
   }
}