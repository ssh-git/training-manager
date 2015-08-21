using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using TM.UI.MVC.Models;

namespace TM.UI.MVC.Controllers
{
   using VM = AuthorViewModels;

   public class AuthorController : ControllerBase<AuthorViewModels.AuthorsManager>
   {
      public AuthorController(AuthorViewModels.AuthorsManager authorsManager)
         : base(authorsManager)
      {
      }

      public async Task<ActionResult> Authors(string trainingProviderName)
      {
         var authorCatalog = await CatalogManager.GetAuthorsCatalogAsync(trainingProviderName, UserSpecializations);

         if (authorCatalog == null)
         {
            return HttpNotFound();
         }

         return View(authorCatalog);
      }

      public async Task<ActionResult> Author(string authorUrlName, string trainingProviderName)
      {
         if (string.IsNullOrWhiteSpace(trainingProviderName) || string.IsNullOrWhiteSpace(authorUrlName))
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }

         var authorInfo = await CatalogManager.GetAuthorInfoAsync(authorUrlName, trainingProviderName, UserSpecializations);

         if (authorInfo == null)
         {
            return HttpNotFound();
         }

         return View(authorInfo);
      }
   }
}