using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.SqlServer;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using TM.Data.Update;
using TM.Shared;
using TM.UI.MVC.ControllableModel;

namespace TM.UI.MVC.Areas.Admin.ViewModels
{
   public class UpdateEventViewModels
   {
      #region Index View Models

      public class IndexViewModel
      {
         public int Id { get; set; }

         [Display(Name = "Provider")]
         public string TrainingProviderName { get; set; }

         [Display(Name = "Description")]
         public string Description { get; set; }

         [Display(Name = "Started")]
         [DataType(DataType.DateTime)]
         [DisplayFormat(DataFormatString = "{0:dd MMM yyyy HH:mm:ss}")]
         public DateTime StartedOn { get; set; }

         [Display(Name = "Ended")]
         [DataType(DataType.DateTime)]
         [DisplayFormat(DataFormatString = "{0:dd MMM yyyy HH:mm:ss}")]
         public DateTime? EndedOn { get; set; }

         [Display(Name = "Duration")]
         [DataType(DataType.Duration)]
         public TimeSpan Duration
         {
            get
            {
               if (EndedOn.HasValue)
               {
                  return EndedOn.Value - StartedOn;
               }

               return TimeSpan.Zero;
            }
         }

         [Display(Name = "Update Result")]
         public UpdateResult UpdateResult { get; set; }


         public static async Task<IndexControllableViewModel> ToControlableViewModelAsync(
            IQueryable<IndexViewModel> query, ControllableViewModelParams controlParams)
         {
            var controllableViewModel = new IndexControllableViewModel(controlParams);

            await controllableViewModel.InitializeAsync(query);

            return controllableViewModel;
         }
      }

      public class UpdateEventSortModel
      {
         public enum State
         {
            None = 0,

            StartDate,
            StartDateDesc,

            TrainingProvider,
            TrainingProviderDesc,

            EndDate,
            EndDateDesc,

            Duration,
            DurationDesc
         }

         public static UpdateEventSortModel Create(string sortKey)
         {
            State sortState;
            Enum.TryParse(sortKey, true, out sortState);

            return new UpdateEventSortModel
            {
               StartDateSortState = sortState == State.None || sortState == State.StartDateDesc
                  ? State.StartDate
                  : State.StartDateDesc,

               TrainingProviderSortState = sortState == State.TrainingProvider
                  ? State.TrainingProviderDesc
                  : State.TrainingProvider,

               EndDateDateSortState = sortState == State.EndDate
                  ? State.EndDateDesc
                  : State.EndDate,

               DurationSortState = sortState == State.Duration
                  ? State.DurationDesc
                  : State.Duration
            };
         }

         public State TrainingProviderSortState { get; private set; }
         public State StartDateSortState { get; private set; }
         public State EndDateDateSortState { get; private set; }
         public State DurationSortState { get; private set; }
      }


      public class IndexControllableViewModel : ControllableViewModel<IndexViewModel, UpdateEventSortModel>
      {
         public IndexControllableViewModel(IControllableViewModelParams viewModelParams, string defaultPageSize = "10")
            : base(viewModelParams, defaultPageSize)
         {
            SearchBoxTextPlaceholder = "Search by Training Provider";
         }

         public IndexControllableViewModel(IControllableViewModelParams viewModelParams, string defaultPageSize,
            params string[] sizeList)
            : base(viewModelParams, defaultPageSize, sizeList)
         {
            SearchBoxTextPlaceholder = "Search by Training Provider";
         }

