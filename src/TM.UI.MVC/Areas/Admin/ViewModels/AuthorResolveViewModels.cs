using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web.Mvc;
using TM.Data.Update;
using TM.Shared;

namespace TM.UI.MVC.Areas.Admin.ViewModels
{
   [SuppressMessage("ReSharper", "LocalizableElement")]
   [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
   [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
   public class AuthorResolvesViewModels
   {
      public abstract class ViewModelBase
      {
         public int Id { get; set; }

         [Display(Name = "Update Date")]
         [DataType(DataType.Date)]
         [DisplayFormat(DataFormatString = "{0:dd MMM yyyy}")]
         public DateTime UpdateDate { get; set; }

         [Display(Name = "Training Provider")]
         public string TrainingProviderName { get; set; }

         [Display(Name = "Course")]
         public string CourseTitle { get; set; }

         public string CourseUrlName { get; set; }
         public string CategoryUrlName { get; set; }

         [Display(Name = "Problem Type")]
         [EnumDataType(typeof(ProblemType))]
         public ProblemType ProblemType { get; set; }

         [Display(Name = "Resolve State")]
         public ResolveState ResolveState { get; set; }

         public object CourseRouteValuesObject
         {
            get { return new { TrainingProviderName, CategoryUrlName, CourseUrlName }; }
         }
      }

      public class IndexViewModel : ViewModelBase
      {
         public int UpdateEventId { get; set; }
      }


      public class UrlNullDetailsViewModel : ViewModelBase
      {
         [Display(Name = "Author")]
         public string AuthorFullName { get; set; }

         public string AuthorUrlName { get; set; }

         [Display(Name = "Provider Course Url")]
         [DataType(DataType.Url)]
         [UIHint("UrlInNewWindow")]
         public string ProviderCourseUrl { get; set; }

         [Display(Name = "Type")]
         [UIHint("AuthorCoAuthor")]
         public bool IsCoAuthor { get; set; }


         [Display(Name = "Resolved Url")]
         [DataType(DataType.Url)]
         [UIHint("UrlInNewWindow")]
         public string ResolvedUrl { get; set; }

         public object AuthorRouteValuesObject
         {
            get { return new { TrainingProviderName, AuthorUrlName }; }
         }
      }

      public class UrlNullResolveViewModel : UrlNullDetailsViewModel
      {
         public UrlNullResolveViewModel()
         {
            PossibleAuthors = new List<PossibleAuthorViewModel>();
         }

         [Display(Name = "Selected Author ID")]
         public int? SelectedAuthorId { get; set; }

         public PossibleAuthorViewModel PossibleAuthorsMetadata;
         public List<PossibleAuthorViewModel> PossibleAuthors { get; set; }


      }

      public class PossibleAuthorViewModel
      {
         public PossibleAuthorViewModel()
         {
            SiteUrls = new List<string>();
         }

         [Display(Name = "Author ID")]
         public int AuthorId { get; set; }

         [Display(Name = "Full Name")]
         public string FullName { get; set; }

         [Display(Name = "Social Links")]
         public Social SocialLinks { get; set; }

         [Display(Name = "Possible Urls")]
         public List<string> SiteUrls { get; set; }

         public IEnumerable<SelectListItem> SiteUrlsSelectList
         {
            get
            {
               return SiteUrls.Select((x, idx) => new SelectListItem
               {
                  Text = x,
                  Value = x,
                  Selected = idx == 0
               });
            }
         }
      }

      public class ResolvedNullUrlModel
      {
         [Required]
         [DataType(DataType.Url)]
         [RegularExpression(@".+(?<!\/)$")]
         public string ResolvedUrl { get; set; }


         public int? SelectedAuthorId { get; set; }
      }
   }
}