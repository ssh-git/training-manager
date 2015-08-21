using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TM.Data.Update.Properties;
using TM.Shared;
using TM.Shared.DownloadManager;

namespace TM.Data.Update
{
   public interface ITrainingCatalogMediaContentProcessor
   {
      /// <exception cref="ArgumentNullException"><paramref name="context"/> is <see langword="null" />.</exception>
      /// <exception cref="MediaContentUpdateException"></exception>
      Task UpdateMediaContentAsync(UpdateDbContext context);
   }

   [SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
   internal class TrainingCatalogMediaContentProcessor : ITrainingCatalogMediaContentProcessor
   {
      private readonly int _trainingProviderId;
      private readonly string _trainingProviderName;
      private readonly IMediaPath _mediaPath;
      private readonly IHttpDownloadManager _httpDownloadManager;
      private readonly IFileSystemProxy _fileSystemProxy;

      /// <exception cref="ArgumentNullException">
      /// <paramref name="trainingProviderName"/> or
      /// <paramref name="mediaPath" /> or
      /// <paramref name="httpDownloadManager" />
      /// is <see langword="null" />.</exception>
      public TrainingCatalogMediaContentProcessor(int trainingProviderId,string trainingProviderName, IMediaPath mediaPath,
         IHttpDownloadManager httpDownloadManager)
         : this(trainingProviderId,trainingProviderName, mediaPath, httpDownloadManager, FileSystemProxy.Instance)
      {
      }

      /// <exception cref="ArgumentNullException">
      /// <paramref name="trainingProviderName"/> or
      /// <paramref name="mediaPath"/> or
      /// <paramref name="httpDownloadManager"/> or
      /// <paramref name="fileSystemProxy"/> 
      /// is <see langword="null" />.</exception>
      internal TrainingCatalogMediaContentProcessor(int trainingProviderId, string trainingProviderName,
         IMediaPath mediaPath,
         IHttpDownloadManager httpDownloadManager, IFileSystemProxy fileSystemProxy)
      {
         if (trainingProviderName == null)
            throw new ArgumentNullException("trainingProviderName");

         if (mediaPath == null)
            throw new ArgumentNullException("mediaPath");

         if (mediaPath == null)
            throw new ArgumentNullException("httpDownloadManager");

         if (mediaPath == null)
            throw new ArgumentNullException("fileSystemProxy");

         _trainingProviderName = trainingProviderName;
         _trainingProviderId = trainingProviderId;
         _mediaPath = mediaPath;
         _httpDownloadManager = httpDownloadManager;
         _fileSystemProxy = fileSystemProxy;
      }


      /// <exception cref="ArgumentNullException"><paramref name="context"/> is <see langword="null" />.</exception>
      /// <exception cref="MediaContentUpdateException"></exception>
      public async Task UpdateMediaContentAsync(UpdateDbContext context)
      {
         if(context == null) 
            throw new ArgumentNullException("context");
         try
         {
            var categoryMediaContentDirectory = _mediaPath.CategoriesLogoPath[_trainingProviderId];

            var dbCategoriesLogo = await context.Categories
               .Where(x => x.LogoFileName != null)
               .Select(x => new MediaContentInfo {Url = x.LogoUrl, FileName = x.LogoFileName})
               .ToListAsync();

            var fsCategoriesLogo = _fileSystemProxy.EnumerateFiles(categoryMediaContentDirectory)
               .Select(Path.GetFileName);

            await UpdateMediaContentAsync(dbCategoriesLogo, fsCategoriesLogo, categoryMediaContentDirectory);


            var authorsMediaContentDirectory = _mediaPath.AuthorsLogoPath[_trainingProviderId];

            var dbAuthorsLogo = await context.Authors
               .Where(x => x.Avatar.Name != null && x.Avatar.Name != "")
               .Select(x => new MediaContentInfo {Url = x.Avatar.SiteUrl, FileName = x.Avatar.Name})
               .ToListAsync();

            var fsAuthorsLogo = _fileSystemProxy.EnumerateFiles(authorsMediaContentDirectory)
               .Select(Path.GetFileName);

            await UpdateMediaContentAsync(dbAuthorsLogo, fsAuthorsLogo, authorsMediaContentDirectory);


            var badgesMediaContentDirectory = _mediaPath.BadgesPath[_trainingProviderId];

            var dbBadgesLogo = await context.Authors
               .Where(x => x.Badge.ImageName != null && x.Badge.ImageName != "")
               .GroupBy(x => new {x.Badge.ImageName, x.Badge.ImageSiteUrl}, x => "")
               .Select(x => new MediaContentInfo {Url = x.Key.ImageSiteUrl, FileName = x.Key.ImageName})
               .ToListAsync();

            var fsBadgesLogo = _fileSystemProxy.EnumerateFiles(badgesMediaContentDirectory)
               .Select(Path.GetFileName);

            await UpdateMediaContentAsync(dbBadgesLogo, fsBadgesLogo, badgesMediaContentDirectory);
         }
         catch(Exception ex)
         {
            var message = string.Format(Resources.MediaContentUpdateException_Message, _trainingProviderName);
            throw new MediaContentUpdateException(message, ex);
         }
      }
      
      protected virtual async Task UpdateMediaContentAsync(IEnumerable<MediaContentInfo> dbRecords, IEnumerable<string> fsRecords,
         string directoryPath)
      {
         var existingContentLookup = new HashSet<string>(fsRecords);

         foreach (var mediaContent in dbRecords)
         {
            if (!existingContentLookup.Contains(mediaContent.FileName) && !string.IsNullOrWhiteSpace(mediaContent.FileName))
            {
               // ensure http scheme
               var uriBuilder = new UriBuilder(new Uri(mediaContent.Url))
               {
                  Scheme = "http"
               };

               var response = await _httpDownloadManager.DownloadFileAsync(uriBuilder.Uri);

               if (response.IsSuccess)
               {
                  var pathToSave = Path.Combine(directoryPath, mediaContent.FileName);

                  await _fileSystemProxy.WriteToNewFileAsync(pathToSave, response.Result);
               }
            }
         }
      }


      internal class MediaContentInfo
      {
         public string FileName { get; set; }
         public string Url { get; set; }
      }
   }
}