using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using TM.UI.MVC.Models;

namespace TM.UI.MVC.Controllers
{
   using VM = TrainingProviderViewModels;

   public class TrainingProviderController : ControllerBase<VM.TrainingProvidersManager>
   {

      public TrainingProviderController(VM.TrainingProvidersManager catalogManager)
         : base(catalogManager)
      {
         
      }


      public async Task<ActionResult> TrainingProviders()
      {
         var viewModel = await CatalogManager.GetTrainingProviderCatalogAsync(UserSpecializations);

         return View(viewModel);
      }

      public async Task<ActionResult> TrainingProvider(string trainingProviderName, DateTime? addDate)
      {
         if (string.IsNullOrWhiteSpace(trainingProviderName))
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }

         var trainingProviderInfo = await CatalogManager.GetTrainingProviderInfoAsync(trainingProviderName, UserSpecializations);

         if (trainingProviderInfo == null)
         {
            return HttpNotFound();
         }

         trainingProviderInfo.SelectedUpdateDate = addDate.HasValue &&
                                                   trainingProviderInfo.UpdateDates.BinarySearch(addDate.Value.Date) >= 0
            ? addDate.Value.Date
            : trainingProviderInfo.UpdateDates.Last();
         

         return View(trainingProviderInfo);
      }
   }
}