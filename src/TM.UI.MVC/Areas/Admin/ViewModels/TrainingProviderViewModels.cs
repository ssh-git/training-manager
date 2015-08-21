using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TM.Shared;

namespace TM.UI.MVC.Areas.Admin.ViewModels
{
   [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
   [SuppressMessage("ReSharper", "LocalizableElement")]
   public class TrainingProviderViewModels
   {
      public class IndexViewModel
      {
         [Required]
         [Display(Name = "Id")]
         public int Id { get; set; }

         [Required]
         [Display(Name = "Title")]
         public string Name { get; set; }

         [Required]
         [Display(Name = "Url")]
         [DataType(DataType.Url)]
         [UIHint("UrlInNewWindow")]
         public string SiteUrl { get; set; }

         [Display(Name = "Current Logo")]
         public LogoViewModel CurrentLogo { get; set; }

         [Display(Name = "Hours to update")]
         public string AllowedUpdateUtcHoursString { get; set; }


         [Required]
         [Display(Name = "Update Frequency (h)")]
         public short UpdateFrequencyHours { get; set; }


         [Required]
         [Display(Name = "Hidden")]
         [UIHint("YesNo")]
         public bool IsDeleted { get; set; }

      }

      public class DetailsViewModel : IndexViewModel
      {
         [Required]
         [Display(Name = "Description")]
         [DataType(DataType.MultilineText)]
         public string Description { get; set; }

         [Required]
         [Display(Name = "Source Url")]
         [DataType(DataType.Url)]
         [UIHint("UrlInNewWindow")]
         public string SourceUrl { get; set; }

         [Display(Name = "Source Location")]
         [EnumDataType(typeof(LocationType))]
         public LocationType SourceLocation { get; set; }

         [Required]
         [Display(Name = "Type Name")]
         public string AssemblyType { get; set; }

         public IEnumerable<SelectListItem> IsDeletedSelectList
         {
            get
            {
               yield return new SelectListItem { Text = "Yes", Value = bool.TrueString, Selected = IsDeleted };
               yield return new SelectListItem { Text = "No", Value = bool.FalseString, Selected = IsDeleted };
            }
         }
      }

      public class EditViewModel : DetailsViewModel
      {
         private IEnumerable<SelectListItem> _logosSelectList;
         private IEnumerable<SelectListItem> _updateHoursSelectList;

         [Required]
         [Display(Name = "Hours To Update")]
         public List<int> SelectedUpdateHours { get; set; }


         public IEnumerable<SelectListItem> LogosSelectList
         {
            get
            {
               if (_logosSelectList != null)
               {
                  return _logosSelectList;
               }

               var mediaDirectoryPath = HttpContext.Current.Server.MapPath(AppConstants.VirtualPaths.TrainingProvidersContent);


               _logosSelectList = Directory.EnumerateFiles(mediaDirectoryPath, Name + "*", SearchOption.TopDirectoryOnly)
                  .Select(Path.GetFileName)
                  .Select(x => new SelectListItem
                  {
                     Text = x,
                     Value = VirtualPathUtility.Combine(AppConstants.VirtualPaths.TrainingProvidersContent, x),
                     Selected = x == CurrentLogo.FileName
                  });

               return _logosSelectList;
            }
         }

         public IEnumerable<SelectListItem> UpdateHoursSelectList
         {
            get
            {
               if (_updateHoursSelectList != null)
               {
                  return _updateHoursSelectList;
               }

               var selectedUpdateHours = AllowedUpdateUtcHoursString.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

               _updateHoursSelectList = Enumerable.Range(0, 24)
                  .Select(x => x.ToString())
                  .Select(x => new SelectListItem
                  {
                     Text = x,
                     Value = x,
                     Selected = selectedUpdateHours.Contains(x)
                  });

               return _updateHoursSelectList;
            }
         }

         [Required]
         [Display(Name = "Current Logo")]
         public SelectedLogoModel SelectedLogo { get; set; }

         [Display(Name = "Upload New Logo")]
         [DataType(DataType.Upload)]
         public HttpPostedFileBase LogoUpload { get; set; }
      }

      public class CreateViewModel : DetailsViewModel
      {

         private IEnumerable<SelectListItem> _updateHoursSelectList;

         [Required]
         [Display(Name = "Hours To Update")]
         public List<int> SelectedUpdateHours { get; set; }

         public IEnumerable<SelectListItem> UpdateHoursSelectList
         {
            get
            {
               if (_updateHoursSelectList != null)
               {
                  return _updateHoursSelectList;
               }

               _updateHoursSelectList = Enumerable.Range(0, 24)
                  .Select(x => x.ToString())
                  .Select(x => new SelectListItem
                  {
                     Text = x,
                     Value = x
                  });

               return _updateHoursSelectList;
            }
         }
         

         [Display(Name = "Select Logo File")]
         [DataType(DataType.Upload)]
         public HttpPostedFileBase LogoUpload { get; set; }

      }

      public class DeleteViewModel
      {
         public int Id { get; set; }
         public string Name { get; set; }
      }

      [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
      public class SelectedLogoModel
      {
         [Required]
         public string LocalUrl { get; set; }

         public string FileName
         {
            get { return VirtualPathUtility.GetFileName(LocalUrl); }
         }
      }

      
      public class LogoViewModel
      {
         public string FileName { get; set; }


         public string LocalUrl
         {
            get
            {
               return VirtualPathUtility.Combine(AppConstants.VirtualPaths.TrainingProvidersContent, FileName).Substring(1);
            }
         }
      }
   }

}