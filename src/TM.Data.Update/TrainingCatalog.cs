using System;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TM.Data.Update.Properties;
using TM.Shared;
using TM.Shared.DownloadManager;
using TM.Shared.HtmlContainer;
using TM.Shared.Parse;

namespace TM.Data.Update
{
   public interface ITrainingCatalog : IDisposable
   {
      /// <exception cref="ArgumentNullException">
      /// <paramref name="trainingProviderName"/>  or
      /// <paramref name="host"/>  or
      /// <paramref name="mediaPath"/>  or
      /// <paramref name="updatesUriOrPath"/>
      /// is <see langword="null" />.</exception>
      /// 
      /// <exception cref="TrainingCatalogInitializationException"></exception>
      void Initialize(string trainingProviderName, int trainingProviderId, string host, string updatesUriOrPath,
         LocationType catalogLocation, IMediaPath mediaPath, string archiveDirectory);

      /// <exception cref="InvalidOperationException">Initialize(...) method invoke required before update</exception>
      /// <exception cref="TrainingCatalogUpdateException"></exception>
      /// <exception cref="ArgumentNullException">
      /// <paramref name="updateEvent"/> or
      /// <paramref name="context"/>
      /// is <see langword="null" />.</exception>
      Task UpdateAsync(UpdateEvent updateEvent, UpdateDbContext context, bool useArchiveData = false, bool logUpdateToDb = true);

      /// <exception cref="InvalidOperationException"><see cref="Initialize"/> method invoke required before update</exception>
      Task ReassignCourseSpecializationsAsync(UpdateDbContext context, bool useArchiveData = false);

      Task<Author> GetAuthorAsync(string authorUrlName, IHttpDownloadManager httpDownloadManager);
   }

