using System;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;
using TM.UI.MVC.Areas.Admin.ViewModels;
using TM.UI.MVC.ControllableModel;
using TM.UI.MVC.Helpers;

namespace TM.UI.MVC.Areas.Admin.Controllers
{
   using VM = ApplicationUserViewModels;

   [Authorize(Roles = AppConstants.UserRole.Administrator)]
   [SuppressMessage("ReSharper", "LocalizableElement")]
   public class ApplicationUsersController : Controller
   {
      private ApplicationRoleManager _roleManager;
      private ApplicationUserManager _userManager;

      public ApplicationUsersController()
      {

      }

      public ApplicationUsersController(ApplicationRoleManager roleManager, ApplicationUserManager userManager)
      {
         _roleManager = roleManager;
         _userManager = userManager;
      }

      private ApplicationRoleManager RoleManager
      {
         get { return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>(); }
      }

      private ApplicationUserManager UserManager
      {
         get { return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
      }



      public async Task<ActionResult> Index(ControllableViewModelParams modelParams)
      {
         var viewModelQuery = UserManager.Users
            .Select(user => new ApplicationUserViewModels.IndexViewModel
            {
               UserName = user.UserName,
               RegisteredOnUtc = user.RegisteredOnUtc,
               LastLoginOnUtc = user.LastLoginOnUtc,
               LockoutEnabled = user.LockoutEnabled,
               LockoutEndDateUtc = user.LockoutEndDateUtc,
               AccessFailedCount = user.AccessFailedCount,
               IsAdmin = RoleManager.Roles
                  .FirstOrDefault(role => role.Name == AppConstants.UserRole.Administrator)
                  .Users.Any(userRole => userRole.UserId == user.Id)
            });

         var controllableViewModel = await ApplicationUserViewModels.IndexViewModel.ToControlableViewModelAsync(viewModelQuery, modelParams);

         return View(controllableViewModel);
      }


      public async Task<ActionResult> Edit(string userName, ControllableViewModelParams modelParams)
      {
         if (userName == null)
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }

         var viewModel = await UserManager.Users.Where(x => x.UserName == userName)
            .Select(user => new ApplicationUserViewModels.EditViewModel
            {
               UserId = user.Id,
               UserName = user.UserName,
               LockoutEnabled = user.LockoutEnabled,
               LockoutEndDateUtc = user.LockoutEndDateUtc,
               RoleList = RoleManager.Roles
                  .Select(role => new SelectListItem
                  {
                     Value = role.Id,
                     Text = role.Name
                  }).ToList(),
               SelectedRoles = RoleManager.Roles
               .Where(role => role.Users.Any(userRole => userRole.UserId == user.Id))
               .Select(role => role.Id).ToList()
            }).SingleOrDefaultAsync();

         if (viewModel == null)
         {
            return HttpNotFound();
         }

         ViewData.SetControllableViewModelParams(modelParams);

         return View(viewModel);
      }


      [HttpPost]
      [ValidateAntiForgeryToken]
      public async Task<ActionResult> Edit(ApplicationUserViewModels.EditViewModel editModel, ControllableViewModelParams modelParams)
      {
         if (ModelState.IsValid)
         {
            do
            {
               var currentUser = await UserManager.FindByIdAsync(editModel.UserId);

               if (currentUser == null)
               {
                  return HttpNotFound();
               }

               var adminRole = await RoleManager.FindByNameAsync(AppConstants.UserRole.Administrator);
               var currentUserRolesIds = currentUser.Roles.Select(x => x.RoleId).ToList();

               // if user is currently stored having the Administrator role
               var isThisUserAnAdmin = currentUserRolesIds.Contains(adminRole.Id);

               // and the user did not have Administrator role checked
               var isThisUserAdminDeselected = editModel.SelectedRoles.All(roleId => roleId != adminRole.Id);

               // and the current stored count of users with Administrator role == 1
               var isOnlyOneUserAnAdmin = adminRole.Users.Count == 1;

               // then prevent the removal of the Administrator role.
               if (isThisUserAnAdmin && isThisUserAdminDeselected && isOnlyOneUserAnAdmin)
               {
                  ModelState.AddModelError("", "At least one user must retain the 'administrator' role.");
                  break;
               }

               if (currentUser.LockoutEnabled != editModel.LockoutEnabled)
               {
                  currentUser.LockoutEnabled = editModel.LockoutEnabled;

                  var result = await UserManager.UpdateAsync(currentUser);
                  if (!result.Succeeded)
                  {
                     ModelState.AddModelError("", result.Errors.First());
                     break;
                  }
               }

               var allRoles = await RoleManager.Roles.ToListAsync();

               var deletedRolesIds = currentUserRolesIds.Except(editModel.SelectedRoles).ToList();

               if (deletedRolesIds.Any())
               {
                  var rolesToRemove = allRoles
                     .Where(x => deletedRolesIds.Contains(x.Id)).Select(x => x.Name)
                     .ToArray();

                  var result = await UserManager.RemoveFromRolesAsync(currentUser.Id, rolesToRemove);

                  if (!result.Succeeded)
                  {
                     ModelState.AddModelError("", result.Errors.First());
                     break;
                  }
               }


               var newRolesIds = editModel.SelectedRoles.Except(currentUserRolesIds).ToArray();

               if (newRolesIds.Any())
               {
                  var rolesToAdd = allRoles
                     .Where(x => newRolesIds.Contains(x.Id)).Select(x => x.Name)
                     .ToArray();

                  var result = await UserManager.AddToRolesAsync(currentUser.Id, rolesToAdd);

                  if (!result.Succeeded)
                  {
                     ModelState.AddModelError("", result.Errors.First());
                     break;
                  }
               }

               return RedirectToAction("Index", modelParams);
#pragma warning disable 162
            } while (false);
#pragma warning restore 162
         }

         editModel.RoleList = await RoleManager.Roles
            .Select(x => new SelectListItem
            {
               Text = x.Name,
               Value = x.Id
            }).ToListAsync();

         return View(editModel);
      }


      [HttpPost]
      [ValidateAntiForgeryToken]
      public async Task<ActionResult> LockAccount(string userId, ControllableViewModelParams modelParams)
      {
         var currentUser = await UserManager.FindByIdAsync(userId);
         if (currentUser == null)
         {
            return HttpNotFound();
         }

         var userIsAnAdministrator = await UserManager.IsInRoleAsync(currentUser.Id, AppConstants.UserRole.Administrator);

         if (userIsAnAdministrator)
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Cannot lock user in an administrator role.");
         }

         await UserManager.ResetAccessFailedCountAsync(currentUser.Id);
         await UserManager.SetLockoutEndDateAsync(currentUser.Id, DateTimeOffset.UtcNow.AddYears(1));

         return RedirectToAction("Index", modelParams);
      }


      [HttpPost]
      [ValidateAntiForgeryToken]
      public async Task<ActionResult> UnLockAccount(string userId, ControllableViewModelParams modelParams)
      {
         var currentUser = await UserManager.FindByIdAsync(userId);
         if (currentUser == null)
         {
            return HttpNotFound();
         }

         await UserManager.ResetAccessFailedCountAsync(currentUser.Id);
         await UserManager.SetLockoutEndDateAsync(currentUser.Id, DateTimeOffset.UtcNow);

         return RedirectToAction("Index", modelParams);
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
            if (_roleManager != null)
            {
               _roleManager.Dispose();
               _roleManager = null;
            }
         }
         base.Dispose(disposing);
      }
   }
}
