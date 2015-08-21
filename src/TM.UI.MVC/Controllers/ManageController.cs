using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using TM.UI.MVC.Models;

namespace TM.UI.MVC.Controllers
{
   [Authorize]
   public class ManageController : Controller
   {
      private ApplicationSignInManager _signInManager;
      private ApplicationUserManager _userManager;

      public ManageController()
      {
      }

      public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
      {
         UserManager = userManager;
         SignInManager = signInManager;
      }

      public ApplicationSignInManager SignInManager
      {
         get
         {
            return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
         }
         private set
         {
            _signInManager = value;
         }
      }

      public ApplicationUserManager UserManager
      {
         get
         {
            return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
         }
         private set
         {
            _userManager = value;
         }
      }



      public ActionResult Message(ManageMessageId? message)
      {
         var statusMessage =
             message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
             : "";

         return View((object)statusMessage);
      }



      public ActionResult ChangePassword()
      {
         return View(new ChangePasswordViewModel());
      }


      [HttpPost, ValidateAntiForgeryToken]
      public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
      {
         if (!ModelState.IsValid)
         {
            return View(model);
         }
         var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
         if (result.Succeeded)
         {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
               await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Message", new { Message = ManageMessageId.ChangePasswordSuccess });
         }
         AddErrors(result);

         return View(model);
      }



      public async Task<ActionResult> ChangeSpecializations(string returnUrl)
      {
         ViewBag.ReturnUrl = returnUrl;
         var selectedSpecializations = await UserManager.GetUserSpecializationsAsync(User.Identity.GetUserId());
         
         var model = new SpecializationsListViewModel(selectedSpecializations);

         return View(model);
      }


      [HttpPost, ValidateAntiForgeryToken]
      public async Task<ActionResult> ChangeSpecializations(SpecializationsListViewModel model, string returnUrl)
      {
         ViewBag.ReturnUrl = returnUrl;
         if (!ModelState.IsValid)
         {
            return View(model);
         }

         var result = await UserManager.SetUserSpecializationsAsync(User.Identity.GetUserId(),model.SelectedSpecializations);
         if (result.Succeeded)
         {
            return RedirectToLocal(returnUrl);
         }

         AddErrors(result);

         return View(model);
      }


      private ActionResult RedirectToLocal(string returnUrl)
      {
         if (Url.IsLocalUrl(returnUrl))
         {
            return Redirect(returnUrl);
         }
         return RedirectToAction("Home", "Home");
      }


      protected override void Dispose(bool disposing)
      {
         if (disposing)
         {
            if (_userManager != null)
            {
               _userManager.Dispose();
               _userManager = null;
            }

            if (_signInManager != null)
            {
               _signInManager.Dispose();
               _signInManager = null;
            }
           
         }

         base.Dispose(disposing);
      }

      #region Helpers

      private void AddErrors(IdentityResult result)
      {
         foreach (var error in result.Errors)
         {
            ModelState.AddModelError("", error);
         }
      }

      public enum ManageMessageId
      {
         ChangePasswordSuccess
      }

      #endregion
   }
}