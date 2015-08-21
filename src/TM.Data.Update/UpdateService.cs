using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TM.Data.Update.Properties;
using TM.Shared;
using TM.Shared.DownloadManager;
using TM.Shared.Parse;

namespace TM.Data.Update
{
   public class UpdateService
   {
      private static SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
      private const int WaitTimeout = 100;

      private readonly string _serverImagesPath;
      private readonly string _archiveDirectoryPath;

      private readonly IDbContextFactory<UpdateDbContext> _contextFactory;
      private readonly IDateTimeProxy _dateTimeProxy;
      private readonly IActivatorProxy _activatorProxy;

      /// <exception cref="ArgumentNullException"><paramref name="serverImagesPath"/> or
      /// <paramref name="archiveDirectoryPath"/> is <see langword="null" />.</exception>
      public UpdateService(string serverImagesPath, string archiveDirectoryPath)
         : this(serverImagesPath, archiveDirectoryPath, new DbContextFactory<UpdateDbContext>(), DateTimeProxy.Instance, ActivatorProxy.Instance)
      { }

      /// <exception cref="ArgumentNullException">
      /// <paramref name="serverImagesPath" /> or
      /// <paramref name="contextFactory" /> is <see langword="null" />.</exception>
      internal UpdateService(string serverImagesPath,string archiveDirectoryPath, IDbContextFactory<UpdateDbContext> contextFactory)
         : this(serverImagesPath, archiveDirectoryPath, contextFactory, DateTimeProxy.Instance, ActivatorProxy.Instance)
      {
      }

      /// <exception cref="ArgumentNullException">
      /// <paramref name="serverImagesPath"/> or
      /// <paramref name="archiveDirectoryPath"/> or
      /// <paramref name="contextFactory"/> or
      /// <paramref name="dateTimeProxy"/> or
      /// <paramref name="activatorProxy"/> is <see langword="null" />.</exception>
      internal UpdateService(string serverImagesPath, string archiveDirectoryPath, IDbContextFactory<UpdateDbContext> contextFactory,
         IDateTimeProxy dateTimeProxy, IActivatorProxy activatorProxy)
      {
         if (serverImagesPath == null)
            throw new ArgumentNullException("serverImagesPath");

         if (archiveDirectoryPath == null)
            throw new ArgumentNullException("archiveDirectoryPath");

         if (contextFactory == null)
            throw new ArgumentNullException("contextFactory");

         if (dateTimeProxy == null)
            throw new ArgumentNullException("dateTimeProxy");

         if (activatorProxy == null)
            throw new ArgumentNullException("activatorProxy");

         _serverImagesPath = serverImagesPath;
         _archiveDirectoryPath = archiveDirectoryPath;
         _contextFactory = contextFactory;
         _dateTimeProxy = dateTimeProxy;
         _activatorProxy = activatorProxy;
      }



      [SuppressMessage("ReSharper", "ExceptionNotDocumented")]
      public async Task<CallResult> UpdateTrainingCatalogsAsync()
      {
         if (!await _semaphore.WaitAsync(WaitTimeout))
         {
            return CallResult.Failed(Resources.UpdateService_CannotUpdateBecauseDatabaseIsInUpdateState);
         }

         try
         {
            List<TrainingProvider> trainingProviders;
            using (var context = _contextFactory.CreateDbContext())
            {
               trainingProviders = await context.TrainingProviders
                  .Where(x => !x.IsDeleted).ToListAsync();
            }

            var pathsContainer = new ServerMediaPathsContainer(_serverImagesPath, trainingProviders);

            foreach (var trainingProvider in trainingProviders)
            {
               using (var context = _contextFactory.CreateDbContext())
               {
                  var lastUpdateEventInfo = await GetLastUpdateEventInfoAsync(trainingProvider.Id, context);

                  if (
                     !CanProcessUpdate(trainingProvider.UpdateFrequency, trainingProvider.AllowedUpdateUtcHours,
                        lastUpdateEventInfo))
                     continue;

                  var description = Resources.UpdateEventDescription_ScheduledEvent;

                  await UpdateTrainingCatalogAsync(context, trainingProvider, description, pathsContainer);
               }
            }
         }
         finally
         {
            _semaphore.Release();
         }

         return CallResult.Success;
      }


      public async Task<CallResult> UpdateTrainingCatalogAsync(int trainingProviderId)
      {
         if (!await _semaphore.WaitAsync(WaitTimeout))
         {
            return CallResult.Failed(Resources.UpdateService_CannotUpdateBecauseDatabaseIsInUpdateState);
         }

         try
         {
            using (var context = _contextFactory.CreateDbContext())
            {
               var trainingProvider = await context.TrainingProviders.FindAsync(trainingProviderId);

               var pathsContainer = new ServerMediaPathsContainer(_serverImagesPath, new[] { trainingProvider });

               var description = Resources.UpdateEventDescription_ManualEvent;

               await UpdateTrainingCatalogAsync(context, trainingProvider, description, pathsContainer);
            }
         }
         finally
         {
            _semaphore.Release();
         }

         return CallResult.Success;
      }


