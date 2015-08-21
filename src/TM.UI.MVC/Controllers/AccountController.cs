using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using TM.UI.MVC.Models;

namespace TM.UI.MVC.Controllers
{
   [Authorize]
   public class AccountController : Controller
   {
      private ApplicationSignInManager _signInManager;
      private ApplicationUserManager _userManager;

      public AccountController()
      {
      }

      public AccountController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
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

      private IAuthenticationManager AuthenticationManager
      {
         get
         {
            return HttpContext.GetOwinContext().Authentication;
         }
      }


     
      [AllowAnonymous]
      public ActionResult Login(string returnUrl)
      {
         ViewBag.ReturnUrl = returnUrl;

         return View(new LoginViewModel());
      }

      
      [HttpPost]
      [AllowAnonymous]
      [ValidateAntiForgeryToken]
      public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
      {
         ViewBag.ReturnUrl = returnUrl;
         if (!ModelState.IsValid)
         {
            return View(model);
         }
         
         var result = await SignInManager.PasswordSignInAsync(model.Nickname, model.Password, model.RememberMe, shouldLockout: true);
         switch (result)
         {
            case SignInStatus.Success:
               if (model.Nickname == "admin")
               {
                  return RedirectToAction("ChangeDefaultAdminNameAndPassword");
               }
               await UserManager.SaveUserLoginDateAsync(model.Nickname);
               return RedirectToLocal(returnUrl);
            case SignInStatus.LockedOut:
               return View("Lockout");
            default:
               ModelState.AddModelError("", "Invalid login attempt.");
               return View(model);
         }
      }

      [Authorize(Roles = AppConstants.UserRole.Administrator, Users = "admin")]
      public ActionResult ChangeDefaultAdminNameAndPassword()
      {
         return View(new DefaultAdminViewModel());
      }

      [HttpPost]
      [Authorize(Roles = AppConstants.UserRole.Administrator, Users = "admin")]
      public async  Task<ActionResult> ChangeDefaultAdminNameAndPassword(DefaultAdminViewModel adminViewModel)
      {
         if (ModelState.IsValid)
         {
            var currentUser = await UserManager.FindByNameAsync("admin");

            currentUser.UserName = adminViewModel.UserName;
            var result = await UserManager.UpdateAsync(currentUser);
            if (!result.Succeeded)
            {
               ModelState.AddModelError("", result.Errors.First());
               return View(adminViewModel);
            }

            result = await UserManager.ChangePasswordAsync(currentUser.Id, "admin", adminViewModel.Password);

            if (!result.Succeeded)
            {
               ModelState.AddModelError("", result.Errors.First());
               return View(adminViewModel);
            }
         }
         
         return RedirectToAction("Login");
      }


     
      [AllowAnonymous]
      public ActionResult Register()
      {
         return View(new RegisterViewModel());
      }
      
     
      [AllowAnonymous]
      [HttpPost, ValidateAntiForgeryToken]
      public async Task<ActionResult> Register(RegisterViewModel model)
      {
         if (!ModelState.IsValid) return View(model);

         var user = new ApplicationUser { UserName = model.Nickname, RegisteredOnUtc = DateTime.UtcNow};
         var result = await UserManager.CreateAsync(user, model.Password);

         if (result.Succeeded)
         {
            result =
               await
                  UserManager.SetUserSpecializationsAsync(user.Id,
                     model.SpecializationsList.SelectedSpecializations);

            if (result.Succeeded)
            {
               await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
               await UserManager.SaveUserLoginDateAsync(model.Nickname);

               return RedirectToAction("Home","Home");
            }
         }

         AddErrors(result);

         // If we got this far, something failed, redisplay form
         return View(model);
      }

     
      [HttpPost, ValidateAntiForgeryToken]
      public ActionResult LogOff()
      {
         AuthenticationManager.SignOut();

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

      private ActionResult RedirectToLocal(string returnUrl)
      {
         if (Url.IsLocalUrl(returnUrl))
         {
            return Redirect(returnUrl);
         }

         return RedirectToAction("Home", "Home");
      }

      #endregion
   }
}