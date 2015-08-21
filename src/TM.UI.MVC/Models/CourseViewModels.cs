using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using TM.Data;
using TM.Data.Update;
using TM.Shared;
using TM.UI.MVC.Helpers;
using TM.UI.MVC.Infrastructure;
using TM.UI.MVC.Properties;

namespace TM.UI.MVC.Models
{
   public class CourseViewModel
   {
      public int Id { get; set; }

      public string SiteUrl { get; set; }
      public string UrlName { get; set; }

      [Display(Name = "Subtitles")]
      [UIHint("YesNo")]
      public bool HasClosedCaptions { get; set; }

      [Display(Name = "Course")]
      public string Title { get; set; }

      public CourseLevel Level { get; set; }


      [UIHint("CourseDuration")]
      public TimeSpan Duration { get; set; }

      [Display(Name = "Published")]
      [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
      public DateTime ReleaseDate { get; set; }

      public CourseRating Rating { get; set; }

      [DataType(DataType.Text)]
      public LearningState LearningState { get; set; }

      public IList<CourseAuthorViewModel> Authors { get; set; }
   }

   public class CourseCatalogEntryViewModel
   {
      public TrainingProviderViewModel TrainingProvider { get; set; }

      public CategoryViewModel Category { get; set; }

      public CourseViewModel Course { get; set; }

      public object CourseRouteValueObject
      {
         get
         {
            return
               new
               {
                  trainingProviderName = TrainingProvider.Name,
                  categoryUrlName = Category.UrlName,
                  courseUrlName = Course.UrlName
               };
         }
      }
      public object CategoryRouteValueObject
      {
         get
         {
            return
               new
               {
                  trainingProviderName = TrainingProvider.Name,
                  categoryUrlName = Category.UrlName
               };
         }
      }
   }


   public class CourseViewModels
   {
      public enum CourseListFor
      {
         Category = 1,
         Author
      }

      public class CourseLinkViewModel
      {
         public string TrainingProviderName { get; set; }
         public string CategoryUrlName { get; set; }
         public string CourseUrlName { get; set; }
         public string CourseTitle { get; set; }

         public object CourseRouteValueObject
         {
            get
            {
               return
                  new
                  {
                     trainingProviderName = TrainingProviderName,
                     categoryUrlName = CategoryUrlName,
                     courseUrlName = CourseUrlName
                  };
            }
         }
      }


      public class CourseCatalogViewModel
      {
         public NavigationViewModel Navigation { get; set; }

         public CourseCatalogEntryViewModel Metadata = new CourseCatalogEntryViewModel();

         public CourseListFor CourseListFor { get; set; }

         public IEnumerable<CourseCatalogEntryViewModel> Courses { get; set; }
      }

      public class CourseSearchViewModel
      {
         public string SearchTerm { get; set; }

         public CourseCatalogEntryViewModel Metadata = new CourseCatalogEntryViewModel();
      }


      public class CourseInfoViewModel : CourseViewModel
      {
         public string Description { get; set; }
         public string ShortDescription { get; set; }

         public TrainingProviderViewModel TrainingProvider { get; set; }
         public CategoryViewModel Category { get; set; }

         public List<AuthorViewModel> AuthorsInfo { get; set; }

         [UIHint("CourseToC")]
         public List<ModuleViewModel> Modules { get; set; }
      }


      [SuppressMessage("ReSharper", "InconsistentNaming")]
      public class CourseCatalogSearchRequest
      {
         public int draw { get; set; }
         public int start { get; set; }
         public int length { get; set; }
         public string trainingProviderName { get; set; }
         public string searchTerm { get; set; }

         public List<Dictionary<string, string>> order { get; set; }
      }


      [JsonConverter(typeof(CourseCatalogJsonConverter))]
      [SuppressMessage("ReSharper", "InconsistentNaming")]
      public class CourseCatalogSearchResult
      {
         public int draw { get; set; }
         public int recordsTotal { get; set; }
         public int recordsFiltered { get; set; }