      /// <exception cref="ArgumentNullException"><paramref name="trainingProvider"/> is <see langword="null" />.</exception>
      public async Task<CallResult> ReassignCourseSpecializationsAsync(TrainingProvider trainingProvider, bool useArchiveData = false)
      {
         if (trainingProvider == null)
            throw new ArgumentNullException("trainingProvider");

         if (!await _semaphore.WaitAsync(WaitTimeout))
         {
            return CallResult.Failed(Resources.UpdateService_CannotUpdateBecauseDatabaseIsInUpdateState);
         }

         try
         {
            var pathsContainer = new ServerMediaPathsContainer(_serverImagesPath, new[] { trainingProvider });

            using (var catalog = _activatorProxy.CreateInstance<ITrainingCatalog>(trainingProvider.AssemblyType))
            {
               // ReSharper disable once ExceptionNotDocumented
               catalog.Initialize(trainingProvider.Name, trainingProvider.Id, trainingProvider.SiteUrl,
                  trainingProvider.SourceUrl, trainingProvider.SourceLocation, pathsContainer, _archiveDirectoryPath);
               using (var context = _contextFactory.CreateDbContext())
               {
                  await catalog.ReassignCourseSpecializationsAsync(context, useArchiveData);
               }
            }
         }
         finally
         {
            _semaphore.Release();
         }

         return CallResult.Success;
      }


      /// <exception cref="UpdateServiceException">Author already in db.</exception>
      public async Task<CallResult<Author>> CreateAuthorAsync(int trainingProviderId, string authorUrlName, IHttpDownloadManager httpDownloadManager)
      {
         if (authorUrlName == null) throw new ArgumentNullException("authorUrlName");
         if (httpDownloadManager == null) throw new ArgumentNullException("httpDownloadManager");

         if (!await _semaphore.WaitAsync(WaitTimeout))
         {
            return CallResult<Author>.Failed(Resources.UpdateService_CannotUpdateBecauseDatabaseIsInUpdateState);
         }


         Author author;

         try
         {
            using (var context = _contextFactory.CreateDbContext())
            {
               var trainingProvider = await context.TrainingProviders.FindAsync(trainingProviderId);

               var pathsContainer = new ServerMediaPathsContainer(_serverImagesPath, new[] { trainingProvider });


               using (var catalog = _activatorProxy.CreateInstance<ITrainingCatalog>(trainingProvider.AssemblyType))
               {
                  // ReSharper disable once ExceptionNotDocumented
                  catalog.Initialize(trainingProvider.Name, trainingProvider.Id, trainingProvider.SiteUrl,
                     trainingProvider.SourceUrl, trainingProvider.SourceLocation, pathsContainer, _archiveDirectoryPath);

                  author = await catalog.GetAuthorAsync(authorUrlName, httpDownloadManager);
               }

               var sameAuthorsIds = await context.Authors
                  .Where(x => x.LastName == author.LastName &&
                              x.FirstName == author.FirstName &&
                              x.Social.FacebookLink == author.Social.FacebookLink &&
                              x.Social.LinkedInLink == author.Social.LinkedInLink &&
                              x.Social.RssLink == author.Social.RssLink &&
                              x.Social.TwitterLink == author.Social.TwitterLink)
                  .Select(x => x.Id).ToListAsync();

               if (sameAuthorsIds.Any())
               {
                  var message = string.Format(Resources.UpdateServiceException_AuthorAlreadyInDb_Message, author.FullName,
                     string.Join(";", sameAuthorsIds));

                  return CallResult<Author>.Failed(message);
               }

               // save avatar to media folder
               var pathToSave = Path.Combine(pathsContainer.AuthorsLogoPath[trainingProviderId], author.Avatar.Name);

               if (!FileSystemProxy.Instance.IsFileExists(pathToSave))
               {
                  // ensure http scheme
                  var avatarUriBuilder = new UriBuilder(new Uri(author.Avatar.SiteUrl))
                  {
                     Scheme = "http"
                  };

                  var avatarResponse = await httpDownloadManager.DownloadFileAsync(avatarUriBuilder.Uri);

                  if (avatarResponse.IsSuccess)
                  {
                     await FileSystemProxy.Instance.WriteToFileAsync(pathToSave, avatarResponse.Result);
                  }
               }

               context.Authors.Add(author);

               await context.SaveChangesAsync();
            }
         }
         finally
         {
            _semaphore.Release();
         }

         return CallResult<Author>.Success(author);
      }



      #region Helper Methods

