using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using TM.Data.Pluralsight.Json;
using TM.Data.Pluralsight.Properties;
using TM.Shared;
using TM.Shared.DownloadManager;

namespace TM.Data.Pluralsight
{
   internal class PluralsightWebDataService : DataServiceBase
   {
      private readonly Uri _authorInfoUrl = new Uri("http://www.pluralsight.com/data/author/");
      private readonly Uri _courseInfoUrl = new Uri("http://www.pluralsight.com/data/course/");
      private readonly Uri _courseToCUrl = new Uri("http://www.pluralsight.com/data/course/content/");

      private readonly Dictionary<string, string> _authorsInfoContainer;
      private readonly Dictionary<string, string> _coursesInfoContainer;
      private readonly Dictionary<string, string> _coursesToCContainer;
      private readonly AsyncLazy<Dictionary<string, Specializations>> _courseSpecializationsContainer;

      private readonly string _archiveCurrentSaveDirectory;
      private readonly IHttpDownloadManager _httpDownloadManager;
      private readonly bool _createArchives;

      public static PluralsightWebDataService CreateForDataQuery(IHttpDownloadManager httpDownloadManager)
      {
         return new PluralsightWebDataService(string.Empty, httpDownloadManager, Shared.FileSystemProxy.Instance, createArchives:false);
      }


      /// <exception cref="ArgumentNullException"> 
      /// <paramref name="archiveCurrentSaveDirectory"/> or
      /// <paramref name="httpDownloadManager"/> or
      /// <paramref name="fileSystemProxy"/> is <see langword="null" />.</exception>
      public PluralsightWebDataService(string archiveCurrentSaveDirectory, IHttpDownloadManager httpDownloadManager,
         IFileSystemProxy fileSystemProxy, bool createArchives = true)
         : base(fileSystemProxy)
      {
         if (archiveCurrentSaveDirectory == null)
            throw new ArgumentNullException("archiveCurrentSaveDirectory");

         if (httpDownloadManager == null)
            throw new ArgumentNullException("httpDownloadManager");

         if (fileSystemProxy == null)
            throw new ArgumentNullException("fileSystemProxy");

         _archiveCurrentSaveDirectory = archiveCurrentSaveDirectory;
         _httpDownloadManager = httpDownloadManager;

         _authorsInfoContainer = new Dictionary<string, string>();
         _coursesInfoContainer = new Dictionary<string, string>();
         _coursesToCContainer = new Dictionary<string, string>();

         _courseSpecializationsContainer = new AsyncLazy<Dictionary<string, Specializations>>(async () =>
            await InitializeCourseSpecializationContainerAsync());

         _createArchives = createArchives;
      }


      #region DataServiceBase Overrides

      protected internal override async Task<string> GetAuthorJsonDataAsync(string urlName)
      {
         var requestUri = new Uri(_authorInfoUrl, urlName);

         var authorJsonData = await GetJsonDataAsync(requestUri);

         return authorJsonData;
      }

      protected override void UpdateAuthorsArchive(string urlName, string jsonData)
      {
         _authorsInfoContainer[urlName] = jsonData;
      }


      protected internal override async Task<string> GetCourseJsonDataAsync(string urlName)
      {
         var requestUri = new Uri(_courseInfoUrl, urlName);

         var courseJsonData = await GetJsonDataAsync(requestUri);

         return courseJsonData;
      }

      protected override void UpdateCourseArchive(string urlName, string jsonData)
      {
         _coursesInfoContainer[urlName] = jsonData;
      }


      protected internal override async Task<string> GetCourseToCJsonDataAsync(string urlName)
      {
         var requestUri = new Uri(_courseToCUrl, urlName);

         var courseToCJsonData = await GetJsonDataAsync(requestUri);

         return courseToCJsonData;
      }

      protected override void UpdateCourseToCArchive(string urlName, string jsonData)
      {
         _coursesToCContainer[urlName] = jsonData;
      }


      protected override async Task<Dictionary<string, Specializations>> InitializeCourseSpecializationContainerAsync()
      {
         var developerUrlNames = await GetCoursesUrlNamesForSpecializationAsync("developer");
         var itAdministratorUrlNames = await GetCoursesUrlNamesForSpecializationAsync("it-ops");
         var creativeProfessionalUrlNames = await GetCoursesUrlNamesForSpecializationAsync("creative");

         var specializationsContainer = developerUrlNames
            .ToDictionary(x => x, x => Specializations.SoftwareDeveloper);

         AddSpecialization(specializationsContainer, itAdministratorUrlNames, Specializations.ITAdministrator);
         AddSpecialization(specializationsContainer, creativeProfessionalUrlNames, Specializations.CreativeProfessional);

         return specializationsContainer;
      }


