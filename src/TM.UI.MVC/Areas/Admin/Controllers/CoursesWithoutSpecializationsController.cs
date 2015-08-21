using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using TM.Data;
using TM.UI.MVC.Areas.Admin.ViewModels;
using TM.UI.MVC.ControllableModel;
using TM.UI.MVC.Helpers;

namespace TM.UI.MVC.Areas.Admin.Controllers
{
   using VM = CoursesWithoutSpecializationsViewModels;

   [Authorize(Roles = AppConstants.UserRole.Administrator)]
   public class CoursesWithoutSpecializationsController : Controller
   {
      private CatalogDbContext _db;

      public CoursesWithoutSpecializationsController(CatalogDbContext db)
      {
         _db = db;
      }


      public async Task<ActionResult> Index(ControllableViewModelParams modelParams)
      {
         var coursesQuery = _db.Courses.Where(x => !x.IsDeleted &&
                                                   !x.CourseSpecializations.Any());

         var viewModelQuery = _db.Categories
            .Where(x => coursesQuery.Any(course => course.CategoryId == x.Id) && !x.IsDeleted)
            .Select(x => new CoursesWithoutSpecializationsViewModels.IndexViewModel
            {
               TrainingProviderName = x.TrainingProvider.Name,
               CategoryId = x.Id,
               CategoryName = x.Title,
               CourseCount = coursesQuery.Count(course => course.CategoryId == x.Id)
            });

         var controllableViewModel = await CoursesWithoutSpecializationsViewModels.IndexViewModel.ToControllableViewModelAsync(viewModelQuery, modelParams);

         return View(controllableViewModel);
      }


      public async Task<ActionResult> CategoryCourses(int? categoryId, ControllableViewModelParams modelParams)
      {
         if (categoryId == null)
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }

         var viewModel = await GetCategoryCoursesViewModel(categoryId.Value);

         if (viewModel == null)
         {
            return HttpNotFound();
         }

         ViewData.SetControllableViewModelParams(modelParams);

         return View(viewModel);
      }


      [HttpPost, ValidateAntiForgeryToken]
      public async Task<ActionResult> CategoryCourses(int? categoryId, ControllableViewModelParams modelParams,
         IEnumerable<CoursesWithoutSpecializationsViewModels.SelectedSpecializationsModel> selectedSpecializations)
      {
         if (categoryId == null)
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }

         if (ModelState.IsValid)
         {
            do
            {
               var courseSpecializations = selectedSpecializations
                  .Where(x => x.Specializations != null && x.Specializations.Any())
                  .SelectMany(x => x.GetCourseSpecializations());

               _db.CourseSpecializations.AddRange(courseSpecializations);

               try
               {
                  await _db.SaveChangesAsync();
               }
                  // ReSharper disable once CatchAllClause
               catch (Exception ex)
               {
                  ModelState.AddModelError("", ex.ToString());
                  break;
               }

               return RedirectToAction("Index", modelParams);

               // only for flow control
#pragma warning disable 162
            } while (false);
#pragma warning restore 162
         }

         var viewModel = await GetCategoryCoursesViewModel(categoryId.Value);

         if (viewModel != null)
         {
            ViewData.SetControllableViewModelParams(modelParams);
         }

         return View(viewModel);
      }


      private async Task<CoursesWithoutSpecializationsViewModels.CategoryCoursesViewModel> GetCategoryCoursesViewModel(int categoryId)
      {
         var viewModel = await _db.Categories.Where(x => x.Id == categoryId)
            .Select(category => new CoursesWithoutSpecializationsViewModels.CategoryCoursesViewModel
            {
               TrainingProviderName = category.TrainingProvider.Name,
               CategoryId = category.Id,
               CategoryTitle = category.Title,
               CategoryUrlName = category.UrlName,
               Courses = category.Courses
               .Where(course => !course.IsDeleted && !course.CourseSpecializations.Any())
               .Select(course => new CoursesWithoutSpecializationsViewModels.CourseViewModel
               {
                  CourseId = course.Id,
                  CourseTitle = course.Title,
                  CourseUrlName = course.UrlName,
                  CourseReleaseDate = course.ReleaseDate
               }).ToList()
            }).SingleOrDefaultAsync();

         return viewModel;
      }

      protected override void Dispose(bool disposing)
      {
         if (disposing && _db != null)
         {

            _db.Dispose();
            _db = null;
         }

         base.Dispose(disposing);
      }
   }
}