         protected override IQueryable<IndexViewModel> ApplyFiltering(IQueryable<IndexViewModel> chainQuery,
            IFilterParam filterParam)
         {
            UpdateResult filterKey;
            Enum.TryParse(filterParam.FilterKey, true, out filterKey);

            switch (filterKey)
            {
               case UpdateResult.None:
                  break;
               case UpdateResult.Error:
                  chainQuery = chainQuery.Where(x => x.UpdateResult == UpdateResult.Error);
                  break;
               case UpdateResult.Success:
                  chainQuery = chainQuery.Where(x => x.UpdateResult == UpdateResult.Success);
                  break;
               case UpdateResult.NeedManualResolve:
                  chainQuery = chainQuery.Where(x => x.UpdateResult == UpdateResult.NeedManualResolve);
                  break;
               case UpdateResult.Resolved:
                  chainQuery = chainQuery.Where(x => x.UpdateResult == UpdateResult.Resolved);
                  break;
               default:
                  throw new ArgumentOutOfRangeException();
            }

            return chainQuery;
         }

         protected override IQueryable<IndexViewModel> ApplySearching(IQueryable<IndexViewModel> chainQuery,
            ISearchParam searchParam)
         {
            if (!string.IsNullOrWhiteSpace(searchParam.SearchKey))
            {
               chainQuery = chainQuery.Where(x => x.TrainingProviderName.Contains(searchParam.SearchKey));
            }

            return chainQuery;
         }

         protected override IQueryable<IndexViewModel> ApplySorting(IQueryable<IndexViewModel> chainQuery,
            ISortParam sortParam)
         {
            UpdateEventSortModel.State sortState;
            Enum.TryParse(sortParam.SortKey, true, out sortState);

            switch (sortState)
            {
               case UpdateEventSortModel.State.TrainingProvider:
                  chainQuery = chainQuery.OrderBy(x => x.TrainingProviderName).ThenByDescending(x => x.Id);
                  break;
               case UpdateEventSortModel.State.TrainingProviderDesc:
                  chainQuery = chainQuery.OrderByDescending(x => x.TrainingProviderName).ThenByDescending(x => x.Id);
                  break;
               case UpdateEventSortModel.State.StartDate:
                  chainQuery = chainQuery.OrderBy(x => x.StartedOn);
                  break;
               case UpdateEventSortModel.State.EndDate:
                  chainQuery = chainQuery.OrderBy(x => x.EndedOn);
                  break;
               case UpdateEventSortModel.State.EndDateDesc:
                  chainQuery = chainQuery.OrderByDescending(x => x.EndedOn);
                  break;
               case UpdateEventSortModel.State.Duration:
                  chainQuery = chainQuery.OrderBy(x => SqlFunctions.DateDiff("ss", x.StartedOn, x.EndedOn));
                  break;
               case UpdateEventSortModel.State.DurationDesc:
                  chainQuery = chainQuery.OrderByDescending(x => SqlFunctions.DateDiff("ss", x.StartedOn, x.EndedOn));
                  break;
               default:
                  chainQuery = chainQuery.OrderByDescending(x => x.StartedOn);
                  break;
            }

            return chainQuery;
         }

         protected override UpdateEventSortModel CreateSortStateModel(IControllableViewModelParams controlParams)
         {
            return UpdateEventSortModel.Create(controlParams.SortKey);
         }

         protected override Task InitializeFilterSelectList(IQueryable<IndexViewModel> chainQuery,
            IFilterParam filterParam)
         {
            UpdateResult filterKey;
            Enum.TryParse(filterParam.FilterKey, true, out filterKey);

            FilterSelectList = new[]
            {
               new SelectListItem
               {
                  Value = UpdateResult.None.ToString(),
                  Text = "Filter not applyed",
                  Selected = filterKey == UpdateResult.None
               },
               new SelectListItem
               {
                  Value = UpdateResult.Success.ToString(),
                  Text = "Display Succeeded",
                  Selected = filterKey == UpdateResult.Success
               },
               new SelectListItem
               {
                  Value = UpdateResult.Error.ToString(),
                  Text = "Display Failed",
                  Selected = filterKey == UpdateResult.Error
               },
               new SelectListItem
               {
                  Value = UpdateResult.NeedManualResolve.ToString(),
                  Text = "Display Not Resolved",
                  Selected = filterKey == UpdateResult.NeedManualResolve
               },
               new SelectListItem
               {
                  Value = UpdateResult.Resolved.ToString(),
                  Text = "Display Resolved",
                  Selected = filterKey == UpdateResult.Resolved
               }
            };

            return Task.FromResult(true);
         }
      }

