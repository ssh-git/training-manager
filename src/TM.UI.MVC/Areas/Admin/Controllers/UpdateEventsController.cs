using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using TM.Data.Update;
using TM.Shared;
using TM.UI.MVC.Areas.Admin.ViewModels;
using TM.UI.MVC.ControllableModel;
using TM.UI.MVC.Helpers;

namespace TM.UI.MVC.Areas.Admin.Controllers
{
   using VM = UpdateEventViewModels;

   [Authorize(Roles = AppConstants.UserRole.Administrator)]
   public class UpdateEventsController : Controller
   {
      private UpdateDbContext db = new UpdateDbContext();



      public async Task<ActionResult> Index(ControllableViewModelParams modelParams)
      {
         var updateEventsQuery = db.UpdateEvents.Select(x => new UpdateEventViewModels.IndexViewModel
         {
            Id = x.Id,
            TrainingProviderName = x.TrainingProvider.Name,
            Description = x.Description,
            StartedOn = x.StartedOn,
            EndedOn = x.EndedOn,
            UpdateResult = x.UpdateResult
         });

         var controllableViewModel = await UpdateEventViewModels.IndexViewModel.ToControlableViewModelAsync(updateEventsQuery, modelParams);

         return View(controllableViewModel);
      }


      public async Task<ActionResult> Details(int? id, ControllableViewModelParams modelParams)
      {
         if (id == null)
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }

         var updateEvent = await db.UpdateEvents.Where(x => x.Id == id).Select(x => new UpdateEventViewModels.DetailsViewModel
         {
            Id = x.Id,
            TrainingProviderName = x.TrainingProvider.Name,
            Description = x.Description,
            StartedOn = x.StartedOn,
            EndedOn = x.EndedOn,
            ErrorData = x.ErrorData,
            UpdateResult = x.UpdateResult,
            Added = new UpdateEventViewModels.StatisticViewModel
            {
               Categories = x.Added.Categories,
               Courses = x.Added.Courses,
               Authors = x.Added.Authors,

               HasCategoriesUpdateLog = x.CategoriesUpdates.Any(u => u.OperationType == OperationType.Add),
               HasCoursesUpdateLog = x.CoursesUpdates.Any(u => u.OperationType == OperationType.Add),
               HasAuthorsUpdateLog = x.AuthorsUpdates.Any(u => u.OperationType == OperationType.Add)
            },
            Deleted = new UpdateEventViewModels.StatisticViewModel
            {
               Categories = x.Deleted.Categories,
               Courses = x.Deleted.Courses,
               Authors = x.Deleted.Authors,

               HasCategoriesUpdateLog = x.CategoriesUpdates.Any(u => u.OperationType == OperationType.Delete),
               HasCoursesUpdateLog = x.CoursesUpdates.Any(u => u.OperationType == OperationType.Delete),
               HasAuthorsUpdateLog = x.AuthorsUpdates.Any(u => u.OperationType == OperationType.Delete)
            },
            Modified = new UpdateEventViewModels.StatisticViewModel
            {
               Categories = x.Modified.Categories,
               Courses = x.Modified.Courses,
               Authors = x.Modified.Authors,

               HasCategoriesUpdateLog = x.CategoriesUpdates.Any(u => u.OperationType == OperationType.Modify),
               HasCoursesUpdateLog = x.CoursesUpdates.Any(u => u.OperationType == OperationType.Modify && u.CourseBackup != null),
               HasAuthorsUpdateLog = x.AuthorsUpdates.Any(u => u.OperationType == OperationType.Modify)
            }
         }).SingleOrDefaultAsync();

         if (updateEvent == null)
         {
            return HttpNotFound();
         }

         ViewData.SetControllableViewModelParams(modelParams);

         return View(updateEvent);
      }



      public async Task<ActionResult> CategoriesUpdateDetails(int? id, OperationType? operation)
      {
         if (operation == null || operation == OperationType.None)
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }

         var categoriesQuery = operation == OperationType.Modify
                    ? db.CategoriesUpdates
                       .Where(x => x.UpdateEventId == id && x.OperationType == operation && x.CategoryBackup.UpdateEventId == x.UpdateEventId)
                    : db.CategoriesUpdates
                       .Where(x => x.UpdateEventId == id && x.OperationType == operation);

