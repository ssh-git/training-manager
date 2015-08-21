using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using TM.UI.MVC.Infrastructure;
using TM.UI.MVC.Models;

namespace TM.UI.MVC.Controllers
{
   using VM = LearningPlanViewModels;

   [Authorize]
   public class LearningPlanController : ControllerBase<VM.LearningPlanManager>
   {
      public LearningPlanController(VM.LearningPlanManager catalogManager)
         : base(catalogManager)
      {
      }


      public async Task<ActionResult> LearningPlan()
      {
         var learningPlan = await CatalogManager.GetLearningPlanAsync(UserId, UserSpecializations);

         if (learningPlan == null)
         {
            return View("Empty");
         }

         return View(learningPlan);
      }

      [HttpPost, ValidateAntiForgeryToken]
      public async Task<ActionResult> Add(int courseId)
      {
         if (courseId == 0)
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }

         await CatalogManager.AddCourseToLearningPlanAsync(UserId, courseId);

         var courseRouteParam = await CatalogManager.GetCourseRouteParamAsync(courseId);

         return RedirectToRoute(AppConstants.RouteNames.Course, courseRouteParam);
      }


      [HttpPost, ValidateAntiForgeryToken]
      public async Task<ActionResult> AjaxAdd(int courseId)
      {
         if (courseId == 0)
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }

         var result = await CatalogManager.AddCourseToLearningPlanAsync(UserId, courseId);

         return result.Succeeded
            ? new HttpStatusCodeResult(HttpStatusCode.NoContent)
            : new HttpStatusCodeResult(HttpStatusCode.BadRequest, result.Errors.First());
      }

      [HttpPost, ValidateAntiForgeryToken]
      public async Task<ActionResult> AjaxStart(int courseId)
      {
         var subscriptionChangeResult = await CatalogManager.SetStartStateAsync(courseId, UserId);

         if (subscriptionChangeResult.Succeeded)
         {
            return new JsonNetResult(subscriptionChangeResult.Value);
         }

         return new HttpStatusCodeResult(HttpStatusCode.BadRequest, subscriptionChangeResult.Errors.First());
      }

      [HttpPost, ValidateAntiForgeryToken]
      public async Task<ActionResult> AjaxFinish(int courseId)
      {
         var subscriptionChangeResult = await CatalogManager.SetFinishStateAsync(courseId, UserId);

         if (subscriptionChangeResult.Succeeded)
         {
            return new JsonNetResult(subscriptionChangeResult.Value);
         }

         return new HttpStatusCodeResult(HttpStatusCode.BadRequest, subscriptionChangeResult.Errors.First());
      }

      [HttpPost, ValidateAntiForgeryToken]
      public async Task<ActionResult> AjaxDelete(int courseId)
      {
         var result = await CatalogManager.DeleteCourseFromLearningPlanAsync(UserId, courseId);

         return result.Succeeded
            ? new HttpStatusCodeResult(HttpStatusCode.NoContent)
            : new HttpStatusCodeResult(HttpStatusCode.BadRequest, result.Errors.First());
      }


      /*public async Task<ActionResult> Subscription(int? courseId)
            {
               if (courseId == null)
               {
                  return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
               }

               var subscriptionInfoResult = await CatalogManager.GetCourseSubscriptionInfo(UserId, courseId.Value);

               if (subscriptionInfoResult.Succeeded)
               {
                  return new JsonNetResult(subscriptionInfoResult.Value);
               }

               return new HttpStatusCodeResult(HttpStatusCode.BadRequest, subscriptionInfoResult.Errors.First());
            }*/

      /*[HttpPost, ValidateAntiForgeryToken]
      public async Task<ActionResult> Subscription(VM.SubscriptionViewModel subscription)
      {
         if (ModelState.IsValid)
         {

         } else
         {
            subscription.ValidationMessage = string.Join(";", ModelState.Values
               .Where(x => x.Errors.Count > 0)
               .SelectMany(x => x.Errors)
               .Select(x => x.ErrorMessage));

            return new JsonNetResult(subscription);
         }
         return null;
      }*/

      public async Task<ActionResult> Subscription(int? courseId)
      {
         if (courseId == null)
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }

         var subscriptionInfoResult = await CatalogManager.GetCourseSubscriptionInfoAsync(UserId, courseId.Value);

         if (subscriptionInfoResult.Succeeded)
         {
            return PartialView("_SubscriptionFormPartial", subscriptionInfoResult.Value);
         }

         return new HttpStatusCodeResult(HttpStatusCode.BadRequest, subscriptionInfoResult.Errors.First());
      }

      [HttpPost, ValidateAntiForgeryToken]
      public async Task<ActionResult> Subscription(LearningPlanViewModels.SubscriptionViewModel subscription)
      {
         if (ModelState.IsValid)
         {
            var result = await CatalogManager.UpdateCourseSubscriptionInfoAsync(UserId, subscription);

            if (result.Succeeded)
            {
               return new JsonNetResult(subscription);
            }

            subscription.IsModelValid = false;
            subscription.ErrorMessage = string.Join(";\r\n", result.Errors);

            return new JsonNetResult(subscription);
         }

         var errorMessage = string.Join(";\r\n", ModelState.Where(x => x.Value.Errors.Any())
            .SelectMany(x => x.Value.Errors)
            .Select(x => x.ErrorMessage));

         subscription.IsModelValid = false;
         subscription.ErrorMessage = errorMessage;
         return new JsonNetResult(subscription);
      }
   }
}