      #endregion




      public class DetailsViewModel
      {
         public int Id { get; set; }

         [Display(Name = "Provider")]
         public string TrainingProviderName { get; set; }

         [Display(Name = "Description")]
         public string Description { get; set; }

         [Display(Name = "Started")]
         [DisplayFormat(DataFormatString = "{0:dd MMM yyyy HH:mm:ss}")]
         public DateTime StartedOn { get; set; }

         [Display(Name = "Ended")]
         [DisplayFormat(DataFormatString = "{0:dd MMM yyyy HH:mm:ss}")]
         public DateTime? EndedOn { get; set; }

         [Display(Name = "Duration")]
         [DataType(DataType.Duration)]
         public TimeSpan Duration
         {
            get
            {
               if (EndedOn.HasValue)
               {
                  return EndedOn.Value - StartedOn;
               }

               return TimeSpan.Zero;
            }
         }

         [Display(Name = "Update Result")]
         public UpdateResult UpdateResult { get; set; }

         [Display(Name = "Error Message")]
         [DisplayFormat(NullDisplayText = "None")]
         [DataType(DataType.MultilineText)]
         public string ErrorData { get; set; }

         [Display(Name = "Added")]
         public StatisticViewModel Added { get; set; }

         [Display(Name = "Deleted")]
         public StatisticViewModel Deleted { get; set; }

         [Display(Name = "Modified")]
         public StatisticViewModel Modified { get; set; }

      }


      public class StatisticViewModel
      {
         [Display(Name = "Categories")]
         public int Categories { get; set; }

         [Display(Name = "Courses")]
         public int Courses { get; set; }

         [Display(Name = "Authors")]
         public int Authors { get; set; }

         public bool HasCategoriesUpdateLog { get; set; }
         public bool HasCoursesUpdateLog { get; set; }
         public bool HasAuthorsUpdateLog { get; set; }
      }


      public class CourseUpdateDetailsViewModel
      {
         public CourseUpdateDetailsViewModel()
         {
            Courses = new List<CourseUpdateListViewModel>();
         }

         public CourseUpdateListViewModel CoursesMetadata { get; set; }

         [Display(Name = "Update Operation")]
         public OperationType OperationType { get; set; }

         [Display(Name = "Update Event Id")]
         public int UpdateEventId { get; set; }

         [Display(Name = "Training Provider")]
         public string TrainingProviderName { get; set; }

         public List<CourseUpdateListViewModel> Courses { get; set; }
      }

      public class CourseUpdateListViewModel
      {
         public int CourseId { get; set; }

         [Display(Name = "Category")]
         public string CategoryTitle { get; set; }
         public string CategoryUrlName { get; set; }

         [Display(Name = "Course")]
         public string CourseTitle { get; set; }
         public string CourseUrlName { get; set; }

         [Display(Name = "Release Date")]
         [DisplayFormat(DataFormatString = "{0: dd MMM yyyy}")]
         public DateTime CourseReleaseDate { get; set; }
      }

      public class CourseChangesDetailsViewModel
      {
         public bool HasAuthorsChanges { get; set; }
         public string TrainingProviderName { get; set; }

         [Display(Name = "Current Content")]
         public CourseChangesViewModel CurrentContent { get; set; }

         [Display(Name = "Previous Content")]
         public CourseChangesViewModel PreviousContent { get; set; }
      }

      public class CourseChangesViewModel
      {
         [Display(Name = "Category")]
         public string CategoryTitle { get; set; }

         public string CategoryUrlName { get; set; }


         [Display(Name = "Title")]
         public string Title { get; set; }

         [Display(Name = "Url")]
         [DataType(DataType.Url)]
         [UIHint("UrlInNewWindow")]
         public string SiteUrl { get; set; }

         [Display(Name = "Title")]
         public string Description { get; set; }

