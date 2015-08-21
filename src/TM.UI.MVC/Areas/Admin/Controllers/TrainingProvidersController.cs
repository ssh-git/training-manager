using System;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using TM.Data;
using TM.UI.MVC.Areas.Admin.ViewModels;

namespace TM.UI.MVC.Areas.Admin.Controllers
{
   using VM = TrainingProviderViewModels;

   [Authorize(Roles = AppConstants.UserRole.Administrator)]
   public class TrainingProvidersController : Controller
   {
      private CatalogDbContext _db;

      public TrainingProvidersController(CatalogDbContext db)
      {
         _db = db;
      }


      public async Task<ActionResult> Index()
      {
         var viewModel = await _db.TrainingProviders.Select(x => new TrainingProviderViewModels.IndexViewModel
          {
             Id = x.Id,
             Name = x.Name,
             SiteUrl = x.SiteUrl,
             CurrentLogo = new TrainingProviderViewModels.LogoViewModel { FileName = x.LogoFileName },
             UpdateFrequencyHours = x.UpdateFrequencyHours,
             AllowedUpdateUtcHoursString = x.AllowedUpdateUtcHoursString,
             IsDeleted = x.IsDeleted
          }).ToListAsync();

         return View(viewModel);
      }


      public async Task<ActionResult> Details(int? id)
      {
         if (id == null)
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }

         var viewModel = await _db.TrainingProviders
            .Where(x => x.Id == id)
            .Select(x => new TrainingProviderViewModels.DetailsViewModel
          {
             Id = x.Id,
             Name = x.Name,
             Description = x.Description,
             SiteUrl = x.SiteUrl,
             CurrentLogo = new TrainingProviderViewModels.LogoViewModel { FileName = x.LogoFileName },
             UpdateFrequencyHours = x.UpdateFrequencyHours,
             AllowedUpdateUtcHoursString = x.AllowedUpdateUtcHoursString,
             SourceLocation = x.SourceLocation,
             SourceUrl = x.SourceUrl,
             AssemblyType = x.AssemblyType,
             IsDeleted = x.IsDeleted
          }).SingleOrDefaultAsync();

         if (viewModel == null)
         {
            return HttpNotFound();
         }
         return View(viewModel);
      }


      public ActionResult Create()
      {
         return View(new TrainingProviderViewModels.CreateViewModel());
      }

      [HttpPost]
      [ValidateAntiForgeryToken]
      [SuppressMessage("ReSharper", "StringLiteralTypo")]
      public async Task<ActionResult> Create(TrainingProviderViewModels.CreateViewModel createModel)
      {
         var validImageTypes = new[]
         {
            "image/gif",
            "image/jpeg",
            "image/pjpeg",
            "image/png"
         };

         var logoUpload = createModel.LogoUpload;
         if (logoUpload == null || logoUpload.ContentLength == 0)
         {
            ModelState.AddModelError("LogoUpload", "Logo file is required.");
         } else if (!validImageTypes.Contains(logoUpload.ContentType))
         {
            ModelState.AddModelError("LogoUpload", "Please choose either a GIF, JPG or PNG image.");
         }

         if (ModelState.IsValid)
         {
            try
            {
               var logoSavePath = Path.Combine(Server.MapPath(AppConstants.VirtualPaths.TrainingProvidersContent), createModel.LogoUpload.FileName);
               createModel.LogoUpload.SaveAs(logoSavePath);

               _db.TrainingProviders.Add(new TrainingProvider
               {
                  Name = createModel.Name,
                  Description = createModel.Description,
                  SiteUrl = createModel.SiteUrl,
                  LogoFileName = createModel.LogoUpload.FileName,
                  UpdateFrequencyHours = createModel.UpdateFrequencyHours,
                  AllowedUpdateUtcHours = createModel.SelectedUpdateHours,
                  SourceUrl = createModel.SourceUrl,
                  SourceLocation = createModel.SourceLocation,
                  AssemblyType = createModel.AssemblyType,
                  IsDeleted = createModel.IsDeleted
               });

               await _db.SaveChangesAsync();

               return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
               ModelState.AddModelError("", ex.Message);

               return View(createModel);
            }
         }

         return View(createModel);
      }


      public async Task<ActionResult> Edit(int? id)
      {
         if (id == null)
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }

