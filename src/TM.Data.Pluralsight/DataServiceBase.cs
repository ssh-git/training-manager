using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TM.Data.Pluralsight.Json;
using TM.Shared;

namespace TM.Data.Pluralsight
{
   internal interface IPluralsightDataService : IDisposable
   {
      Task<Author> GetAuthorAsync(string urlName);
      Task<string> GetCourseShortDescriptionAsync(string urlName);
      Task<ICollection<Module>> GetCourseToCAsync(string urlName);
      Task<Specializations> GetCourseSpecializationsAsync(string courseUrlName);
      Task<Dictionary<string, Specializations>> GetCourseSpecializationsContainerAsync();
   }

   internal abstract class DataServiceBase : IPluralsightDataService
   {
      protected const string TrainingProviderName = "pluralsight";

      protected const string AuthorsArchiveNamePrefix = "pluralsight_authors - ";
      protected const string CoursesArchiveNamePrefix = "pluralsight_courses - ";
      protected const string CoursesToCArchiveNamePrefix = "pluralsight_coursesToC - ";
      protected const string CourseSpecializationsArchiveNamePrefix = "pluralsight_specializations - ";

      protected const string DateTimeFormatPattern = "yyyy.MM.dd [HH-mm.ss]";
      protected const string ArchiveFileExtension = ".json";
      
      protected Dictionary<string, string> AuthorCoursesContainer;

      protected readonly IFileSystemProxy FileSystemProxy;

      protected DataServiceBase(IFileSystemProxy fileSystemProxy)
      {
         FileSystemProxy = fileSystemProxy;

         AuthorCoursesContainer = new Dictionary<string, string>();
      }

      #region Authors

      protected abstract Task<string> GetAuthorJsonDataAsync(string urlName);
      protected abstract void UpdateAuthorsArchive(string urlName, string jsonData);

      public async Task<Author> GetAuthorAsync(string urlName)
      {
         var author = new Author();

         var jsonData = await GetAuthorJsonDataAsync(urlName);

         UpdateAuthorCoursesDictionary(jsonData);

         UpdateAuthorsArchive(urlName, jsonData);

         var jsonObject = JsonConvert.DeserializeObject<JsonAuthorInfo>(jsonData);

         // Name
         author.FirstName = jsonObject.firstName.Trim();
         author.LastName = jsonObject.fullName
            .Replace(jsonObject.firstName, string.Empty).Trim();

         // Author.Bio
         if (!string.IsNullOrWhiteSpace(jsonObject.longBio))
         {
            author.Bio = jsonObject.longBio.Trim();
         }

         // Social.FacebookLink
         author.Social.FacebookLink = string.IsNullOrWhiteSpace(jsonObject.optionalFacebookUrl)
            ? null
            : jsonObject.optionalFacebookUrl;

         // Social.TwitterLink
         author.Social.TwitterLink = string.IsNullOrWhiteSpace(jsonObject.optionalTwitterUrl)
            ? null
            : jsonObject.optionalTwitterUrl;

         // Social.LinkedInLink
         author.Social.LinkedInLink = string.IsNullOrWhiteSpace(jsonObject.optionalLinkedInUrl)
            ? null
            : jsonObject.optionalLinkedInUrl;

         // Social.RssLink
         author.Social.RssLink = string.IsNullOrWhiteSpace(jsonObject.optionalRssUrl)
            ? null
            : jsonObject.optionalRssUrl;

         // Badge.ImageSiteUrl and Badge.ImageName
         if (!string.IsNullOrWhiteSpace(jsonObject.optionalBadgeImageUrl))
         {
            var badgeImageSiteUrl = new Uri(jsonObject.optionalBadgeImageUrl);
            author.Badge.ImageSiteUrl = badgeImageSiteUrl.Scheme == Uri.UriSchemeFile
               ? new UriBuilder(badgeImageSiteUrl) { Scheme = "http" }.ToString()
               : badgeImageSiteUrl.ToString();

            author.Badge.ImageName = badgeImageSiteUrl.Segments.Last().EndsWith("/", StringComparison.Ordinal)
               ? null
               : badgeImageSiteUrl.Segments.Last();
         }

         // Badge.Link
         if (!string.IsNullOrWhiteSpace(jsonObject.optionalBadgeLink))
         {
            var badgeLinkUri = new Uri(jsonObject.optionalBadgeLink);
            author.Badge.Link = badgeLinkUri.Scheme == Uri.UriSchemeFile
               ? new UriBuilder(badgeLinkUri) { Scheme = "http" }.ToString()
               : badgeLinkUri.ToString();
         }

         // Badge.HoverText
         author.Badge.HoverText = string.IsNullOrWhiteSpace(jsonObject.optionalBadgeHoverText)
            ? null
            : jsonObject.optionalBadgeHoverText;

         // Avatar.SiteUrl and Avatar.Name
         if (!string.IsNullOrWhiteSpace(jsonObject.largeImageUrl))
         {
            var avatarSiteUri = new Uri(jsonObject.largeImageUrl);
            author.Avatar.SiteUrl = avatarSiteUri.Scheme == Uri.UriSchemeFile
               ? new UriBuilder(avatarSiteUri) { Scheme = "http" }.ToString()
               : avatarSiteUri.ToString();

            author.Avatar.Name = avatarSiteUri.Segments.Last().EndsWith("/", StringComparison.Ordinal)
               ? null
               : avatarSiteUri.Segments.Last();
         }

         return author;
      }

