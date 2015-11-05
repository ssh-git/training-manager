using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Xml;
using TM.Data.Update;
using TM.UI.MVC.Helpers;
using TM.UI.MVC.Infrastructure;

namespace TM.UI.MVC.Models
{
   public class SyndicationFeedModels
   {
      public class UpdateFeedModel
      {
         public DateTime LastUpdateDateTime { get; set; }

         public DateTimeOffset LastUpdateUtcDateTimeOffset
         {
            get { return new DateTimeOffset(DateTime.SpecifyKind(LastUpdateDateTime, DateTimeKind.Utc)); }
         }

         public ICollection<UpdateFeedItemModel> Courses { get; set; }
      }

      public class UpdateFeedItemModel
      {
         public string CourseTitle { get; set; }

         public string CourseDescription { get; set; }

         public ICollection<FeedPersonModel> Authors { get; set; }
         public ICollection<FeedPersonModel> CoAuthors { get; set; }

         public string TrainingProviderName { get; set; }
         public string CategoryTitle { get; set; }
         public string CategoryUrlName { get; set; }
         public string CourseUrlName { get; set; }

         public DateTime PublishedDateTime { get; set; }

         public DateTimeOffset PublishedUtcDateOffset
         {
            get { return new DateTimeOffset(DateTime.SpecifyKind(PublishedDateTime.Date, DateTimeKind.Utc)); }
         }


         public Uri GetCourseUri(UrlHelper urlHelper, string protocol)
         {
            var url = urlHelper.RouteUrl(AppConstants.RouteNames.Course,
               new
               {
                  trainingProviderName = TrainingProviderName,
                  categoryUrlName = CategoryUrlName,
                  courseUrlName = CourseUrlName
               },
               protocol);

            return new Uri(url);
         }

         public string GetCategoryUriString(UrlHelper urlHelper, string protocol)
         {
            var url = urlHelper.RouteUrl(AppConstants.RouteNames.TrainingProviderCategories,
               new
               {
                  trainingProviderName = TrainingProviderName
               },
               protocol);

            return url;
         }

         public string GetPersonUriString(string personUrlName, UrlHelper urlHelper, string protocol)
         {
            var url = urlHelper.RouteUrl(AppConstants.RouteNames.Author,
               new
               {
                  trainingProviderName = TrainingProviderName,
                  authorUrlName = personUrlName
               },
               protocol);

            return url;
         }
      }

      public class FeedPersonModel
      {
         public string Title { get; set; }
         public string UrlName { get; set; }
      }

      public class SyndicationFeedsManager : CatalogManagerBase
      {
         public SyndicationFeedsManager()
         {
         }

         public SyndicationFeedsManager(UpdateDbContext context)
            : base(context)
         {
         }

         public int GetLastSuccessUpdateIdWithNewCourses()
         {
            var updateId = UpdateContext.UpdateEvents
               .Where(x => (x.UpdateResult == UpdateResult.Success || x.UpdateResult == UpdateResult.Resolved) && x.Added.Courses > 0)
               .OrderByDescending(x => x.Id)
               .Select(x => x.Id)
               .FirstOrDefault();

            return updateId;
         }

         public Task<int> GetLastSuccessUpdateIdWithNewCoursesAsync()
         {
            var updateId = UpdateContext.UpdateEvents
               .Where(x => (x.UpdateResult == UpdateResult.Success || x.UpdateResult == UpdateResult.Resolved) && x.Added.Courses > 0)
               .OrderByDescending(x => x.Id)
               .Select(x => x.Id)
               .FirstOrDefaultAsync();

            return updateId;
         }

         public Task<string> CreateUpdateFeedStringAsync(int updateId, Uri currentUri, UrlHelper urlHelper)
         {
            return CreateUpdateFeedStringAsync(updateId, currentUri, urlHelper, new XmlWriterSettings());
         }

         public async Task<string> CreateUpdateFeedStringAsync(int updateId, Uri currentUri, UrlHelper urlHelper, XmlWriterSettings xmlWriterSettings)
         {
            var updateFeedModel = await GetUpdateFeedModelAsync(updateId);

            var updateFeed = CreateUpdateFeed(updateFeedModel, currentUri, urlHelper);

            var updateFeedString = GetUpdateFeedString(updateFeed, xmlWriterSettings);

            return updateFeedString;
         }

