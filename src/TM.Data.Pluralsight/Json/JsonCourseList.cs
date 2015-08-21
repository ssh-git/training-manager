// ReSharper disable All

namespace TM.Data.Pluralsight.Json
{
   internal class JsonCourseStringList
   {
      public object[] courses { get; set; }
   }


   internal class JsonCourseList
   {
      public int totalNumberOfCoursesFound { get; set; }
      public JsonCourseListItem[] courses { get; set; }
      // public int totalNumberOfRetiredCourses { get; set; }
      // public JsonCourseLevelCounts courseLevelCounts { get; set; }


      /*public class JsonCourseLevelCounts
      {
         public int beginner { get; set; }
         public int intermediate { get; set; }
         public int advanced { get; set; }
         public int retiredBeginner { get; set; }
         public int retiredIntermediate { get; set; }
         public int retiredAdvanced { get; set; }
      }*/


      public class JsonCourseListItem
      {
         // public string title { get; set; }
         // public string level { get; set; }
         // public string duration { get; set; }
         // public DateTime releaseDate { get; set; }
         public string name { get; set; }
         // public JsonCourseListItemAuthor[] authors { get; set; }
         // public bool hasTranscript { get; set; }
         // public JsonCourseListItemRating courseRating { get; set; }
         // public bool isRetired { get; set; }
         // public int recentViewTime { get; set; }
      }

      /*public class JsonCourseListItemRating
      {
         public int currentUsersRating { get; set; }
         public float averageRating { get; set; }
         public float rating { get; set; }
         public bool canRateThisCourse { get; set; }
         public string courseName { get; set; }
         public int numberOfRaters { get; set; }
         public bool hasUserRatedCourse { get; set; }
      }*/

      /* public class JsonCourseListItemAuthor
       {
          public string handle { get; set; }
          public string firstName { get; set; }
          public string lastName { get; set; }
       }*/

   }

}