         [Display(Name = "Has Subtitles")]
         [UIHint("YesNo")]
         public bool? HasClosedCaptions { get; set; }

         [Display(Name = "Level")]
         public CourseLevel? CourseLevel { get; set; }

         [Display(Name = "Duration")]
         [DataType(DataType.Duration)]
         public TimeSpan? Duration { get; set; }

         [Display(Name = "Release Date")]
         [DisplayFormat(DataFormatString = "{0: dd MMM yyyy}")]
         public DateTime? ReleaseDate { get; set; }

         public List<CourseAuthorsChangesViewModel> AuthorsChanges { get; set; }
      }

      public class CourseAuthorsChangesViewModel
      {
         [Display(Name = "Full Name")]
         public string FullName { get; set; }

         public string AuthorUrlName { get; set; }

         [Display(Name = "Is CoAuthor")]
         [UIHint("AuthorCoAuthor")]
         public bool? IsAuthorCoAuthor { get; set; }

         [Display(Name = "Update Operation")]
         public OperationType OperationType { get; set; }
      }



      public class AuthorUpdateDetailsViewModel
      {
         public AuthorUpdateDetailsViewModel()
         {
            Authors = new List<AuthorUpdateListViewModel>();
         }

         public AuthorUpdateListViewModel AuthorsMetadata { get; set; }

         [Display(Name = "Update Operation")]
         public OperationType OperationType { get; set; }

         [Display(Name = "Update Event Id")]
         public int UpdateEventId { get; set; }

         [Display(Name = "Training Provider")]
         public string TrainingProviderName { get; set; }

         public List<AuthorUpdateListViewModel> Authors { get; set; }
      }


      public class AuthorUpdateListViewModel
      {
         public int AuthorId { get; set; }

         [Display(Name = "Full Name")]
         public string FullName { get; set; }

         [Display(Name = "Url")]
         public string SiteUrl { get; set; }

         public string AuthorUrlName { get; set; }
      }


      public class AuthorChangesDetailsViewModel
      {
         public string TrainingProviderName { get; set; }

         [Display(Name = "Current Content")]
         public AuthorChangesViewModel CurrentContent { get; set; }

         [Display(Name = "Previous Content")]
         public AuthorChangesViewModel PreviousContent { get; set; }
      }

      public class AuthorChangesViewModel
      {
         [Display(Name = "Full Name")]
         public string FullName { get; set; }

         [Display(Name = "Url")]
         public string SiteUrl { get; set; }

      }



      public class CategoryUpdateDetailsViewModel
      {
         public CategoryUpdateDetailsViewModel()
         {
            Categories = new List<CategoryUpdateListViewModel>();
         }

         public CategoryUpdateListViewModel CategoriesMetadata;

         [Display(Name = "Update Operation")]
         public OperationType OperationType { get; set; }

         [Display(Name = "Update Event Id")]
         public int UpdateEventId { get; set; }

         [Display(Name = "Training Provider")]
         public string TrainingProviderName { get; set; }

         public List<CategoryUpdateListViewModel> Categories { get; set; }
      }


      public class CategoryUpdateListViewModel
      {
         public int CategoryId { get; set; }

         [Display(Name = "Title")]
         public string Title { get; set; }

         public string CategoryUrlName { get; set; }
      }


      public class CategoryChangesDetailsViewModel
      {
         public string TrainingProviderName { get; set; }

         [Display(Name = "Current Content")]
         public CategoryChangesViewModel CurrentContent { get; set; }

         [Display(Name = "Previous Content")]
         public CategoryChangesViewModel PreviousContent { get; set; }
      }

      public class CategoryChangesViewModel
      {
         [Display(Name = "Title")]
         public string Title { get; set; }

         [Display(Name = "Logo Url")]
         [DataType(DataType.Url)]
         public string LogoUrl { get; set; }

         [Display(Name = "Logo FileName")]
         public string LogoFileName { get; set; }

      }
   }
}