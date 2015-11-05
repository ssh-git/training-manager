using System.Web.Mvc;
using System.Web.Routing;

namespace TM.UI.MVC
{
   public class RouteConfig
   {
      public static void RegisterRoutes(RouteCollection routes)
      {
         routes.IgnoreRoute("{resource}.axd/{*pathInfo}");


         #region Error Routes

         routes.MapRoute(
            name: AppConstants.RouteNames.Error,
            url: "error",
            defaults: new
            {
               controller = "Home",
               action = "Error"
            });

         #endregion

         #region Feed Routes

         routes.MapRoute(
            name: AppConstants.RouteNames.Feed,
            url: "feeds/atom",
            defaults: new
            {
               controller = "Feed",
               action = "NewCoursesAsync"
            });

         #endregion


         #region Learning Plan Routes

         routes.MapRoute(
            name: AppConstants.RouteNames.LearningPlan,
            url: "learning-plan",
            defaults: new
            {
               controller = "LearningPlan",
               action = "LearningPlan"
            });

         routes.MapRoute(
            name: AppConstants.RouteNames.LearningPlanCourseSubscription,
            url: "learning-plan/subscription",
            defaults: new
            {
               controller = "LearningPlan",
               action = "Subscription"
            });

         #endregion


         #region Training Provider Routes


         routes.MapRoute(
            name: AppConstants.RouteNames.AllTrainingProviders,
            url: "training-providers",
            defaults: new
            {
               controller = "TrainingProvider",
               action = "TrainingProviders"
            });

         routes.MapRoute(
            name: AppConstants.RouteNames.TrainingProvider,
            url: "training-provider/{trainingProviderName}",
            defaults: new
            {
               controller = "TrainingProvider",
               action = "TrainingProvider"
            });



         #endregion


         #region Author Routes

         routes.MapRoute(
          name: AppConstants.RouteNames.AllAuthors,
          url: "authors",
          defaults: new
          {
             controller = "Author",
             action = "Authors"
          });

         routes.MapRoute(
          name: AppConstants.RouteNames.TrainingProviderAuthors,
          url: "{trainingProviderName}/authors",
          defaults: new
          {
             controller = "Author",
             action = "Authors"
          });

         routes.MapRoute(
            name: AppConstants.RouteNames.Author,
            url: "{trainingProviderName}/author/{authorUrlName}",
            defaults: new
            {
               controller = "Author",
               action = "Author"
            });

         #endregion


         #region Category Routes

         routes.MapRoute(
            name: AppConstants.RouteNames.AllCategories,
            url: "categories",
            defaults: new
            {
               controller = "Category",
               action = "Categories"
            });

         routes.MapRoute(
            name: AppConstants.RouteNames.TrainingProviderCategories,
            url: "{trainingProviderName}/categories",
            defaults: new
            {
               controller = "Category",
               action = "Categories"
            });

         routes.MapRoute(
            name: AppConstants.RouteNames.TrainingProviderCategory,
            url: "{trainingProviderName}/category/{categoryUrlName}",
            defaults: new
            {
               controller = "Category",
               action = "Category"
            });

         #endregion


         #region Course Routes

         routes.MapRoute(
            name: AppConstants.RouteNames.AllCourses,
            url: "courses",
            defaults: new
            {
               controller = "Course",
               action = "Courses"
            });

         routes.MapRoute(
           name: AppConstants.RouteNames.TopNewCourses,
           url: "courses/top-new-courses",
           defaults: new
           {
              controller = "Course",
              action = "Top5NewCourses"
           });

         routes.MapRoute(
           name: AppConstants.RouteNames.TopMonthPopularCourses,
           url: "courses/top-month-popular-courses",
           defaults: new
           {
              controller = "Course",
              action = "Top5MonthPopularCourses"
           });

         routes.MapRoute(
           name: AppConstants.RouteNames.CoursesForTrainingProviderCategory,
           url: "courses/for-category/{categoryUrlName}/at/{trainingProviderName}",
           defaults: new
           {
              controller = "Course",
              action = "CategoryCourses"
           });

         routes.MapRoute(
           name: AppConstants.RouteNames.CoursesForTrainingProviderAuthor,
           url: "courses/for-author/{authorUrlName}/at/{trainingProviderName}",
           defaults: new
           {
              controller = "Course",
              action = "AuthorCourses"
           });

         routes.MapRoute(
           name: AppConstants.RouteNames.CoursesAddedOnDate,
           url: "courses/added-on/{addDate}/at/{trainingProviderName}",
           defaults: new
           {
              controller = "Course",
              action = "AddedCourses"
           });

         routes.MapRoute(
            name: AppConstants.RouteNames.TrainingProviderCourses,
            url: "{trainingProviderName}/courses",
            defaults: new
            {
               controller = "Course",
               action = "Courses"
            });


         routes.MapRoute(
            name: AppConstants.RouteNames.Course,
            url: "{trainingProviderName}/{categoryUrlName}/{courseUrlName}",
            defaults: new
            {
               controller = "Course",
               action = "Course"
            });

         #endregion


         #region Search Routes

         routes.MapRoute(
            name: AppConstants.RouteNames.Search,
            url: "search/{searchTerm}",
            defaults: new
            {
               controller = "Course",
               action = "Search"
            });

         #endregion

         routes.MapRoute(
            name: AppConstants.RouteNames.Default,
            url: "{controller}/{action}/{id}",
            defaults: new { controller = "Home", action = "Home", id = UrlParameter.Optional }
        );

      }
   }
}
