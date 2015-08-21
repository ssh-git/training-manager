using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TM.Data.Pluralsight.Json;
using TM.Shared;

namespace TM.Data.Pluralsight
{
   internal class PluralsightArchiveDataService : DataServiceBase
   {
      private  AsyncLazy<Dictionary<string, string>> _authorsInfoArchive;
      private  AsyncLazy<Dictionary<string, string>> _coursesInfoArchive;
      private  AsyncLazy<Dictionary<string, string>> _coursesToCArchive;
      private  AsyncLazy<Dictionary<string, Specializations>> _courseSpecializationsArchive;

      private readonly string _archiveFolderPath;

      /// <exception cref="ArgumentNullException">
      /// <paramref name="archiveFolderPath"/> or
      /// <paramref name="fileSystemProxy"/> is <see langword="null" />.</exception>
      public PluralsightArchiveDataService(string archiveFolderPath, IFileSystemProxy fileSystemProxy)
         : base(fileSystemProxy)
      {
         if (archiveFolderPath == null)
            throw new ArgumentNullException("archiveFolderPath");

         if (fileSystemProxy == null)
            throw new ArgumentNullException("fileSystemProxy");

         _archiveFolderPath = archiveFolderPath;

         _authorsInfoArchive = new AsyncLazy<Dictionary<string, string>>(async () =>
            await MergeArchivesAsync<string>(AuthorsArchiveNamePrefix));

         _coursesInfoArchive = new AsyncLazy<Dictionary<string, string>>(async () =>
         {
            foreach (var entry in await _authorsInfoArchive)
            {
               UpdateAuthorCoursesDictionary(entry.Value);
            }

            return await MergeArchivesAsync<string>(CoursesArchiveNamePrefix);
         });

         _coursesToCArchive = new AsyncLazy<Dictionary<string, string>>(async () =>
            await MergeArchivesAsync<string>(CoursesToCArchiveNamePrefix));

         _courseSpecializationsArchive = new AsyncLazy<Dictionary<string, Specializations>>(async () =>
            await InitializeCourseSpecializationContainerAsync());
      }


      #region DataServiceBase Overrides

      /// <exception cref="KeyNotFoundException"><paramref name="urlName" /> does not exist in the authors archive.</exception>
      protected override async Task<string> GetAuthorJsonDataAsync(string urlName)
      {
         var archive = await _authorsInfoArchive;

         var jsonData = archive[urlName];

         return jsonData;
      }

      protected override void UpdateAuthorsArchive(string urlName, string jsonData)
      {
         // no need to update archive
      }


      /// <exception cref="KeyNotFoundException"><paramref name="urlName" /> does not exist in the courses archive.</exception>
      protected override async Task<string> GetCourseJsonDataAsync(string urlName)
      {
         var archive = await _coursesInfoArchive;

         var jsonData = archive[urlName];

         return jsonData;
      }

      protected override void UpdateCourseArchive(string urlName, string jsonData)
      {
         // no need to update archive
      }


      /// <exception cref="KeyNotFoundException"><paramref name="urlName" /> does not exist in the coursesToC archive.</exception>
      protected override async Task<string> GetCourseToCJsonDataAsync(string urlName)
      {
         var archive = await _coursesToCArchive;

         var jsonData = archive[urlName];

         return jsonData;
      }

      protected override void UpdateCourseToCArchive(string urlName, string jsonData)
      {
         // no need to update archive
      }

      protected override async Task<Dictionary<string, Specializations>> InitializeCourseSpecializationContainerAsync()
      {
         var courseSpecializationsArchive = await GetLatestArchiveAsync<Specializations>(CourseSpecializationsArchiveNamePrefix);
         return courseSpecializationsArchive;
      }

      protected override async Task<Dictionary<string, Specializations>> GetInstanceOfCourseSpecializationsContainerAsync()
      {
         return await _courseSpecializationsArchive;
      }

      protected override void Dispose(bool disposing)
      {
         if (disposing)
         {
            _authorsInfoArchive = null;
            _coursesInfoArchive = null;
            _coursesToCArchive = null;
            _courseSpecializationsArchive = null;
         }
         base.Dispose(disposing);
      }

      #endregion


      #region Helpers

      internal async Task<Dictionary<string, TValue>> GetArchiveContentAsync<TValue>(string archivePath)
      {
         var fileText = await FileSystemProxy.ReadTextFromFileAsync(archivePath);
         var archiveFile = JsonConvert.DeserializeObject<ArchiveFile<TValue>>(fileText);
         var archiveContent = archiveFile.Content;
         return archiveContent;
      }

      internal async Task<Dictionary<string, TValue>> GetLatestArchiveAsync<TValue>(string archiveNamePrefix)
      {
         var archiveFilesName = FileSystemProxy.EnumerateFiles(_archiveFolderPath, archiveNamePrefix + "*", SearchOption.AllDirectories)
            .OrderByDescending(x => ExtractDateTime(x, archiveNamePrefix))
            .First();

         var archiveContent = await GetArchiveContentAsync<TValue>(archiveFilesName);

         return archiveContent;
      }


      internal async Task<Dictionary<string, TValue>> MergeArchivesAsync<TValue>(string archiveNamePrefix)
      {
         var archiveFilesNames = FileSystemProxy.EnumerateFiles(_archiveFolderPath, archiveNamePrefix + "*", SearchOption.AllDirectories)
            .OrderBy(x => ExtractDateTime(x, archiveNamePrefix));

         var mergedArchivesContent = new Dictionary<string, TValue>();

         foreach (var filesName in archiveFilesNames)
         {
            var archiveContent = await GetArchiveContentAsync<TValue>(filesName);
            foreach (var item in archiveContent)
            {
               mergedArchivesContent[item.Key] = item.Value;
            }
         }

         return mergedArchivesContent;
      }

      internal DateTime ExtractDateTime(string fileName, string archiveNamePrefix)
      {
         var extractedDateTimeString =
            fileName.Split(new[] { archiveNamePrefix, ArchiveFileExtension }, StringSplitOptions.RemoveEmptyEntries).Last().Trim();

         var extractedDateTime = DateTime.ParseExact(extractedDateTimeString, DateTimeFormatPattern,
            DateTimeFormatInfo.InvariantInfo);

         return extractedDateTime;
      }

      #endregion
   }
}