         public IList<CourseCatalogEntryViewModel> catalog { get; set; }
      }


      public class CourseCatalogJsonConverter : JsonConverter
      {
         public override bool CanRead { get { return false; } }

         public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
         {
            var searchResult = (CourseCatalogSearchResult)value;
            var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
            writer.WriteStartObject();

            writer.WritePropertyName("draw");
            writer.WriteValue(searchResult.draw);

            writer.WritePropertyName("recordsTotal");
            writer.WriteValue(searchResult.recordsTotal);

            writer.WritePropertyName("recordsFiltered");
            writer.WriteValue(searchResult.recordsFiltered);

            writer.WritePropertyName("catalog");
            writer.WriteStartArray();
            foreach (var entry in searchResult.catalog)
            {
               writer.WriteStartObject();

               writer.WritePropertyName("trainingProvider");
               var trainingProviderHtmlString =
                  string.Format("<a href='{0}'><img width='75' src='{1}' alt='{2}' /></a>",
                     urlHelper.RouteUrl(AppConstants.RouteNames.TrainingProvider, entry.TrainingProvider.TrainingProviderRouteValuesObject),
                     entry.TrainingProvider.LogoUrl,
                     entry.TrainingProvider.Name);
               writer.WriteValue(trainingProviderHtmlString);

               writer.WritePropertyName("category");
               var categoryHtmlString =
                  string.Format("<a href='{0}'>{1}</a>",
                     urlHelper.RouteUrl(AppConstants.RouteNames.TrainingProviderCategory, entry.Category.CategoryRouteValuesObject),
                     entry.Category.Title);
               writer.WriteValue(categoryHtmlString);

               writer.WritePropertyName("courseTitle");
               var courseTitleHtmlString =
                  string.Format("<span>{0}{1}<a href='{2}'>{3}</a>{4}</span>",
                     HtmlHelperExtensions.GetCourseSubscriptionMarker(entry.Course.LearningState).ToString(),
                     "&nbsp;",
                     urlHelper.RouteUrl(AppConstants.RouteNames.Course, entry.CourseRouteValueObject),
                     entry.Course.Title,
                     entry.Course.HasClosedCaptions ? "&nbsp;<i class='text-info fa fa-cc'></i>" : null);
               writer.WriteValue(courseTitleHtmlString);

               writer.WritePropertyName("authors");
               var authorsHtmlString = HtmlHelperExtensions.GetCourseAuthors(entry.TrainingProvider.Name,
                  entry.Course.Authors);
               writer.WriteValue(authorsHtmlString.ToString());

               writer.WritePropertyName("level");
               writer.WriteValue(entry.Course.Level.ToString());

               writer.WritePropertyName("duration");
               var durationHtmlString = string.Format("<span>{0}<small>h</small>&nbsp;{1}<small>m</small></span>",
                  entry.Course.Duration.Hours, entry.Course.Duration.Minutes);
               writer.WriteValue(durationHtmlString);

               writer.WritePropertyName("releaseDate");
               var releaseDateString = entry.Course.ReleaseDate.ToString("yyyy-MM-dd");
               writer.WriteValue(releaseDateString);

               writer.WritePropertyName("rating");
               var ratingHtmlString = HtmlHelperExtensions.GetCourseRating(entry.Course.Rating);
               writer.WriteValue(ratingHtmlString.ToString());

               writer.WriteEndObject();
            }
            writer.WriteEndArray();

            writer.WriteEndObject();

            writer.Flush();
         }

         /// <exception cref="NotSupportedException"></exception>
         public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
         {
            throw new NotSupportedException();
         }

         public override bool CanConvert(Type objectType)
         {
            return objectType == typeof(CourseCatalogSearchResult);
         }
      }


      public class ModuleViewModel : TopicViewModel
      {
         public string Description { get; set; }
         public List<TopicViewModel> Topics { get; set; }
      }

