using System.Threading.Tasks;
using System.Web.Mvc;
using TM.UI.MVC.Models;

namespace TM.UI.MVC.Controllers
{
   using VM = HomeViewModels;

   public class HomeController : ControllerBase<VM.HomeManager>
   {
      public HomeController(VM.HomeManager catalogManager) 
         : base(catalogManager)
      {
      }

      public ActionResult Error()
      {
         return View("Error");
      }

      public async Task<ActionResult> Home()
      {
         var catalogStatistic = await CatalogManager.GetCatalogStatisticAsync(UserSpecializations);

         var viewModel = new VM.HomeViewModel(Url, catalogStatistic);

         return View(viewModel);
      }
    
   }
}