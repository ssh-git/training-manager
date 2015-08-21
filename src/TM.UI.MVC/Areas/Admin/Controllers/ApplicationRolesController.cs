using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.Owin;
using TM.UI.MVC.Areas.Admin.ViewModels;

namespace TM.UI.MVC.Areas.Admin.Controllers
{
   [SuppressMessage("ReSharper", "LocalizableElement")]
   [Authorize(Roles = AppConstants.UserRole.Administrator)]
   public class ApplicationRolesController : Controller
   {
      private ApplicationRoleManager _roleManager;

      public ApplicationRolesController()
      {

      }

      public ApplicationRolesController(ApplicationRoleManager roleManager)
      {
         _roleManager = roleManager;
      }

      private ApplicationRoleManager RoleManager
      {
         get { return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>(); }
      }

      
      public async Task<ActionResult> Index()
      {
         var roles = await RoleManager.Roles
            .ToListAsync();

         return View(roles);
      }

      public ActionResult Create()
      {
         var roleViewModel = new ApplicationRoleViewModel();
         return View(roleViewModel);
      }


      [HttpPost]
      [ValidateAntiForgeryToken]
      public async Task<ActionResult> Create([Bind(Include = "Name")] ApplicationRoleViewModel roleViewModel)
      {
         if (!ModelState.IsValid)
         {
            return View(roleViewModel);
         }

         var applicationRole = new ApplicationRole(roleViewModel.Name);
         var result = await RoleManager.CreateAsync(applicationRole);

         if (result.Succeeded)
         {
            return RedirectToAction("Index");
         }

         ModelState.AddModelError("", result.Errors.First());

         return View(roleViewModel);
      }


      public async Task<ActionResult> Edit(string name)
      {
         if (name == null)
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }

         var retrievedRole = await RoleManager.FindByNameAsync(name);

         if (retrievedRole == null)
         {
            return HttpNotFound();
         }

         var roleViewModel = new ApplicationRoleViewModel{Id = retrievedRole.Id, Name = retrievedRole.Name};

         return View(roleViewModel);
      }


      [HttpPost]
      [ValidateAntiForgeryToken]
      public async Task<ActionResult> Edit([Bind(Include = "Id,Name")] ApplicationRoleViewModel roleViewModel)
      {
         if (!ModelState.IsValid)
         {
            return View(roleViewModel);
         }

         var retrievedRole = await RoleManager.FindByIdAsync(roleViewModel.Id);
         var originalName = retrievedRole.Name;

         if (originalName == AppConstants.UserRole.Administrator &&
             roleViewModel.Name != AppConstants.UserRole.Administrator)
         {
            ModelState.AddModelError("",
               "You cannot change the name of the '" + AppConstants.UserRole.Administrator + "' role.");

            return View(roleViewModel);
         }

         if (originalName != AppConstants.UserRole.Administrator &&
             roleViewModel.Name == AppConstants.UserRole.Administrator)
         {
            ModelState.AddModelError("",
               "You cannot change the name of a role to '" + AppConstants.UserRole.Administrator + "'");

            return View(roleViewModel);
         }

         retrievedRole.Name = roleViewModel.Name;

         var result = await RoleManager.UpdateAsync(retrievedRole);

         if (!result.Succeeded)
         {
            ModelState.AddModelError("", result.Errors.First());

            return View(roleViewModel);
         }

         return RedirectToAction("Index");
      }


      public async Task<ActionResult> Delete(string name)
      {
         if (name == null)
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }

         var retrievedRole = await RoleManager.FindByNameAsync(name);
         if (retrievedRole == null)
         {
            return HttpNotFound();
         }

         return View(retrievedRole);
      }


      [HttpPost, ActionName("Delete")]
      [ValidateAntiForgeryToken]
      public async Task<ActionResult> DeleteConfirmed(string name)
      {
         var retrievedRole = await RoleManager.FindByNameAsync(name);

         if (retrievedRole.Name == AppConstants.UserRole.Administrator)
         {
            ModelState.AddModelError("", "You cannot delete '" + AppConstants.UserRole.Administrator + "' role.");

            return View(retrievedRole);
         }

         var result = await RoleManager.DeleteAsync(retrievedRole);

         if (!result.Succeeded)
         {
            ModelState.AddModelError("", result.Errors.First());

            return View(retrievedRole);
         }

         return RedirectToAction("Index");
      }

      protected override void Dispose(bool disposing)
      {
         if (disposing && _roleManager != null)
         {
            _roleManager.Dispose();
            _roleManager = null;
         }
         base.Dispose(disposing);
      }
   }
}