      protected void UpdateAuthorCoursesDictionary(string jsonAuthor)
      {
         var jsonObject = JsonConvert.DeserializeObject<JsonAuthorCourses>(jsonAuthor);

         foreach (var course in jsonObject.courses)
         {
            var courseString = JsonConvert.SerializeObject(course, Formatting.Indented);
            AuthorCoursesContainer[course.name] = courseString;
         }
      }

      #endregion


      #region Courses

      protected abstract Task<string> GetCourseJsonDataAsync(string urlName);
      protected abstract void UpdateCourseArchive(string urlName, string jsonData);

      public async Task<string> GetCourseShortDescriptionAsync(string urlName)
      {
         string shortDescription;

         string authorCourseJsonString;
         if (AuthorCoursesContainer.TryGetValue(urlName, out authorCourseJsonString))
         {
            var jsonObject = JsonConvert.DeserializeObject<JsonAuthorCourse>(authorCourseJsonString);

            shortDescription = string.IsNullOrWhiteSpace(jsonObject.shortDescription)
               ? null
               : jsonObject.shortDescription.Trim();
         } else
         {
            var jsonData = await GetCourseJsonDataAsync(urlName);

            UpdateCourseArchive(urlName, jsonData);

            var jsonObject = JsonConvert.DeserializeObject<JsonCourseInfo>(jsonData);

            shortDescription = string.IsNullOrWhiteSpace(jsonObject.shortDescription)
               ? null
               : jsonObject.shortDescription.Trim();
         }

         return shortDescription;
      }

      #endregion


      #region Courses table of contents

      protected abstract Task<string> GetCourseToCJsonDataAsync(string urlName);
      protected abstract void UpdateCourseToCArchive(string urlName, string jsonData);

      public async Task<ICollection<Module>> GetCourseToCAsync(string urlName)
      {
         var jsonData = await GetCourseToCJsonDataAsync(urlName);

         UpdateCourseToCArchive(urlName, jsonData);

         var jsonObject = JsonConvert.DeserializeObject<List<JsonModule>>(jsonData);

         var modules = jsonObject.Select((chapter, capterIndex) => new Module
         {
            Description = chapter.description,
            Ordinal = Convert.ToByte(capterIndex + 1),
            Title = chapter.title,
            Duration = TimeSpan.Parse(chapter.duration),
            Topics = chapter.clips.Select((topic, topicIndex) => new Topic
            {
               Title = topic.title,
               Ordinal = Convert.ToByte(topicIndex + 1),
               Duration = TimeSpan.Parse(topic.duration)
            }).ToList()
         }).ToList();

         return modules;
      }

      #endregion

      #region Courses Specialization

      protected abstract Task<Dictionary<string, Specializations>> InitializeCourseSpecializationContainerAsync();

      public async Task<Dictionary<string, Specializations>> GetCourseSpecializationsContainerAsync()
      {
         var container = await InitializeCourseSpecializationContainerAsync();
         return container;
      }

      protected abstract Task<Dictionary<string, Specializations>> GetInstanceOfCourseSpecializationsContainerAsync();

      public async Task<Specializations> GetCourseSpecializationsAsync(string courseUrlName)
      {
         var specializationsContainer = await GetInstanceOfCourseSpecializationsContainerAsync();

         Specializations specializations;
         if (specializationsContainer.TryGetValue(courseUrlName, out specializations))
         {
            return specializations;
         }

         return Specializations.None;
      }

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
         AuthorCoursesContainer = null;
         _disposed = true;
      }

      #endregion
   }
}