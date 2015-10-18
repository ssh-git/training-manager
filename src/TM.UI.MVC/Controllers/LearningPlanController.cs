using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
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
      public async Task<JsonNetResult> AjaxStart(int courseId)
      {
         var subscriptionChangeResult = await CatalogManager.SetStartStateAsync(courseId, UserId);

         return subscriptionChangeResult.Succeeded
            ? JsonNet(subscriptionChangeResult.Value)
            : JsonNetError(subscriptionChangeResult.Errors);
      }

      [HttpPost, ValidateAntiForgeryToken]
      public async Task<JsonNetResult> AjaxFinish(int courseId)
      {
         var subscriptionChangeResult = await CatalogManager.SetFinishStateAsync(courseId, UserId);

         return subscriptionChangeResult.Succeeded
            ? JsonNet(subscriptionChangeResult.Value)
            : JsonNetError(subscriptionChangeResult.Errors);
      }

      [HttpPost, ValidateAntiForgeryToken]
      public async Task<ActionResult> AjaxDelete(int courseId)
      {
         var result = await CatalogManager.DeleteCourseFromLearningPlanAsync(UserId, courseId);

         return result.Succeeded
            ? (ActionResult)new HttpStatusCodeResult(HttpStatusCode.NoContent)
            : JsonNetError(result.Errors);
      }

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
      public async Task<JsonNetResult> Subscription(LearningPlanViewModels.SubscriptionViewModel subscription)
      {
         if (ModelState.IsValid)
         {
            var result = await CatalogManager.UpdateCourseSubscriptionInfoAsync(UserId, subscription);

            if (result.Succeeded)
            {
               return JsonNet(subscription);
            }

            subscription.IsModelValid = false;
            subscription.ErrorMessage = string.Join(";\n", result.Errors);
         }
         
         return JsonNetModelError(subscription);
      }
   }
}