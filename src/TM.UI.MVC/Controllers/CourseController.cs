using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using TM.UI.MVC.Infrastructure;
using TM.UI.MVC.Models;

namespace TM.UI.MVC.Controllers
{
   using VM = CourseViewModels;

   public class CourseController : ControllerBase<CourseViewModels.CoursesManager>
   {
      public CourseController(CourseViewModels.CoursesManager catalogManager) 
         : base(catalogManager)
      {
      }


      public async Task<ActionResult> CategoryCourses(string trainingProviderName, string categoryUrlName)
      {
         if (string.IsNullOrWhiteSpace(categoryUrlName) || string.IsNullOrWhiteSpace(trainingProviderName))
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }

         var viewModel = await CatalogManager.GetCoursesForCategoryAsync(trainingProviderName, categoryUrlName, UserId, UserSpecializations);

         if (viewModel == null)
         {
            return HttpNotFound();
         }


         return PartialView("_CourseListPartial", viewModel);
      }

      public async Task<ActionResult> AuthorCourses(string trainingProviderName, string authorUrlName )
      {
         if (string.IsNullOrWhiteSpace(authorUrlName) || string.IsNullOrWhiteSpace(trainingProviderName))
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }

         var viewModel = await CatalogManager.GetCoursesForAuthorAsync(authorUrlName, trainingProviderName, UserId, UserSpecializations);

         if (viewModel == null)
         {
            return HttpNotFound();
         }


         return PartialView("_CourseListPartial", viewModel);
      }

      public async Task<ActionResult> AddedCourses(string trainingProviderName, DateTime? addDate)
      {
         if (addDate == null || string.IsNullOrWhiteSpace(trainingProviderName))
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }

         var viewModel = await CatalogManager.GetAddedCoursesForDateAsync(addDate.Value, trainingProviderName, UserId, UserSpecializations);

         if (viewModel == null)
         {
            return HttpNotFound();
         }


         return PartialView("_CourseListPartial", viewModel);;
      }

      public async Task<ActionResult> Top5NewCourses()
      {
         var courses = await CatalogManager.GetTop5NewCoursesAsync(UserSpecializations);

         return PartialView("_TopCourseListPartial", courses);
      }

      public async Task<ActionResult> Top5MonthPopularCourses()
      {
         var courses = await CatalogManager.GetTop5MonthPopularCoursesAsync(UserSpecializations);

         return PartialView("_TopCourseListPartial", courses);
      }


      public ActionResult Courses(string trainingProviderName)
      {
         var courseCatalogViewModel = CatalogManager.GetCourseCatalog(trainingProviderName);

         if (courseCatalogViewModel == null)
         {
            return HttpNotFound();
         }

         return View(courseCatalogViewModel);
      }

      public async Task<ActionResult> Course(string trainingProviderName, string courseUrlName)
      {
         if (string.IsNullOrWhiteSpace(trainingProviderName) || string.IsNullOrWhiteSpace(courseUrlName))
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }

         var courseInfo = await CatalogManager.GetCourseInfoAsync(trainingProviderName,courseUrlName, UserId);

         if (courseInfo == null)
         {
            return HttpNotFound();
         }

         return View(courseInfo);
      }


      public ActionResult Search(string searchTerm)
      {
         var searchViewModel = new CourseViewModels.CourseSearchViewModel
         {
            SearchTerm = searchTerm
         };

         return View(searchViewModel);
      }


      [HttpPost, ValidateAntiForgeryToken]
      public async Task<JsonNetResult> AjaxSearch(CourseViewModels.CourseCatalogSearchRequest searchRequest)
      {
         if (!ModelState.IsValid)
         {
            return new JsonNetResult(new HttpStatusCodeResult(HttpStatusCode.BadRequest));
         }

         var searchResult = await CatalogManager.GetCourseCatalogSearchResultAsync(searchRequest, UserId, UserSpecializations);

         return new JsonNetResult(searchResult);
      }
   }
}