         var viewModel = await db.UpdateEvents
            .Where(x => x.Id == id)
            .Select(x => new UpdateEventViewModels.CategoryUpdateDetailsViewModel
            {
               UpdateEventId = x.Id,
               OperationType = operation.Value,
               TrainingProviderName = x.TrainingProvider.Name,
               Categories = categoriesQuery.Select(c => new UpdateEventViewModels.CategoryUpdateListViewModel
               {
                  CategoryId = c.CategoryId,
                  Title = c.Category.Title,
                  CategoryUrlName = c.Category.UrlName
               }).ToList()
            }).SingleOrDefaultAsync();

         if (viewModel == null)
         {
            return HttpNotFound();
         }

         return View(viewModel);
      }


      public async Task<ActionResult> CategoryChangesDetails(int? updateEventId, int? categoryId)
      {
         if (updateEventId == null || categoryId == null)
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }

         var viewModel = await db.CategoriesBackups
            .Where(x => x.UpdateEventId == updateEventId && x.CategoryId == categoryId)
            .Select(x => new UpdateEventViewModels.CategoryChangesDetailsViewModel
            {
               TrainingProviderName = x.CategoryUpdate.UpdateEvent.TrainingProvider.Name,

               PreviousContent = new UpdateEventViewModels.CategoryChangesViewModel
               {
                  Title = x.Title,
                  LogoUrl = x.LogoUrl,
                  LogoFileName = x.LogoFileName
               },
               CurrentContent = new UpdateEventViewModels.CategoryChangesViewModel
               {
                  Title = x.CategoryUpdate.Category.Title,
                  LogoUrl = x.CategoryUpdate.Category.LogoUrl,
                  LogoFileName = x.CategoryUpdate.Category.LogoFileName
               }
            }).SingleOrDefaultAsync();

         return View(viewModel);
      }




      public async Task<ActionResult> CoursesUpdateDetails(int? id, OperationType? operation)
      {
         if (operation == null || operation == OperationType.None)
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }

         var coursesQuery = operation == OperationType.Modify
            ? db.CoursesUpdates
               .Where(x => x.UpdateEventId == id && x.OperationType == operation && x.CourseBackup.UpdateEventId == x.UpdateEventId)
            : db.CoursesUpdates
               .Where(x => x.UpdateEventId == id && x.OperationType == operation);

         var viewModel = await db.UpdateEvents
            .Where(x => x.Id == id)
            .Select(x => new UpdateEventViewModels.CourseUpdateDetailsViewModel
            {
               UpdateEventId = x.Id,
               OperationType = operation.Value,
               TrainingProviderName = x.TrainingProvider.Name,
               Courses = coursesQuery.Select(c => new UpdateEventViewModels.CourseUpdateListViewModel
               {
                  CourseId = c.CourseId,
                  CategoryTitle = c.Course.Category.Title,
                  CategoryUrlName = c.Course.Category.UrlName,
                  CourseTitle = c.Course.Title,
                  CourseUrlName = c.Course.UrlName,
                  CourseReleaseDate = c.Course.ReleaseDate
               }).OrderByDescending(c => c.CourseReleaseDate).ToList()
            }).SingleOrDefaultAsync();

         if (viewModel == null)
         {
            return HttpNotFound();
         }

         return View(viewModel);
      }

      public async Task<ActionResult> CourseChangesDetails(int? updateEventId, int? courseId)
      {
         if (updateEventId == null || courseId == null)
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }

         var viewModel = await db.CoursesBackups
            .Where(x => x.UpdateEventId == updateEventId && x.CourseId == courseId)
            .Select(x => new UpdateEventViewModels.CourseChangesDetailsViewModel
            {
               HasAuthorsChanges = x.CourseAuthorBackups.Any(),
               TrainingProviderName = x.CourseUpdate.Course.TrainingProvider.Name,

               PreviousContent = new UpdateEventViewModels.CourseChangesViewModel
               {
                  CategoryTitle = x.Category.Title,
                  CategoryUrlName = x.Category.UrlName,
                  Title = x.Title,
                  SiteUrl = x.SiteUrl,
                  Description = x.Description,
                  ReleaseDate = x.ReleaseDate,
                  CourseLevel = (CourseLevel?) x.CourseLevel,
                  HasClosedCaptions = x.HasClosedCaptions,
                  Duration = x.Duration,
                  AuthorsChanges = x.CourseAuthorBackups
                     .Select(a => new UpdateEventViewModels.CourseAuthorsChangesViewModel
                     {
                        FullName = a.TrainingProviderAuthor.FullName,
                        AuthorUrlName = a.TrainingProviderAuthor.UrlName,
                        IsAuthorCoAuthor = a.IsAuthorCoAuthor,
                        OperationType = a.OperationType
                     }).ToList()
               },
               CurrentContent = new UpdateEventViewModels.CourseChangesViewModel
               {
                  CategoryTitle = x.CourseUpdate.Course.Category.Title,
                  CategoryUrlName = x.CourseUpdate.Course.Category.UrlName,
                  Title = x.CourseUpdate.Course.Title,
                  SiteUrl = x.CourseUpdate.Course.SiteUrl,
                  Description = x.CourseUpdate.Course.Description,
                  ReleaseDate = x.CourseUpdate.Course.ReleaseDate,
                  CourseLevel = x.CourseUpdate.Course.Level,
                  HasClosedCaptions = x.CourseUpdate.Course.HasClosedCaptions,
                  Duration = x.CourseUpdate.Course.Duration,
                  AuthorsChanges = db.CourseAuthors
                     .Where(ca => ca.CourseId == x.CourseId)
                     .Select(ca => new UpdateEventViewModels.CourseAuthorsChangesViewModel
                     {
                        FullName = ca.TrainingProviderAuthor.FullName,
                        AuthorUrlName = ca.TrainingProviderAuthor.UrlName,
                        IsAuthorCoAuthor = ca.IsAuthorCoAuthor,
                        OperationType = OperationType.None
                     }).ToList()
               }
            }).SingleOrDefaultAsync();

         return View(viewModel);
      }



      public async Task<ActionResult> AuthorsUpdateDetails(int? id, OperationType? operation)
      {
         if (operation == null || operation == OperationType.None)
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }

         var authorsQuery = operation == OperationType.Modify
            ? db.AuthorsUpdates
               .Where(x => x.UpdateEventId == id && x.OperationType == operation && x.AuthorBackup.UpdateEventId == x.UpdateEventId)
            : db.AuthorsUpdates
               .Where(x => x.UpdateEventId == id && x.OperationType == operation);

         var viewModel = await db.UpdateEvents
            .Where(x => x.Id == id)
            .Select(x => new UpdateEventViewModels.AuthorUpdateDetailsViewModel
            {
               UpdateEventId = x.Id,
               OperationType = operation.Value,
               TrainingProviderName = x.TrainingProvider.Name,
               Authors = authorsQuery.Select(a => new UpdateEventViewModels.AuthorUpdateListViewModel
               {
                 AuthorId = a.AuthorId,
                 FullName = a.TrainingProviderAuthor.FullName,
                 SiteUrl = a.TrainingProviderAuthor.SiteUrl,
                 AuthorUrlName = a.TrainingProviderAuthor.UrlName
               }).ToList()
            }).SingleOrDefaultAsync();

         if (viewModel == null)
         {
            return HttpNotFound();
         }

         return View(viewModel);
      }

      public async Task<ActionResult> AuthorChangesDetails(int? updateEventId, int? authorId)
      {
         if (updateEventId == null || authorId == null)
         {
            return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
         }

         var viewModel = await db.AuthorsBackups
            .Where(x => x.UpdateEventId == updateEventId && x.AuthorId == authorId)
            .Select(x => new UpdateEventViewModels.AuthorChangesDetailsViewModel
            {
               TrainingProviderName = x.AuthorUpdate.UpdateEvent.TrainingProvider.Name,

               PreviousContent = new UpdateEventViewModels.AuthorChangesViewModel
               {
                  FullName = x.FullName,
                  SiteUrl = x.SiteUrl
               },
               CurrentContent = new UpdateEventViewModels.AuthorChangesViewModel
               {
                  FullName = x.AuthorUpdate.TrainingProviderAuthor.FullName,
                  SiteUrl = x.AuthorUpdate.TrainingProviderAuthor.SiteUrl
               }
            }).SingleOrDefaultAsync();

         return View(viewModel);
      }


      protected override void Dispose(bool disposing)
      {
         if (disposing)
         {
            db.Dispose();
            db = null;
         }
         base.Dispose(disposing);
      }
   }
}