   public abstract class TrainingCatalog<TCategoryParseModel, TCourseParseModel, TAuthorParseModel> : ITrainingCatalog
      where TCategoryParseModel : ICategoryParseModel
      where TCourseParseModel : ICourseParseModel
      where TAuthorParseModel : IAuthorParseModel
   {
      protected readonly IHttpDownloadManager HttpDownloadManager;
      private ITrainingCatalogMediaContentProcessor _mediaContentProcessor;
      protected readonly IFileSystemProxy FileSystemProxy;

      /// <exception cref="ArgumentNullException">
      /// <paramref name="httpDownloadManager"/> or
      /// <paramref name="fileSystemProxy"/>
      /// is <see langword="null" />.</exception>
      protected TrainingCatalog(IHttpDownloadManager httpDownloadManager, IFileSystemProxy fileSystemProxy)
      {
         if (httpDownloadManager == null)
            throw new ArgumentNullException("httpDownloadManager");

         if (fileSystemProxy == null)
            throw new ArgumentNullException("fileSystemProxy");

         HttpDownloadManager = httpDownloadManager;
         FileSystemProxy = fileSystemProxy;
      }

      public string ArchiveFolderPath { get; private set; }
      public string ArchiveCurrentSaveDirectory { get; private set; }
      public string TrainingProviderName { get; private set; }
      public int TrainingProviderId { get; private set; }
      public string Host { get; private set; }
      public IMediaPath MediaPath { get; private set; }
      public string UpdateContentUriOrPath { get; private set; }
      public LocationType UpdateContentLocation { get; private set; }

      protected bool IsInitialized { get; private set; }


      /// <exception cref="ArgumentNullException"><paramref name="mediaContentProcessor"/> is <see langword="null" />.</exception>
      /// <exception cref="InvalidOperationException">Method must be invoked only after Initialize(...) method</exception>
      public void SetMediaContentProcessor(ITrainingCatalogMediaContentProcessor mediaContentProcessor)
      {
         if (mediaContentProcessor == null)
            throw new ArgumentNullException("mediaContentProcessor");

         if (IsInitialized == false)
         {
            throw new InvalidOperationException("Method must be invoked only after Initialize(...) method");
         }

         _mediaContentProcessor = mediaContentProcessor;
      }


      /// <exception cref="ArgumentNullException">
      /// <paramref name="trainingProviderName"/>  or
      /// <paramref name="host"/>  or   
      /// <paramref name="updatesUriOrPath"/> or
      /// <paramref name="mediaPath"/> is <see langword="null" />.</exception> 
      /// <exception cref="TrainingCatalogInitializationException"></exception>
      /// <exception cref="DirectoryNotFoundException"><param name="archiveDirectory"></param> not found.</exception>
      public virtual void Initialize(string trainingProviderName, int trainingProviderId, string host,
         string updatesUriOrPath, LocationType catalogLocation, IMediaPath mediaPath, string archiveDirectory)
      {
         if (trainingProviderName == null)
            throw new ArgumentNullException("trainingProviderName");

         if (host == null)
            throw new ArgumentNullException("host");

         if (updatesUriOrPath == null)
            throw new ArgumentNullException("updatesUriOrPath");

         if (mediaPath == null)
            throw new ArgumentNullException("mediaPath");

         if (!FileSystemProxy.IsDirectoryExists(archiveDirectory))
            throw new DirectoryNotFoundException(
               string.Format(Resources.TrainingCatalogException_ArchiveDirectoryNotFound, archiveDirectory));
        

         TrainingProviderName = trainingProviderName;
         TrainingProviderId = trainingProviderId;
         Host = host;
         MediaPath = mediaPath;
         UpdateContentUriOrPath = updatesUriOrPath;
         UpdateContentLocation = catalogLocation;

         try
         {
            ArchiveFolderPath = Path.Combine(archiveDirectory, TrainingProviderName);

            var utcNow = DateTime.UtcNow;
            var currentSaveDirectory = Path.Combine(ArchiveFolderPath, utcNow.ToString("yyyy"), utcNow.ToString("yyyy.MM"), utcNow.ToString("yyyy.MM.dd"));
            ArchiveCurrentSaveDirectory = Path.Combine(ArchiveFolderPath, currentSaveDirectory);

            if (!FileSystemProxy.IsDirectoryExists(ArchiveCurrentSaveDirectory))
            {
               FileSystemProxy.CreateDirectory(ArchiveCurrentSaveDirectory);
            }

            _mediaContentProcessor = new TrainingCatalogMediaContentProcessor(TrainingProviderId, TrainingProviderName,
               MediaPath, HttpDownloadManager, FileSystemProxy);

            IsInitialized = true;
         }
         catch (Exception ex)
         {
            throw new TrainingCatalogInitializationException(Resources.TrainingCatalogInitializationException_Message, ex);
         }
      }


      /// <exception cref="InvalidOperationException"><see cref="Initialize"/> method invoke required before update</exception>
      /// <exception cref="TrainingCatalogUpdateException"></exception>
      /// <exception cref="ArgumentNullException">
      /// <paramref name="updateEvent"/> or
      /// <paramref name="context"/>
      /// is <see langword="null" />.</exception>
      public virtual async Task UpdateAsync(UpdateEvent updateEvent, UpdateDbContext context,
         bool useArchiveData = false, bool logUpdateToDb = true)
      {
         if (updateEvent == null)
            throw new ArgumentNullException("updateEvent");

         if (context == null)
            throw new ArgumentNullException("context");

         if (!IsInitialized)
            throw new InvalidOperationException("Initialize(...) method invoke required before update");

         try
         {
            var updateParseResult = await GetUpdateAsync();
            var changesProcessor = CreateChangesProcessor(updateParseResult, useArchiveData);

            var updateProcessor = CreateUpdateProcessor(context, logUpdateToDb);
            await updateProcessor.ProcessUpdateAsync(updateEvent, changesProcessor);

            await UpdateMediaContentAsync(context);
         }
         catch (Exception ex)
         {
            var message = string.Format(Resources.TrainingCatalogUpdateException_Message, TrainingProviderName);
            throw new TrainingCatalogUpdateException(message, ex);
         }
      }


      private Task UpdateMediaContentAsync(UpdateDbContext context)
      {
         return _mediaContentProcessor.UpdateMediaContentAsync(context);
      }


      /// <exception cref="ArgumentNullException"><paramref name="catalog"/> is <see langword="null" />.</exception>
      /// <exception cref="ArchiveSaveException"></exception>
      protected virtual void CreateUpdateContentArchive(IHtmlContainer catalog)
      {
         if (catalog == null)
            throw new ArgumentNullException("catalog");

         string savePath = null;
         try
         {
            savePath = Path.Combine(ArchiveCurrentSaveDirectory,
               TrainingProviderName + "_catalog" + " - " + DateTime.UtcNow.ToString("yyyy.MM.dd [HH-mm.ss]") + ".html");

            catalog.Save(savePath, Encoding.UTF8);
         }
         catch (Exception ex)
         {
            var message = string.Format(Resources.ArchiveSaveException_Message, savePath);
            throw new ArchiveSaveException(message, ex);
         }
      }


      #region Abstract Methods

      protected abstract Task<IUpdateContentParseResult<TCategoryParseModel, TCourseParseModel, TAuthorParseModel>>
         GetUpdateAsync();

      protected abstract TrainingCatalogChangesProcessor<TCategoryParseModel, TCourseParseModel, TAuthorParseModel>
         CreateChangesProcessor(
         IUpdateContentParseResult<TCategoryParseModel, TCourseParseModel, TAuthorParseModel> updateParseResult,
         bool useArchiveData);

      protected abstract TrainingCatalogUpdateProcessor<TCategoryParseModel, TCourseParseModel, TAuthorParseModel>
         CreateUpdateProcessor(UpdateDbContext context, bool logUpdateToDb);

      /// <exception cref="InvalidOperationException"><see cref="Initialize"/> method invoke required before update</exception>
      public abstract Task ReassignCourseSpecializationsAsync(UpdateDbContext context, bool useArchiveData = false);

      public abstract Task<Author> GetAuthorAsync(string authorUrlName, IHttpDownloadManager httpDownloadManager);
      

      #endregion


      #region IDisposable Implementation

      private bool _disposed;

      public void Dispose()
      {
         Dispose(true);
         GC.SuppressFinalize(this);
      }

      protected virtual void Dispose(bool disposing)
      {
         if (!_disposed && disposing)
         {
            if (HttpDownloadManager != null)
            {
               HttpDownloadManager.Dispose();
            }

            _disposed = true;
         }
      }

      #endregion
   }
}
