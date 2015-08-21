using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using TM.Data;
using TM.Data.Update;
using TM.Shared.DownloadManager;
using TM.UI.MVC.Areas.Admin.ViewModels;

namespace TM.UI.MVC.Areas.Admin.Controllers
{
   using VM = AuthorResolvesViewModels;

   [Authorize(Roles = AppConstants.UserRole.Administrator)]
   public class AuthorResolvesController : Controller
   {
      private UpdateDbContext _db = new UpdateDbContext();


      public async Task<ActionResult> Index(int? updateEventId)
      {
         var query = updateEventId.HasValue
            ? _db.AuthorsResolves.Where(x => x.UpdateEventId == updateEventId)
            : _db.AuthorsResolves;

         var viewModel = await query.Select(x => new AuthorResolvesViewModels.IndexViewModel
            {
               Id = x.Id,
               UpdateEventId = x.UpdateEventId,
               UpdateDate = x.UpdateEvent.StartedOn,
               TrainingProviderName = x.TrainingProvider.Name,

               CourseTitle = x.Course.Title,
               CourseUrlName = x.Course.UrlName,
               CategoryUrlName = x.Course.Category.UrlName,

               ProblemType = x.ProblemType,
               ResolveState = x.ResolveState
            })
            .OrderByDescending(x => x.UpdateDate)
            .ThenBy(x => x.ProblemType).ToListAsync();

         if (viewModel == null)
         {
            return HttpNotFound();
         }

         return View(viewModel);
      }


      public async Task<ActionResult> UrlNullDetails(int? id)
      {
         if (id == null)
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }

         var viewModel = await _db.AuthorsResolves.Where(x => x.Id == id.Value)
            .Select(x => new AuthorResolvesViewModels.UrlNullDetailsViewModel
            {
               Id = x.Id,
               UpdateDate = x.UpdateEvent.StartedOn,
               TrainingProviderName = x.TrainingProvider.Name,

               CourseTitle = x.Course.Title,
               CourseUrlName = x.Course.UrlName,
               CategoryUrlName = x.Course.Category.UrlName,

               ProviderCourseUrl = x.Course.SiteUrl,
               AuthorFullName = x.AuthorFullName,
               AuthorUrlName = x.AuthorUrlName,
               IsCoAuthor = x.IsAuthorCoAuthor,
               ResolvedUrl = x.AuthorSiteUrl,
               ProblemType = x.ProblemType,
               ResolveState = x.ResolveState,
            }).SingleOrDefaultAsync();

         if (viewModel == null)
         {
            return HttpNotFound();
         }

         return View(viewModel);
      }

      public async Task<ActionResult> UrlNullResolve(int? id)
      {
         if (id == null)
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }

         var viewModel = await GetUrlNullResolveViewModel(id.Value);

         if (viewModel == null)
         {
            return HttpNotFound();
         }

