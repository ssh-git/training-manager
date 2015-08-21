using System.Data.Entity;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using TM.Data.Update;
using TM.Shared;
using TM.UI.MVC.Infrastructure;

namespace TM.UI.MVC.Models
{
   [SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
   public class HomeViewModels
   {
      public class CatalogStatistic
      {
         public int TrainingProviderCount { get; set; }
         public int CategoryCount { get; set; }
         public int CourseCount { get; set; }
         public int AuthorCount { get; set; }
      }

      public class StatisticViewModel
      {
         public string Title { get; set; }
         public int Count { get; set; }
         public string Url { get; set; }
      }

      public class HomeViewModel
      {
         public HomeViewModel(UrlHelper urlHelper,CatalogStatistic catalogStatistic )
         {
            TrainingProvidersStatistic = new StatisticViewModel
            {
               Title = "Providers",
               Count = catalogStatistic.TrainingProviderCount,
               Url = urlHelper.RouteUrl(AppConstants.RouteNames.AllTrainingProviders)
            };

            CategoriesStatistic = new StatisticViewModel
            {
               Title = "Categories",
               Count = catalogStatistic.CategoryCount,
               Url = urlHelper.RouteUrl(AppConstants.RouteNames.AllCategories)
            };

            CoursesStatistic = new StatisticViewModel
            {
               Title = "Courses",
               Count = catalogStatistic.CourseCount,
               Url = urlHelper.RouteUrl(AppConstants.RouteNames.AllCourses)
            };

            AuthorsStatistic = new StatisticViewModel
            {
               Title = "Authors",
               Count = catalogStatistic.AuthorCount,
               Url = urlHelper.RouteUrl(AppConstants.RouteNames.AllAuthors)
            };
         }
         public StatisticViewModel TrainingProvidersStatistic { get; set; }
         public StatisticViewModel CategoriesStatistic { get; set; }
         public StatisticViewModel CoursesStatistic { get; set; }
         public StatisticViewModel AuthorsStatistic { get; set; }
      }

      public class HomeManager : CatalogManagerBase
      {
         public HomeManager()
         {
         }

         public HomeManager(UpdateDbContext context) : base(context)
         {
         }

         [SuppressMessage("ReSharper", "ExceptionNotDocumented")]
         public async Task<CatalogStatistic> GetCatalogStatisticAsync(Specializations? specializations)
         {
            var coursesQuery = GetCoursesQueryForSpecializations(specializations);

            var statistic = await (from needForBatch in coursesQuery
               select new CatalogStatistic
               {
                  TrainingProviderCount =
                     CatalogContext.TrainingProviders.Count(
                        tp => coursesQuery.Any(q => q.TrainingProviderId == tp.Id) && !tp.IsDeleted),
                  CategoryCount =
                     CatalogContext.Categories.Count(cat => coursesQuery.Any(q => q.CategoryId == cat.Id) && !cat.IsDeleted),
                  CourseCount = coursesQuery.Count(),
                  AuthorCount = CatalogContext.CourseAuthors
                     .Where(ca => coursesQuery.Any(q => q.Id == ca.CourseId) && !ca.IsDeleted)
                     .GroupBy(ca => ca.AuthorId).Count()
               }).FirstAsync();

            return statistic;
         }
      }
   }
}