         var viewModel = await _db.TrainingProviders.Where(x => x.Id == id)
            .Select(x => new TrainingProviderViewModels.EditViewModel
         {
            Id = x.Id,
            Name = x.Name,
            Description = x.Description,
            SiteUrl = x.SiteUrl,
            CurrentLogo = new TrainingProviderViewModels.LogoViewModel { FileName = x.LogoFileName },
            UpdateFrequencyHours = x.UpdateFrequencyHours,
            AllowedUpdateUtcHoursString = x.AllowedUpdateUtcHoursString,
            SourceLocation = x.SourceLocation,
            SourceUrl = x.SourceUrl,
            AssemblyType = x.AssemblyType,
            IsDeleted = x.IsDeleted
         }).SingleOrDefaultAsync();



         if (viewModel == null)
         {
            return HttpNotFound();
         }

         return View(viewModel);
      }


      [HttpPost]
      [ValidateAntiForgeryToken]
      [SuppressMessage("ReSharper", "StringLiteralTypo")]
      public async Task<ActionResult> Edit(TrainingProviderViewModels.EditViewModel editModel)
      {
         var validImageTypes = new[]
         {
            "image/gif",
            "image/jpeg",
            "image/pjpeg",
            "image/png"
         };

         var logoUpload = editModel.LogoUpload;
         if (logoUpload != null && logoUpload.ContentLength != 0 && !validImageTypes.Contains(logoUpload.ContentType))
         {
            ModelState.AddModelError("LogoUpload", "Please choose either a GIF, JPG or PNG image.");
         }

         if (ModelState.IsValid)
         {
            var currentTrainingProvider = await _db.TrainingProviders.FindAsync(editModel.Id);
            if (currentTrainingProvider == null)
            {
               return HttpNotFound();
            }

            if (editModel.LogoUpload != null && editModel.LogoUpload.ContentLength != 0)
            {
               try
               {
                  var logoSavePath = Path.Combine(Server.MapPath(AppConstants.VirtualPaths.TrainingProvidersContent), editModel.LogoUpload.FileName);
                  editModel.LogoUpload.SaveAs(logoSavePath);
               }
               catch (Exception ex)
               {
                  ModelState.AddModelError("", ex.Message);

                  return View(editModel);
               }
            }

            currentTrainingProvider.Name = editModel.Name;
            currentTrainingProvider.Description = editModel.Description;
            currentTrainingProvider.SiteUrl = editModel.SiteUrl;

            currentTrainingProvider.LogoFileName = editModel.LogoUpload != null
               ? editModel.LogoUpload.FileName
               : editModel.SelectedLogo.FileName;

            currentTrainingProvider.UpdateFrequencyHours = editModel.UpdateFrequencyHours;
            currentTrainingProvider.AllowedUpdateUtcHours = editModel.SelectedUpdateHours;
            currentTrainingProvider.SourceUrl = editModel.SourceUrl;
            currentTrainingProvider.SourceLocation = editModel.SourceLocation;
            currentTrainingProvider.AssemblyType = editModel.AssemblyType;
            currentTrainingProvider.IsDeleted = editModel.IsDeleted;

            try
            {
               await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
               ModelState.AddModelError("", ex.Message);

               return View(editModel);
            }


            return RedirectToAction("Index");
         }

         return View(editModel);
      }

      public async Task<ActionResult> Delete(int? id)
      {
         if (id == null)
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }

         var deleteModel = await _db.TrainingProviders
           .Where(x => x.Id == id)
           .Select(x => new TrainingProviderViewModels.DeleteViewModel { Id = x.Id, Name = x.Name })
           .SingleOrDefaultAsync();

         if (deleteModel == null)
         {
            return HttpNotFound();
         }

         return View(deleteModel);
      }

      [HttpPost, ActionName("Delete")]
      [ValidateAntiForgeryToken]
      public async Task<ActionResult> DeleteConfirmed(int? id)
      {
         if (id == null)
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }

         var deleteModel = await _db.TrainingProviders
            .Where(x => x.Id == id)
            .Select(x => new TrainingProviderViewModels.DeleteViewModel { Id = x.Id, Name = x.Name })
            .SingleOrDefaultAsync();

         if (deleteModel == null)
         {
            return HttpNotFound();
         }

         _db.Entry(new TrainingProvider { Id = deleteModel.Id }).State = EntityState.Deleted;

         try
         {
            await _db.SaveChangesAsync();
         }
         catch (Exception ex)
         {
            ModelState.AddModelError("", ex.Message);

            return View(deleteModel);
         }

         return RedirectToAction("Index");
      }



      protected override void Dispose(bool disposing)
      {
         if (disposing && _db != null)
         {
            _db.Dispose();
            _db = null;
         }
         base.Dispose(disposing);
      }
   }
}