         return View(viewModel);
      }


      [HttpPost, ValidateAntiForgeryToken]
      public async Task<ActionResult> UrlNullResolve(int? id, AuthorResolvesViewModels.ResolvedNullUrlModel resolveModel)
      {
         if (id == null)
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }

         if (ModelState.IsValid)
         {
            do
            {
               var currentAuthorResolve = await _db.AuthorsResolves.FindAsync(id);

               if (resolveModel.SelectedAuthorId.HasValue)
               {
                  var authorUrlName = Author.GetUrlName(resolveModel.ResolvedUrl);
                  await ApplayAuthorResolve(currentAuthorResolve, resolveModel.SelectedAuthorId.Value, resolveModel.ResolvedUrl, authorUrlName);

               } else
               {
                  try
                  {
                     var authorUrlName = Author.GetUrlName(resolveModel.ResolvedUrl);
                     Author author = null;

                     var updateService = new UpdateService(AppConstants.ServerPaths.MediaDirectory, AppConstants.ServerPaths.ArchiveDirectory);
                     using (var downloadManger = new HttpDownloadManager(100, 100, 3, new InCloakWebAnonymizer()))
                     {
                        var callResult =
                           await
                              updateService.CreateAuthorAsync(currentAuthorResolve.TrainingProviderId, authorUrlName,
                                 downloadManger);
                        if (callResult.Succeeded)
                        {
                           author = callResult.Value;
                        }
                        else
                        {
                           ModelState.AddModelError("", string.Join(";\r\n", callResult.Errors));
                           break;
                        }
                     }

                     await ApplayAuthorResolve(currentAuthorResolve, author.Id, resolveModel.ResolvedUrl, authorUrlName);
                  }
                  // ReSharper disable once CatchAllClause
                  catch (Exception ex)
                  {
                     ModelState.AddModelError("", ex.ToString());
                     break;
                  }
               }

               return RedirectToAction("Index");
               // need for flow control
#pragma warning disable 162
            } while (false);
#pragma warning restore 162
         }

         var viewModel = await GetUrlNullResolveViewModel(id.Value);

         viewModel.SelectedAuthorId = resolveModel.SelectedAuthorId;
         viewModel.ResolvedUrl = resolveModel.ResolvedUrl;

         return View(viewModel);
      }


      private async Task ApplayAuthorResolve(AuthorResolve currentAuthorResolve, int resolvedId, string resolvedUrl, string resolvedUrlName)
      {
         currentAuthorResolve.ResolveState = ResolveState.Resolved;
         currentAuthorResolve.ResolvedAuthorId = resolvedId;
         currentAuthorResolve.AuthorSiteUrl = resolvedUrl;
         currentAuthorResolve.AuthorUrlName = resolvedUrlName;

         var trainingProviderAuthor = await _db.TrainingProviderAuthors
            .SingleOrDefaultAsync(x => x.TrainingProviderId == currentAuthorResolve.TrainingProviderId && x.AuthorId == resolvedId);
         if (trainingProviderAuthor == null)
         {
            trainingProviderAuthor = new TrainingProviderAuthor
            {
               TrainingProviderId = currentAuthorResolve.TrainingProviderId,
               AuthorId = resolvedId,
               FullName = currentAuthorResolve.AuthorFullName,
               SiteUrl = currentAuthorResolve.AuthorSiteUrl,
               UrlName = currentAuthorResolve.AuthorUrlName
            };

            _db.TrainingProviderAuthors.Add(trainingProviderAuthor);
         } else if (trainingProviderAuthor.IsDeleted)
         {
            trainingProviderAuthor.IsDeleted = false;
         }

         var courseAuthor = await _db.CourseAuthors
            .SingleOrDefaultAsync(x => x.CourseId == currentAuthorResolve.CourseId && x.AuthorId == resolvedId);
         if (courseAuthor == null)
         {
            courseAuthor = new CourseAuthor
            {
               TrainingProviderId = currentAuthorResolve.TrainingProviderId,
               AuthorId = resolvedId,
               CourseId = currentAuthorResolve.CourseId,
               IsAuthorCoAuthor = currentAuthorResolve.IsAuthorCoAuthor
            };

            _db.CourseAuthors.Add(courseAuthor);
         } else if (courseAuthor.IsDeleted)
         {
            courseAuthor.IsDeleted = false;
         }

         await _db.SaveChangesAsync();

         var unresolvedAuthorCount = await _db.AuthorsResolves
            .CountAsync(x => x.UpdateEventId == currentAuthorResolve.UpdateEventId && x.ResolveState == ResolveState.Pending);

         if (unresolvedAuthorCount == 0)
         {
            var currentUpdateEvent = await _db.UpdateEvents
               .SingleOrDefaultAsync(x => x.Id == currentAuthorResolve.UpdateEventId && x.UpdateResult == UpdateResult.NeedManualResolve);

            if (currentUpdateEvent != null)
            {
               currentUpdateEvent.UpdateResult = UpdateResult.Resolved;
               await _db.SaveChangesAsync();
            }
         }
      }


      private async Task<AuthorResolvesViewModels.UrlNullResolveViewModel> GetUrlNullResolveViewModel(int id)
      {
         var viewModel = await _db.AuthorsResolves.Where(x => x.Id == id)
            .Select(x => new AuthorResolvesViewModels.UrlNullResolveViewModel
            {
               Id = x.Id,
               UpdateDate = x.UpdateEvent.StartedOn,
               TrainingProviderName = x.TrainingProvider.Name,

               CourseTitle = x.Course.Title,
               CourseUrlName = x.Course.UrlName,
               CategoryUrlName = x.Course.Category.UrlName,

               ProviderCourseUrl = x.Course.SiteUrl,
               AuthorFullName = x.AuthorFullName,
               AuthorUrlName = x.AuthorUrlName,
               IsCoAuthor = x.IsAuthorCoAuthor,
               ResolvedUrl = x.AuthorSiteUrl,
               ProblemType = x.ProblemType,
               ResolveState = x.ResolveState
            }).SingleOrDefaultAsync();

         if (viewModel == null) return null;

         var authorFullName = viewModel.AuthorFullName;
         var authorFirstName = Author.GetFirstName(authorFullName);
         var authorLastName = Author.GetLastName(authorFullName);

         var possibleAuthors = await _db.Authors
            .Where(x => x.LastName == authorLastName && x.FirstName == authorFirstName)
            .Select(x => new AuthorResolvesViewModels.PossibleAuthorViewModel
            {
               AuthorId = x.Id,
               SocialLinks = x.Social,
               FullName = authorFullName,
               SiteUrls = x.AuthorTrainingProviders.Select(tpa => tpa.SiteUrl).ToList()
            }).ToListAsync();

         if (possibleAuthors.Any())
         {
            viewModel.PossibleAuthors = possibleAuthors;
         }

         return viewModel;
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
