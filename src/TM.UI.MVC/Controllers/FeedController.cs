using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using TM.UI.MVC.Models;

namespace TM.UI.MVC.Controllers
{
   using VM = SyndicationFeedModels;

   public class FeedController : ControllerBase<VM.SyndicationFeedsManager>
   {
      public FeedController(VM.SyndicationFeedsManager catalogManager)
         : base(catalogManager)
      {
      }

      public async Task<ActionResult> NewCoursesAsync()
      {
         var lastUpdateId = await CatalogManager.GetLastSuccessUpdateIdWithNewCoursesAsync();
         var feedString = await CatalogManager.CreateUpdateFeedStringAsync(lastUpdateId, Request.Url, Url);

         Response.Cache.SetExpires(DateTime.UtcNow.AddHours(24.0));
         Response.Cache.SetCacheability(HttpCacheability.Server);
         Response.Cache.AddValidationCallback(CheckCacheItem, lastUpdateId);

         return Content(feedString, "text/xml;charset=utf-8", Encoding.UTF8);
      }

      [SuppressMessage("ReSharper", "RedundantAssignment")]
      private static void CheckCacheItem(HttpContext context, object previousUpdateId, ref HttpValidationStatus validationstatus)
      {
         using (var catalogManager = new VM.SyndicationFeedsManager())
         {
            var currentUpdateId = catalogManager.GetLastSuccessUpdateIdWithNewCourses();
            validationstatus = currentUpdateId == (int) previousUpdateId
               ? HttpValidationStatus.Valid
               : HttpValidationStatus.Invalid;
         }
      }
   }
}