using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using TM.Shared;
using TM.UI.MVC.Infrastructure;

namespace TM.UI.MVC.Controllers
{
   public abstract class ControllerBase<TCatalogManager> : Controller
      where TCatalogManager : CatalogManagerBase
   {
      private ApplicationUserManager _userManager;
      private string _userId = string.Empty;
      private Specializations? _userSpecializations;

      protected TCatalogManager CatalogManager;

      protected ControllerBase(TCatalogManager catalogManager)
      {
         CatalogManager = catalogManager;
      }


      protected string UserId
      {
         get
         {
            if (User.Identity.IsAuthenticated)
            {
               _userId = User.Identity.GetUserId();
            }

            return _userId;
         }
      }

      protected Specializations? UserSpecializations
      {
         get
         {
            if (User.Identity.IsAuthenticated)
            {
               return _userSpecializations ?? (_userSpecializations = UserManager.GetUserSpecializations(UserId));
            }

            return null;
         }
      }


      protected ApplicationUserManager UserManager
      {
         get
         {
            return _userManager ?? (_userManager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>());
         }
      }

      public JsonNetResult<T> JsonNet<T>(T model)
      {
         return new JsonNetResult<T>
         {
            Data = model
         };
      }

      public JsonNetResult<T> JsonNetModelError<T>(T model)
      {
         var result = new JsonNetResult<T> { Data = model };
         var errors = ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage);
         result.AddErrors(errors);
         return result;
      }

      public JsonNetResult JsonNetError(IEnumerable<string> errors)
      {
         var result = new JsonNetResult();
         result.AddErrors(errors);
         return result;
      }


      protected override void Dispose(bool disposing)
      {
         if (disposing)
         {
            if (CatalogManager != null)
            {
               CatalogManager.Dispose();
               CatalogManager = null;
            }
            if (_userManager != null)
            {
               _userManager.Dispose();
               _userManager = null;
            }
         }

         base.Dispose(disposing);
      }
   }
}