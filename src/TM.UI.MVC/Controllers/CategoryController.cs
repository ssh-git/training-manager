using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using TM.UI.MVC.Models;

namespace TM.UI.MVC.Controllers
{
   using VM = CategoryViewModels;

   public class CategoryController : ControllerBase<VM.CategoriesManager>
   {
      public CategoryController(VM.CategoriesManager catalogManager)
         : base(catalogManager)
      {
      }

      public async Task<ActionResult> Categories(string trainingProviderName)
      {
         var categoryCatalog = await CatalogManager.GetCategoriesCatalogAsync(UserSpecializations, trainingProviderName);

         if (categoryCatalog == null)
         {
            return HttpNotFound();
         }

         return View(categoryCatalog);
      }

      public async Task<ActionResult> Category(string trainingProviderName, string categoryUrlName)
      {
         if (string.IsNullOrWhiteSpace(trainingProviderName) || string.IsNullOrWhiteSpace(categoryUrlName))
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }

         var categoryInfo = await CatalogManager.GetCategoryInfoAsync(trainingProviderName, categoryUrlName);

         if (categoryInfo == null)
         {
            return HttpNotFound();
         }

         return View(categoryInfo);
      }
   }
}