         internal async Task<UpdateFeedModel> GetUpdateFeedModelAsync(int updateId)
         {
            var updateFeedModel = await UpdateContext.UpdateEvents
               .Where(x => x.Id == updateId)
               .Select(x => new UpdateFeedModel
               {
                  LastUpdateDateTime = x.StartedOn,
                  Courses = x.CoursesUpdates
                     .Where(c => c.OperationType == OperationType.Add)
                     .Select(c => new UpdateFeedItemModel
                     {
                        CourseTitle = c.Course.Title,
                        CourseDescription = c.Course.Description,
                        PublishedDateTime = c.Course.ReleaseDate,
                        CourseUrlName = c.Course.UrlName,

                        TrainingProviderName = c.Course.TrainingProvider.Name,

                        CategoryTitle = c.Course.Category.Title,
                        CategoryUrlName = c.Course.Category.UrlName,

                        Authors = c.Course.CourseAuthors
                           .Where(author => !author.IsAuthorCoAuthor)
                           .Select(author => new FeedPersonModel
                           {
                              Title = author.TrainingProviderAuthor.FullName,
                              UrlName = author.TrainingProviderAuthor.UrlName
                           }).ToList(),
                        CoAuthors = c.Course.CourseAuthors
                           .Where(author => author.IsAuthorCoAuthor)
                           .Select(author => new FeedPersonModel
                           {
                              Title = author.TrainingProviderAuthor.FullName,
                              UrlName = author.TrainingProviderAuthor.UrlName
                           }).ToList()
                     }).ToList()
               }).FirstOrDefaultAsync();

            return updateFeedModel;
         }

         internal SyndicationFeed CreateUpdateFeed(UpdateFeedModel feedModel, Uri currentUri, UrlHelper urlHelper)
         {
            var updateFeed = new SyndicationFeed
            {
               Id = string.Format("tag:{0},{1:yyyy-MM-dd}:feed/utc-time/{1:HH:mm:ss}", currentUri.Host, feedModel.LastUpdateUtcDateTimeOffset),
               Language = "en",
               Title = SyndicationContent.CreatePlaintextContent("IT Training Manager"),
               Description = SyndicationContent.CreatePlaintextContent("New Courses"),
               LastUpdatedTime = feedModel.LastUpdateUtcDateTimeOffset,
               Links =
               {
                  SyndicationLink.CreateSelfLink(currentUri, "application/atom+xml"),
                  SyndicationLink.CreateAlternateLink(new UriBuilder{Scheme = currentUri.Scheme,Host = currentUri.Host,Port = currentUri.Port}.Uri,
                  "text/html")
               },
               Items = feedModel.Courses.Select(item =>
               {
                  var syndicationItem = new SyndicationItem
                  {
                     Id = item.GetCourseUri(urlHelper, currentUri.Scheme).ToString(),
                     Title = SyndicationContent.CreatePlaintextContent(item.CourseTitle),
                     Content = SyndicationContent.CreateHtmlContent(item.CourseDescription),
                     Links =
                     {
                        SyndicationLink.CreateAlternateLink(item.GetCourseUri(urlHelper, currentUri.Scheme), "text/html")
                     },
                     PublishDate = item.PublishedUtcDateOffset,
                     LastUpdatedTime = feedModel.LastUpdateUtcDateTimeOffset,
                     Categories =
                     {
                        new SyndicationCategory
                        {
                           Label = "/" + item.CategoryTitle,
                           Name = item.CategoryTitle,
                           Scheme = item.GetCategoryUriString(urlHelper, currentUri.Scheme)
                        }
                     }
                  };

                  foreach (var person in item.Authors)
                  {
                     syndicationItem.Authors.Add(new SyndicationPerson
                     {
                        Name = person.Title,
                        Uri = item.GetPersonUriString(person.UrlName, urlHelper, currentUri.Scheme)
                     });
                  }

                  foreach (var person in item.CoAuthors)
                  {
                     syndicationItem.Contributors.Add(new SyndicationPerson
                     {
                        Name = person.Title,
                        Uri = item.GetPersonUriString(person.UrlName, urlHelper, currentUri.Scheme)
                     });
                  }

                  return syndicationItem;
               })
            };

            return updateFeed;
         }

         internal string GetUpdateFeedString(SyndicationFeed feed, XmlWriterSettings xmlWriterSettings)
         {
            using (var stringWriter = new StringWriterWithEncoding(Encoding.UTF8))
            {
               using (var writer = XmlWriter.Create(stringWriter, xmlWriterSettings))
               {
                  feed.SaveAsAtom10(writer);
               }

               var feedString = stringWriter.ToString();

               return feedString;
            }
         }
      }
   }
}