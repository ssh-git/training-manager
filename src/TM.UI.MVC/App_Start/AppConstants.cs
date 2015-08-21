using System.Configuration;
using System.Web.Hosting;

namespace TM.UI.MVC
{
   public static class AppConstants
   {
      public static class VirtualPaths
      {
         public const string ImagePlaceholder = "~/Content/images/placeholder.png";

         public const string CategoryContent = "~/Content/images/category/";

         public const string AuthorsContent = "~/Content/images/authors/";

         public const string BadgesContent = "~/Content/images/badges/";

         public const string TrainingProvidersContent = "~/Content/images/training-provider/";
      }

      public static class ServerPaths
      {
         private static string _mediaDirectory;
         private static string _archiveDirectory;

         public static string MediaDirectory
         {
            get { return _mediaDirectory ?? (_mediaDirectory = HostingEnvironment.MapPath("~/Content/images/")); }
         }
         public static string ArchiveDirectory
         {
            get { return _archiveDirectory ?? (_archiveDirectory = HostingEnvironment.MapPath(ConfigurationManager.AppSettings["TM.Paths.Archive"])); }
         }
      }

      public static class RouteNames
      {
         /// <summary>
         /// Name for route 'error'
         /// </summary>
         public const string Error = "Error Route";

         /// <summary>
         /// Name for route 'learning-plan'
         /// </summary>
         public const string LearningPlan = "Learning Plan Route";

         /// <summary>
         /// Name for route 'learning-plan/subscription'
         /// </summary>
         public const string LearningPlanCourseSubscription = "Learning Plan Course Subscription Route";

         /// <summary>
         /// Name for route 'search/{searchTerm}'
         /// </summary>
         public const string Search = "Search Route";

         /// <summary>
         /// Name for route '{controller}/{action}/{id}'
         /// </summary>
         public const string Default = "Default"; 

         /// <summary>
         /// Name for route 'training-providers'
         /// </summary>
         public const string AllTrainingProviders = "All Training Providers Route"; 

         /// <summary>
         /// Name for route 'training-provider/{trainingProviderName}'
         /// </summary>
         public const string TrainingProvider = "Training Provider Route";

         /// <summary>
         /// Name for route 'authors'
         /// </summary>
         public const string AllAuthors = "All Authors Route";

         /// <summary>
         /// Name for route '{trainingProviderName}/authors'
         /// </summary>
         public const string TrainingProviderAuthors = "TP Authors Route";

         /// <summary>
         /// Name for route '{trainingProviderName}/author/{authorUrlName}'
         /// </summary>
         public const string Author = "Author Route";

         /// <summary>
         /// Name for route 'categories'
         /// </summary>
         public const string AllCategories = "All Categories Route"; 

         /// <summary>
         /// Name for route '{trainingProviderName}/categories'
         /// </summary>
         public const string TrainingProviderCategories = "TP Categories Route";

         /// <summary>
         /// Name for route '{trainingProviderName}/category/{categoryUrlName}'
         /// </summary>
         public const string TrainingProviderCategory = "TP Courses In Category Route";

         /// <summary>
         /// Name for route 'courses'
         /// </summary>
         public const string AllCourses = "All Courses Route";

         /// <summary>
         /// Name for route 'courses/top-new-courses'
         /// </summary>
         public const string TopNewCourses = "Top New Courses Route";


         /// <summary>
         /// Name for route 'courses/top-month-popular-courses'
         /// </summary>
         public const string TopMonthPopularCourses = "Top Month Popular Courses Route";

         /// <summary>
         /// Name for route 'courses/for-category/{categoryUrlName}/at/{trainingProviderName}'
         /// </summary>
         public const string CoursesForTrainingProviderCategory = "Courses For Training Provider Category Route"; 

         /// <summary>
         /// Name for route 'courses/for-author/{authorUrlName}/at/{trainingProviderName}'
         /// </summary>
         public const string CoursesForTrainingProviderAuthor = "Courses For Training Provider Author Route";

         /// <summary>
         /// Name for route 'courses/added-on/{addDate}/at/{trainingProviderName}'
         /// </summary>
         public const string CoursesAddedOnDate = "Courses Added On Date Route";

         /// <summary>
         /// Name for route '{trainingProviderName}/courses'
         /// </summary>
         public const string TrainingProviderCourses = "TP Courses Route";

         /// <summary>
         /// Name for route '{trainingProviderName}/{categoryUrlName}/{courseUrlName}'
         /// </summary>
         public const string Course = "Course Route";
      }

      public static class UserRole
      {
         public const string Administrator = "administrator";
      }
   }
}