using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using TM.Data;
using TM.Shared;
using TM.UI.MVC.ControllableModel;
using TM.UI.MVC.Helpers;

namespace TM.UI.MVC.Areas.Admin.ViewModels
{
   [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
   [SuppressMessage("ReSharper", "LocalizableElement")]
   [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
   public class CoursesWithoutSpecializationsViewModels
   {
      #region Index View Model

      public class IndexViewModel
      {
         public int CategoryId { get; set; }

         [Display(Name = "Training Provider")]
         public string TrainingProviderName { get; set; }

         [Display(Name = "Category")]
         public string CategoryName { get; set; }

         [Display(Name = "Course Count")]
         public int CourseCount { get; set; }

         public static async Task<IndexControllableViewModel> ToControllableViewModelAsync(
            IQueryable<IndexViewModel> query,
            ControllableViewModelParams controlParams)
         {
            var controllableViewModel = new IndexControllableViewModel(controlParams);
            await controllableViewModel.InitializeAsync(query);
            return controllableViewModel;
         }
      }

      public class IndexSortStateModel
      {
         public enum State
         {
            None = 0,
            CategoryDesc,

            CourseCount,
            CourseCountDesc
         }

         public static IndexSortStateModel Create(string sortKey)
         {
            State sortState;
            Enum.TryParse(sortKey, true, out sortState);

            return new IndexSortStateModel
            {
               CategorySortState = sortState == State.None ? State.CategoryDesc : (State?) null,
               CourseCountSortState = sortState == State.CourseCount ? State.CourseCountDesc : State.CourseCount
            };
         }


         public State? CategorySortState { get; private set; }
         public State CourseCountSortState { get; private set; }
      }


      public class IndexControllableViewModel : ControllableViewModel<IndexViewModel, IndexSortStateModel>
      {
         public IndexControllableViewModel(IControllableViewModelParams viewModelParams, string defaultPageSize = "10")
            : base(viewModelParams, defaultPageSize)
         {
            SearchBoxTextPlaceholder = "Search by Category";
         }

         public IndexControllableViewModel(IControllableViewModelParams viewModelParams, string defaultPageSize,
            params string[] sizeList)
            : base(viewModelParams, defaultPageSize, sizeList)
         {
            SearchBoxTextPlaceholder = "Search by Category";
         }

         protected override IQueryable<IndexViewModel> ApplyFiltering(IQueryable<IndexViewModel> chainQuery,
            IFilterParam filterParam)
         {
            var selectedTrainingProviderName = filterParam.FilterKey;
            if (string.IsNullOrWhiteSpace(selectedTrainingProviderName))
            {
               return chainQuery;
            }

            chainQuery = chainQuery.Where(x => x.TrainingProviderName == selectedTrainingProviderName);

            return chainQuery;
         }

         protected override IQueryable<IndexViewModel> ApplySearching(IQueryable<IndexViewModel> chainQuery,
            ISearchParam searchParam)
         {
            var searchedCategoryName = searchParam.SearchKey;
            if (string.IsNullOrWhiteSpace(searchedCategoryName))
            {
               return chainQuery;
            }

            chainQuery = chainQuery.Where(x => x.CategoryName.Contains(searchedCategoryName));

            return chainQuery;
         }

         protected override IQueryable<IndexViewModel> ApplySorting(IQueryable<IndexViewModel> chainQuery,
            ISortParam sortParam)
         {
            IndexSortStateModel.State sortState;
            Enum.TryParse(sortParam.SortKey, true, out sortState);

            switch (sortState)
            {
               case IndexSortStateModel.State.CategoryDesc:
                  chainQuery = chainQuery.OrderByDescending(x => x.CategoryName);
                  break;
               case IndexSortStateModel.State.CourseCount:
                  chainQuery = chainQuery.OrderBy(x => x.CourseCount);
                  break;
               case IndexSortStateModel.State.CourseCountDesc:
                  chainQuery = chainQuery.OrderByDescending(x => x.CourseCount);
                  break;
               default:
                  chainQuery = chainQuery.OrderBy(x => x.CategoryName);
                  break;
            }

            return chainQuery;
         }

         protected override IndexSortStateModel CreateSortStateModel(IControllableViewModelParams controlParams)
         {
            var sortStateModel = IndexSortStateModel.Create(controlParams.SortKey);

            return sortStateModel;
         }

         protected override async Task InitializeFilterSelectList(IQueryable<IndexViewModel> chainQuery,
            IFilterParam filterParam)
         {
            const string all = "All Training Providers";

            var selectedTrainingProvider = filterParam.FilterKey ?? all;

            var availableTrainingProviders = await InputQuery.Select(x => x.TrainingProviderName)
               .Distinct()
               .ToListAsync();

            FilterSelectList = availableTrainingProviders
               .Select(x => new SelectListItem
               {
                  Text = x,
                  Value = x,
                  Selected = x == selectedTrainingProvider
               }).Concat(new[]
               {
                  new SelectListItem
                  {
                     Text = all,
                     Value = string.Empty,
                     Selected = selectedTrainingProvider == all
                  }
               });
         }
      }

      #endregion


      #region CategoryCourses View Model

      public class CategoryCoursesViewModel
      {
         private IEnumerable<SelectListItem> _specializationsSelectList;
         private List<SelectedSpecializationsModel> _selectedSpecializations;

         public string TrainingProviderName { get; set; }
         public int CategoryId { get; set; }
         public string CategoryTitle { get; set; }
         public string CategoryUrlName { get; set; }

         public List<CourseViewModel> Courses { get; set; }

         public IEnumerable<SelectListItem> SpecializationsSelectList
         {
            get
            {
               if (_specializationsSelectList != null)
               {
                  return _specializationsSelectList;
               }

               _specializationsSelectList = Specializations.AllSpecializations.GetFlags().Select(x => new SelectListItem
               {
                  Text = x.GetDisplayName().ToString(),
                  Value = x.ToString()
               }).ToList();

               return _specializationsSelectList;
            }
         }

         public List<SelectedSpecializationsModel> SelectedSpecializations
         {
            get
            {
               if (_selectedSpecializations != null)
               {
                  return _selectedSpecializations;
               }

               _selectedSpecializations = Courses.Select(x => new SelectedSpecializationsModel
               {
                  CourseId = x.CourseId,
                  Specializations = null
               }).ToList();

               return _selectedSpecializations;
            }
         }

      }

      public class CourseViewModel
      {
         public int CourseId { get; set; }
         public string CourseTitle { get; set; }
         public string CourseUrlName { get; set; }

         [Display(Name = "Release Date")]
         [DataType(DataType.Date)]
         [DisplayFormat(DataFormatString = "{0:dd MMM yyyy}")]
         public DateTime CourseReleaseDate { get; set; }
      }

      [SuppressMessage("ReSharper", "UnusedMember.Global")]
      public class CourseSpecializationsModel
      {
         public List<SelectedSpecializationsModel> Courses { get; set; }
      }

      public class SelectedSpecializationsModel
      {
         public int CourseId { get; set; }

         public IEnumerable<string> Specializations { get; set; }

         public Specializations SpecializationsEnum
         {
            get
            {
               if (Specializations != null && Specializations.Any())
               {
                  Specializations value;
                  Enum.TryParse(string.Join(",", Specializations), out value);

                  return value;
               }
               return Shared.Specializations.None;
            }
         }

         public IEnumerable<CourseSpecialization> GetCourseSpecializations()
         {
            var courseSpecialization = SpecializationsEnum.GetFlags()
               .Select(specialization => new CourseSpecialization
               {
                  CourseId = CourseId,
                  Specialization = specialization
               });

            return courseSpecialization;
         }
      }

      #endregion
   }
}