      public class TopicViewModel
      {
         public int Ordinal { get; set; }

         public string Title { get; set; }

         public long DurationTicks { get; set; }

         [DataType(DataType.Duration)]
         public TimeSpan Duration
         {
            get { return TimeSpan.FromTicks(DurationTicks); }
            set { DurationTicks = value.Ticks; }
         }
      }


      [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
      public class CoursesManager : CatalogManagerBase
      {
         public CoursesManager()
         {
         }

         public CoursesManager(UpdateDbContext context)
            : base(context)
         {
         }


         public async Task<IEnumerable<CourseLinkViewModel>> GetTop5NewCoursesAsync(Specializations? specializations)
         {
            var courseQuery = GetCoursesQueryForSpecializations(specializations);

            var top5NewCourses = await courseQuery
               .OrderByDescending(x => x.ReleaseDate)
               .Take(5)
               .Select(x => new CourseLinkViewModel
               {
                  CourseTitle = x.Title,
                  CourseUrlName = x.UrlName,
                  TrainingProviderName = x.TrainingProvider.Name,
                  CategoryUrlName = x.Category.UrlName
               }).ToListAsync();

            return top5NewCourses;
         }

         public async Task<IEnumerable<CourseLinkViewModel>> GetTop5MonthPopularCoursesAsync(Specializations? specializations)
         {
            var courseQuery = GetCoursesQueryForSpecializations(specializations);
            var utcDateNow = DateTime.UtcNow.Date;

            var top5MonthPopularCourses = await courseQuery
               .Where(x => DbFunctions.DiffDays(x.ReleaseDate, utcDateNow) <= 30)
               .OrderByDescending(x => x.Rating.Raters)
               .Take(5)
               .Select(x => new CourseLinkViewModel
               {
                  CourseTitle = x.Title,
                  CourseUrlName = x.UrlName,
                  TrainingProviderName = x.TrainingProvider.Name,
                  CategoryUrlName = x.Category.UrlName
               }).ToListAsync();

            return top5MonthPopularCourses;
         }


         public async Task<CourseCatalogViewModel> GetCoursesForCategoryAsync(string trainingProviderName, string categoryUrlName,
            string userId, Specializations? specializations)
         {
            var courseQuery = GetCoursesQueryForSpecializations(specializations);

            courseQuery = courseQuery.Where(x => x.Category.UrlName == categoryUrlName &&
                                                 !x.Category.IsDeleted &&
                                                 x.TrainingProvider.Name == trainingProviderName);

            var courses = await ProjectToCourseCatalogEntry(courseQuery, userId).ToListAsync();

            if (courses.Any())
            {
               var courseList = new CourseCatalogViewModel
               {
                  CourseListFor = CourseListFor.Category,
                  Courses = courses
               };

               return courseList;
            }

            return null;
         }


         public async Task<CourseCatalogViewModel> GetCoursesForAuthorAsync(string authorUrlName, string trainingProviderName, string userId,
            Specializations? specializations)
         {
            var authorIdentity = await CatalogContext.TrainingProviderAuthors
               .Where(x => x.UrlName == authorUrlName &&
                           x.TrainingProvider.Name == trainingProviderName &&
                           !x.IsDeleted)
               .Select(x => new { x.AuthorId, x.TrainingProviderId })
               .SingleOrDefaultAsync();

            if (authorIdentity == null)
            {
               return null;
            }

            var courseQuery = GetCoursesQueryForSpecializations(specializations);

            courseQuery = courseQuery.Where(x => x.CourseAuthors.Any(ca => ca.AuthorId == authorIdentity.AuthorId) &&
            x.TrainingProviderId == authorIdentity.TrainingProviderId);

            var courses = await ProjectToCourseCatalogEntry(courseQuery, userId).ToListAsync();

            if (courses.Any())
            {
               var courseList = new CourseCatalogViewModel
               {
                  CourseListFor = CourseListFor.Author,
                  Courses = courses
               };

               return courseList;
            }

            return null;
         }



         public async Task<CourseCatalogViewModel> GetAddedCoursesForDateAsync(DateTime addDate, string trainingProviderName, string userId,
            Specializations? specializations)
         {
            var userSpecializationList = GetUserSpecializationList(specializations);

            var courseQuery = UpdateContext.UpdateEvents
               .Where(x => DbFunctions.TruncateTime(x.StartedOn) == DbFunctions.TruncateTime(addDate) &&
                           x.TrainingProvider.Name == trainingProviderName &&
                           x.CoursesUpdates.Any(cu => cu.OperationType == OperationType.Add))
               .SelectMany(x => x.CoursesUpdates)
               .Where(x => x.OperationType == OperationType.Add)
               .Select(x => x.Course)
               .Where(x => !x.IsDeleted &&
                           (!userSpecializationList.Any() ||
                            x.CourseSpecializations.Any(cs => userSpecializationList.Contains(cs.Specialization))));

            var courses = await ProjectToCourseCatalogEntry(courseQuery, userId).ToListAsync();

            if (courses.Any())
            {
               var courseList = new CourseCatalogViewModel
               {
                  CourseListFor = CourseListFor.Author,
                  Courses = courses
               };

               return courseList;
            }

            return null;
         }


         public IQueryable<CourseCatalogEntryViewModel> ProjectToCourseCatalogEntry(IQueryable<Course> courseQuery, string userId)
         {
            var projectionQuery = courseQuery
                .Select(x => new CourseCatalogEntryViewModel
                {
                   TrainingProvider = new TrainingProviderViewModel
                   {
                      Name = x.TrainingProvider.Name,
                      LogoFileName = x.TrainingProvider.LogoFileName,
                      SiteUrl = x.TrainingProvider.SiteUrl
                   },
                   Category = new CategoryViewModel
                   {
                      Title = x.Category.Title,
                      UrlName = x.Category.UrlName,
                      LogoFileName = x.Category.LogoFileName
                   },
                   Course = new CourseViewModel
                   {
                      Id = x.Id,
                      Title = x.Title,
                      SiteUrl = x.SiteUrl,
                      UrlName = x.UrlName,
                      HasClosedCaptions = x.HasClosedCaptions,
                      Level = x.Level,
                      Rating = x.Rating,
                      Duration = x.Duration,
                      ReleaseDate = x.ReleaseDate,
                      LearningState = x.Subscriptions
                         .Where(s => s.UserId == userId)
                         .Select(s => s.State)
                         .FirstOrDefault(),
                      Authors = x.CourseAuthors
                      .Where(a => (!a.IsDeleted))
                      .Select(a => new CourseAuthorViewModel
                      {
                         FullName = a.TrainingProviderAuthor.FullName,
                         UrlName = a.TrainingProviderAuthor.UrlName,
                         IsCoAuthor = a.IsAuthorCoAuthor
                      }).ToList()
                   }
                });

            return projectionQuery;
         }


         public CourseCatalogViewModel GetCourseCatalog(string trainingProviderName)
         {
            var tokenCatalog = GetTokenCatalog();

            var selectedToken = !string.IsNullOrWhiteSpace(trainingProviderName) &&
                                tokenCatalog.Contains(trainingProviderName, StringComparer.OrdinalIgnoreCase)
               ? trainingProviderName.ToTitleCaseInvariant()
               : NavigationViewModel.ALLToken;

            var courseCatalogViewModel = new CourseCatalogViewModel
            {
               Navigation = new NavigationViewModel
               {
                  SelectedToken = selectedToken,
                  TokenCatalog = tokenCatalog
               }
            };

            return courseCatalogViewModel;
         }

         public async Task<CourseInfoViewModel> GetCourseInfoAsync(string trainingProviderName, string courseUrlName, string userId)
         {
            var courseInfo = await CatalogContext.Courses
               .Where(x => x.UrlName == courseUrlName && x.TrainingProvider.Name == trainingProviderName)
               .Select(x => new CourseInfoViewModel
               {
                  TrainingProvider = new TrainingProviderViewModel
                  {
                     Name = x.TrainingProvider.Name,
                     SiteUrl = x.TrainingProvider.SiteUrl,
                     LogoFileName = x.TrainingProvider.LogoFileName
                  },
                  Category = new CategoryViewModel
                  {
                     Title = x.Category.Title,
                     UrlName = x.Category.UrlName,
                     LogoFileName = x.Category.LogoFileName
                  },

                  Id = x.Id,
                  Title = x.Title,
                  SiteUrl = x.SiteUrl,
                  UrlName = x.UrlName,
                  HasClosedCaptions = x.HasClosedCaptions,
                  Level = x.Level,
                  Rating = x.Rating,
                  Duration = x.Duration,
                  ReleaseDate = x.ReleaseDate,
                  LearningState = x.Subscriptions
                     .Where(s => s.UserId == userId)
                     .Select(s => s.State)
                     .FirstOrDefault(),
                  Authors = x.CourseAuthors
                     .Where(ca => !ca.IsDeleted)
                     .Select(a => new CourseAuthorViewModel
                     {
                        FullName = a.TrainingProviderAuthor.FullName,
                        UrlName = a.TrainingProviderAuthor.UrlName,
                        IsCoAuthor = a.IsAuthorCoAuthor
                     }).ToList(),

                  AuthorsInfo = x.CourseAuthors
                     .Where(ca => !ca.IsDeleted)
                     .Select(ca => new AuthorViewModel
                     {
                        SiteUrl = ca.TrainingProviderAuthor.SiteUrl,
                        UrlName = ca.TrainingProviderAuthor.UrlName,
                        FirstName = ca.TrainingProviderAuthor.Author.FirstName,
                        LastName = ca.TrainingProviderAuthor.Author.LastName,
                        Avatar = ca.TrainingProviderAuthor.Author.Avatar,
                        Bio = ca.TrainingProviderAuthor.Author.Bio,
                        Badge = ca.TrainingProviderAuthor.Author.Badge,
                        Social = ca.TrainingProviderAuthor.Author.Social
                     }).ToList(),

                  Description = x.Description,
                  ShortDescription = x.ShortDescription,

                  Modules = x.Modules
                     .OrderBy(ch => ch.Ordinal)
                     .Select(ch => new ModuleViewModel
                     {
                        Ordinal = ch.Ordinal,
                        Description = ch.Description,
                        Title = ch.Title,
                        Duration = ch.Duration,
                        Topics = ch.Topics
                           .OrderBy(t => t.Ordinal)
                           .Select(t => new TopicViewModel
                           {
                              Ordinal = t.Ordinal,
                              Title = t.Title,
                              Duration = t.Duration
                           }).ToList()
                     }).ToList()

               }).SingleOrDefaultAsync();

            if (courseInfo == null)
            {
               return null;
            }

            courseInfo.Category.TrainingProvider = courseInfo.TrainingProvider;

            return courseInfo;
         }


         public async Task<CourseCatalogSearchResult> GetCourseCatalogSearchResultAsync(CourseCatalogSearchRequest request, string userId, Specializations? specializations)
         {

            var courseQuery = GetCoursesQueryForSpecializations(specializations);

            if (!string.IsNullOrWhiteSpace(request.trainingProviderName) &&
                request.trainingProviderName != NavigationViewModel.ALLToken)
            {
               courseQuery = courseQuery
                  .Where(x => x.TrainingProvider.Name == request.trainingProviderName);
            }

            var recordsTotal = await courseQuery.CountAsync();

            var recordsFiltered = recordsTotal;
            if (!string.IsNullOrWhiteSpace(request.searchTerm))
            {
               courseQuery = courseQuery
                  .Where(x => x.Title.Contains(request.searchTerm));
               recordsFiltered = await courseQuery.CountAsync();
            }

            var catalog = await courseQuery
               .SortCourses(request.order)
               .Skip(request.start)
               .Take(request.length)
               .Select(x => new CourseCatalogEntryViewModel
               {
                  TrainingProvider = new TrainingProviderViewModel
                  {
                     Name = x.TrainingProvider.Name,
                     LogoFileName = x.TrainingProvider.LogoFileName,
                     SiteUrl = x.TrainingProvider.SiteUrl
                  },
                  Category = new CategoryViewModel
                  {
                     Title = x.Category.Title,
                     UrlName = x.Category.UrlName,
                     LogoFileName = x.Category.LogoFileName
                  },
                  Course = new CourseViewModel
                  {
                     Id = x.Id,
                     Title = x.Title,
                     SiteUrl = x.SiteUrl,
                     UrlName = x.UrlName,
                     HasClosedCaptions = x.HasClosedCaptions,
                     Level = x.Level,
                     Rating = x.Rating,
                     Duration = x.Duration,
                     ReleaseDate = x.ReleaseDate,
                     LearningState = x.Subscriptions
                        .Where(s => s.UserId == userId)
                        .Select(s => s.State)
                        .FirstOrDefault(),
                     Authors = x.CourseAuthors
                     .Where(a => (!a.IsDeleted))
                     .Select(a => new CourseAuthorViewModel
                     {
                        FullName = a.TrainingProviderAuthor.FullName,
                        UrlName = a.TrainingProviderAuthor.UrlName,
                        IsCoAuthor = a.IsAuthorCoAuthor
                     }).ToList()
                  }
               }).ToListAsync();

            foreach (var entry in catalog)
            {
               entry.Category.TrainingProvider = entry.TrainingProvider;
            }

            var searchResult = new CourseCatalogSearchResult
            {
               draw = request.draw,
               recordsTotal = recordsTotal,
               recordsFiltered = recordsFiltered,
               catalog = catalog
            };
            return searchResult;
         }

      }
   }


