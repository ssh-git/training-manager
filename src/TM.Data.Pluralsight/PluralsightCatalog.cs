using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using TM.Data.Parse;
using TM.Data.Pluralsight.Properties;
using TM.Data.Update;
using TM.Shared;
using TM.Shared.DownloadManager;
using TM.Shared.HtmlContainer;
using TM.Shared.Parse;

namespace TM.Data.Pluralsight
{
   [SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
   public class PluralsightCatalog : TrainingCatalog<PluralsightCategory, PluralsightCourse, PluralsightAuthor>
   {
      private readonly IHtmlLoader _htmlLoader;
      private readonly ICatalogBackupProcessor<PluralsightCategory, PluralsightCourse, PluralsightAuthor> _backupProcessor;
      private ITrainingCatalogParser<PluralsightCategory, PluralsightCourse, PluralsightAuthor> _parser;
      private IPluralsightDataService _dataService;


      #region Constructors

      public PluralsightCatalog()
         : base(new HttpDownloadManager(), Shared.FileSystemProxy.Instance)
      {
         _htmlLoader = new HtmlLoader<HtmlAgilityPackHtmlContainer>();
         _backupProcessor = new PluralsightBackupProcessor();
      }

      /// <exception cref="ArgumentNullException">
      /// <paramref name="htmlLoader" /> or
      /// <paramref name="parser" /> or
      /// <paramref name="backupProcessor" /> or
      /// <paramref name="httpDownloadManager" /> or
      /// <paramref name="dataService" /> or
      /// <paramref name="fileSystemProxy" /> is <see langword="null" />.</exception>
      internal PluralsightCatalog(IHtmlLoader htmlLoader,
         ITrainingCatalogParser<PluralsightCategory, PluralsightCourse, PluralsightAuthor> parser,
         ICatalogBackupProcessor<PluralsightCategory, PluralsightCourse, PluralsightAuthor> backupProcessor,
         IHttpDownloadManager httpDownloadManager,
         IPluralsightDataService dataService,
         IFileSystemProxy fileSystemProxy)
         : base(httpDownloadManager, fileSystemProxy)
      {
         if (htmlLoader == null)
            throw new ArgumentNullException("htmlLoader");

         if (parser == null)
            throw new ArgumentNullException("parser");

         if (backupProcessor == null)
            throw new ArgumentNullException("backupProcessor");

         if (dataService == null)
            throw new ArgumentNullException("dataService");

         _htmlLoader = htmlLoader;
         _parser = parser;
         _backupProcessor = backupProcessor;
         _dataService = dataService;
      }

      #endregion


      #region TrainingCatalog Overrides

      /// <exception cref="ArgumentNullException">
      /// <paramref name="trainingProviderName"/>  or
      /// <paramref name="host"/>  or   
      /// <paramref name="updatesUriOrPath"/> or
      /// <paramref name="mediaPath"/> is <see langword="null" />.</exception> 
      /// <exception cref="TrainingCatalogInitializationException"></exception>
      public override void Initialize(string trainingProviderName, int trainingProviderId, string host,
         string updatesUriOrPath, LocationType catalogLocation, IMediaPath mediaPath, string archiveDirectory)
      {
         base.Initialize(trainingProviderName, trainingProviderId, host, updatesUriOrPath, catalogLocation, mediaPath, archiveDirectory);

         if (_parser == null)
         {
            var nodeSelector = new PluralsightNodeSelector();
            var nodeParser = new PluralsightNodeParser(Host, nodeSelector);
            _parser = new PluralsightCatalogParser(nodeSelector, nodeParser);
         }
      }


      [SuppressMessage("ReSharper", "ExceptionNotDocumented")]
      protected override async Task<IUpdateContentParseResult<PluralsightCategory, PluralsightCourse, PluralsightAuthor>>
         GetUpdateAsync()
      {
         var updateContent = await _htmlLoader.LoadAsync(UpdateContentUriOrPath, UpdateContentLocation);

         if (UpdateContentLocation == LocationType.Remote)
         {
            CreateUpdateContentArchive(updateContent);
         }

         var parseResult = _parser.Parse(updateContent);

         return parseResult;
      }


      protected override TrainingCatalogUpdateProcessor<PluralsightCategory, PluralsightCourse, PluralsightAuthor>
         CreateUpdateProcessor(UpdateDbContext context, bool logUpdateToDb)
      {
         var updateProcessor = new PluralsightUpdateProcessor(TrainingProviderId, context, _backupProcessor, logUpdateToDb);

         return updateProcessor;
      }


      /// <exception cref="InvalidOperationException"><see cref="Initialize"/> method invoke required.</exception>
      /// <exception cref="ArgumentNullException"><paramref name="context"/> is <see langword="null" />.</exception>
      [SuppressMessage("ReSharper", "ExceptionNotDocumented")]
      public override async Task ReassignCourseSpecializationsAsync(UpdateDbContext context, bool useArchiveData = false)
      {
         if (!IsInitialized)
            throw new InvalidOperationException(Resources.InvalidOperation_PluralsightCatalog_InitializeMethodInvokeRequired);

         if (context == null)
            throw new ArgumentNullException("context");

         var allCourses = await context.Courses
            .Where(x => x.TrainingProviderId == TrainingProviderId && !x.IsDeleted)
            .Select(x => new
            {
               x.Id,
               x.UrlName,
               CourseSpecializations = x.CourseSpecializations.ToList()
            })
            .AsNoTracking()
            .ToListAsync();

         var courseSpecializationsContainer = await GetCourseSpecializationsContainerAsync(useArchiveData);

         try
         {
            context.Configuration.AutoDetectChangesEnabled = false;

            foreach (var dbCourse in allCourses)
            {
               Specializations specializationsForCourse;
               if (courseSpecializationsContainer.TryGetValue(dbCourse.UrlName, out specializationsForCourse))
               {
                  var currentSpecializations = specializationsForCourse.GetFlags<Specializations>().ToList();

                  foreach (var dbCourseSpecialization in dbCourse.CourseSpecializations)
                  {
                     if (currentSpecializations.Contains(dbCourseSpecialization.Specialization))
                     {
                        currentSpecializations.Remove(dbCourseSpecialization.Specialization);
                     }
                     else
                     {
                        context.SetStateToDeleted(dbCourseSpecialization);
                     }
                  }
                  foreach (var specialization in currentSpecializations)
                  {
                     var courseSpecialization = new CourseSpecialization
                     {
                        CourseId = dbCourse.Id,
                        Specialization = specialization
                     };
                     context.SetStateToAdded(courseSpecialization);
                  }
               }
            }
         }
         finally
         {
            context.Configuration.AutoDetectChangesEnabled = true;
         }

         await context.SaveChangesAsync();
      }

      protected override TrainingCatalogChangesProcessor<PluralsightCategory, PluralsightCourse, PluralsightAuthor>
        CreateChangesProcessor(IUpdateContentParseResult<PluralsightCategory, PluralsightCourse, PluralsightAuthor> updateParseResult,
        bool useArchiveData)
      {
         if (_dataService == null)
         {
            _dataService = useArchiveData
               ? new PluralsightArchiveDataService(ArchiveFolderPath, FileSystemProxy)
               : (IPluralsightDataService)
                  new PluralsightWebDataService(ArchiveCurrentSaveDirectory, HttpDownloadManager, FileSystemProxy);
         }

         var changesProcessor = new PluralsightChangesProcessor(updateParseResult, _dataService);

         return changesProcessor;
      }

      public override async Task<Author> GetAuthorAsync(string authorUrlName, IHttpDownloadManager httpDownloadManager)
      {
         using (var dataService = PluralsightWebDataService.CreateForDataQuery(HttpDownloadManager))
         {
            var author = await dataService.GetAuthorAsync(authorUrlName);
            return author;
         }
      }


      private bool _disposed;

      protected override void Dispose(bool disposing)
      {
         if (!_disposed && disposing)
         {
            if (_dataService != null)
            {
               _dataService.Dispose();
            }

            _disposed = true;
         }

         base.Dispose(disposing);
      }

      #endregion


      #region Helpers

      internal virtual async Task<Dictionary<string, Specializations>> GetCourseSpecializationsContainerAsync(bool useArchiveData)
      {
         var dataService = useArchiveData
            ? new PluralsightArchiveDataService(ArchiveFolderPath, FileSystemProxy)
            : (IPluralsightDataService)new PluralsightWebDataService(ArchiveCurrentSaveDirectory, HttpDownloadManager, FileSystemProxy);

         using (dataService)
         {
            var courseSpecializationsContainer = await dataService.GetCourseSpecializationsContainerAsync();

            return courseSpecializationsContainer;
         }
      }

      #endregion
   }
}