      protected override async Task<Dictionary<string, Specializations>> GetInstanceOfCourseSpecializationsContainerAsync()
      {
         return await _courseSpecializationsContainer;
      }

      private bool _disposed;
      protected override void Dispose(bool disposing)
      {
         if ( _createArchives && disposing && !_disposed)
         {
            try
            {
               CreateArchives();
            }
            catch
            {
            }
            _disposed = true;
         }
         base.Dispose(disposing);
      }

      #endregion


      #region Helpers

      /// <exception><see cref="DownloadException{T}"/> An error occurred while download.</exception>
      private async Task<string> GetJsonDataAsync(Uri requestUri)
      {
         var response = await _httpDownloadManager.DownloadAsStringAsync(requestUri, acceptMediaType: "application/json");

         if (!response.IsSuccess)
         {
            // ReSharper disable once ExceptionNotDocumented
            throw new DownloadException<string>(Resources.DownloadException_Message, response);
         }

         return response.Result;
      }

      private async Task<List<string>> GetCoursesUrlNamesForSpecializationAsync(string specializationId)
      {
         int totalCourses;
         var downloadedCourses = 0;
         const int coursesToDownload = 500;
         var page = 1;

         var coursesUrlNames = new List<string>();
         DownloadResult<string> response;
         do
         {
            var requestUri = new Uri(string.Format("http://www.pluralsight.com/data/courses/tags?id={0}&page={1}&pageSize={2}&sort=new&level=",
               specializationId, page, coursesToDownload));

            response = await _httpDownloadManager.DownloadAsStringAsync(requestUri, acceptMediaType: "application/json");

            if (response.IsSuccess)
            {
               var courseList = JsonConvert.DeserializeObject<JsonCourseList>(response.Result);

               totalCourses = courseList.totalNumberOfCoursesFound;
               downloadedCourses += courseList.courses.Length;

               coursesUrlNames.AddRange(courseList.courses.Select(x => x.name));
               page++;
            } else
            {
               return null;
            }

         } while (response.IsSuccess && downloadedCourses != totalCourses);

         return coursesUrlNames;
      }


      private void AddSpecialization(Dictionary<string, Specializations> specializationsContainer,
         List<string> coursesUrlNames, Specializations specializationToAdd)
      {
         foreach (var urlName in coursesUrlNames)
         {
            Specializations currentSpecialization;
            if (specializationsContainer.TryGetValue(urlName, out currentSpecialization))
            {
               specializationsContainer[urlName] = currentSpecialization | specializationToAdd;
            } else
            {
               specializationsContainer[urlName] = specializationToAdd;
            }
         }
      }


      internal void CreateArchives()
      {
         string archiveName;
         if (_authorsInfoContainer != null && _authorsInfoContainer.Any())
         {
            archiveName = AuthorsArchiveNamePrefix + DateTime.UtcNow.ToString(DateTimeFormatPattern) +
                          ArchiveFileExtension;
            CreateArchive(archiveName, _authorsInfoContainer);
         }

         if (_coursesInfoContainer != null && _coursesInfoContainer.Any())
         {
            archiveName = CoursesArchiveNamePrefix + DateTime.UtcNow.ToString(DateTimeFormatPattern) +
                          ArchiveFileExtension;
            CreateArchive(archiveName, _coursesInfoContainer);
         }

         if (_coursesToCContainer != null && _coursesToCContainer.Any())
         {
            archiveName = CoursesToCArchiveNamePrefix + DateTime.UtcNow.ToString(DateTimeFormatPattern) +
                          ArchiveFileExtension;
            CreateArchive(archiveName, _coursesToCContainer);
         }

         if (_courseSpecializationsContainer != null && _courseSpecializationsContainer.IsValueCreated)
         {
            archiveName = CourseSpecializationsArchiveNamePrefix + DateTime.UtcNow.ToString(DateTimeFormatPattern) +
                          ArchiveFileExtension;

            CreateArchive(archiveName, _courseSpecializationsContainer.Value.Result);
         }
      }

      internal void CreateArchive<TValue>(string archiveName, Dictionary<string, TValue> archive)
      {
         var archiveFile = new ArchiveFile<TValue>
         {
            Date = DateTime.UtcNow,
            TrainingProviderName = TrainingProviderName,
            Content = archive
         };

         var fileName = Path.Combine(_archiveCurrentSaveDirectory, archiveName);

         var enumConverter = new StringEnumConverter { AllowIntegerValues = false, CamelCaseText = false };

         FileSystemProxy.WriteTextToNewFile(fileName, JsonConvert.SerializeObject(archiveFile, Formatting.Indented, enumConverter));
      }

      #endregion
   }
}