      internal bool CanProcessUpdate(TimeSpan updateFrequency, List<int> allowedUpdateUtcHours, UpdateEventInfo lastUpdateEventInfo)
      {
         var utcNow = _dateTimeProxy.UtcNow;
         var currentTimeHours = utcNow.TimeOfDay.Hours;

         if (!allowedUpdateUtcHours.Contains(currentTimeHours))
         {
            return false;
         }

         if (lastUpdateEventInfo != null)
         {
            var nextUpdateDate = lastUpdateEventInfo.StartedOn + updateFrequency;
            if (utcNow < nextUpdateDate)
            {
               return false;
            }

            if (lastUpdateEventInfo.UpdateResult == UpdateResult.NeedManualResolve)
            {
               return false;
            }
         }

         return true;
      }

      internal async Task<UpdateEventInfo> GetLastUpdateEventInfoAsync(int trainingProviderId, UpdateDbContext context)
      {
         var lastUpdateEventInfo = await context.UpdateEvents
              .Where(x => x.TrainingProviderId == trainingProviderId)
              .OrderByDescending(x => x.StartedOn)
              .Select(x => new UpdateEventInfo { StartedOn = x.StartedOn, UpdateResult = x.UpdateResult })
              .FirstOrDefaultAsync();

         return lastUpdateEventInfo;
      }


      [SuppressMessage("ReSharper", "ExceptionNotDocumented")]
      internal async Task UpdateTrainingCatalogAsync(UpdateDbContext context, TrainingProvider trainingProvider,
         string description, ServerMediaPathsContainer pathsContainer, bool useArchiveData = false, bool logUpdateToDb = true)
      {
         var updateEvent = new UpdateEvent(trainingProvider.Id, description, _dateTimeProxy.UtcNow);

         context.UpdateEvents.Add(updateEvent);

         await context.SaveChangesAsync();

         using (var catalog = _activatorProxy.CreateInstance<ITrainingCatalog>(trainingProvider.AssemblyType))
         {
            catalog.Initialize(trainingProvider.Name, trainingProvider.Id, trainingProvider.SiteUrl,
               trainingProvider.SourceUrl, trainingProvider.SourceLocation, pathsContainer,_archiveDirectoryPath);
            try
            {
               await catalog.UpdateAsync(updateEvent, context, useArchiveData, logUpdateToDb);

               updateEvent.EndedOn = _dateTimeProxy.UtcNow;
               if (context.AuthorsResolves.Local.Any())
               {
                  updateEvent.UpdateResult = UpdateResult.NeedManualResolve;
               } else
               {
                  updateEvent.UpdateResult = UpdateResult.Success;
               }

               await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
               updateEvent.EndedOn = _dateTimeProxy.UtcNow;
               updateEvent.UpdateResult = UpdateResult.Error;

               var aggregateException = ex as AggregateException;
               var errorData = aggregateException != null
                  ? aggregateException.Flatten().ToString()
                  : ex.ToString();

               updateEvent.ErrorData = errorData;

               updateEvent.AuthorsUpdates = null;
               updateEvent.CategoriesUpdates = null;
               updateEvent.CoursesUpdates = null;

               context.SetStateToDetached(updateEvent);

               using (var ctx = _contextFactory.CreateDbContext())
               {
                  ctx.SetStateToModified(updateEvent);
                  ctx.SaveChanges();
               }
            }
         }
      }

      #endregion


      #region Nested Types

      internal class ServerMediaPathsContainer : IMediaPath
      {
         public IDictionary<int, string> CategoriesLogoPath { get; private set; }
         public IDictionary<int, string> AuthorsLogoPath { get; private set; }
         public IDictionary<int, string> BadgesPath { get; private set; }

         internal ServerMediaPathsContainer() { }

         public ServerMediaPathsContainer(string serverImagesPath, IEnumerable<TrainingProvider> trainingProviders)
         {
            CategoriesLogoPath = new Dictionary<int, string>();
            AuthorsLogoPath = new Dictionary<int, string>();
            BadgesPath = new Dictionary<int, string>();

            foreach (var trainingProvider in trainingProviders)
            {
               var categoryPath =
                  Path.Combine(serverImagesPath, "category", trainingProvider.Name.ToLowerInvariant());
               CategoriesLogoPath.Add(trainingProvider.Id, categoryPath);

               var authorLogoPath =
                  Path.Combine(serverImagesPath, "authors");
               AuthorsLogoPath.Add(trainingProvider.Id, authorLogoPath);

               var badgesPath =
                  Path.Combine(serverImagesPath, "badges");
               BadgesPath.Add(trainingProvider.Id, badgesPath);
            }
         }
      }

      internal class UpdateEventInfo
      {
         public DateTime StartedOn { get; set; }
         public UpdateResult UpdateResult { get; set; }
      }

      #endregion
   }
}