   public static class CourseExtensions
   {
      public static IQueryable<Course> SortCourses(this IQueryable<Course> source, List<Dictionary<string, string>> sortParam)
      {
         int columnNumber = Convert.ToInt32(sortParam[0]["column"]);
         string sortDirection = sortParam[0]["dir"];

         switch (columnNumber)
         {
            case 0:
               source = sortDirection == "asc"
                  ? source.OrderBy(x => x.TrainingProvider.Name)
                  : source.OrderByDescending(x => x.TrainingProvider.Name);
               break;
            case 1:
               source = sortDirection == "asc"
                  ? source.OrderBy(x => x.Category.Title)
                  : source.OrderByDescending(x => x.Category.Title);
               break;
            case 2:
               source = sortDirection == "asc"
                  ? source.OrderBy(x => x.Title)
                  : source.OrderByDescending(x => x.Title);
               break;
            // authors sort not supported
            case 3:
               throw new NotSupportedException(Resources.NotSupported_AuthorsSort);
            case 4:
               source = sortDirection == "asc"
                  ? source.OrderBy(x => x.Level)
                  : source.OrderByDescending(x => x.Level);
               break;
            case 5:
               source = sortDirection == "asc"
                  ? source.OrderBy(x => x.Duration)
                  : source.OrderByDescending(x => x.Duration);
               break;
            case 6:
               source = sortDirection == "asc"
                  ? source.OrderBy(x => x.ReleaseDate)
                  : source.OrderByDescending(x => x.ReleaseDate);
               break;
            case 7:
               source = sortDirection == "asc"
                  ? source.OrderBy(x => x.Rating.Rating)
                  : source.OrderByDescending(x => x.Rating.Rating);
               break;
            default:
               throw new ArgumentOutOfRangeException();
         }

         return source;
      }
   }
}