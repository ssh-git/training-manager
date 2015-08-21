using System.Collections.Generic;

// ReSharper disable All

namespace TM.Data.Pluralsight.Json
{
  
   public class JsonAuthorCourses
   {
      public List<JsonAuthorCourse> courses { get; set; }
   }

   
   public class JsonAuthorCourse
   {
      // public string title { get; set; }
      // public string level { get; set; }
      // public string duration { get; set; }
      // public string releaseDate { get; set; }
      // public string description { get; set; }
      // public bool isNew { get; set; }
      public string name { get; set; }
      // public bool hasTranscript { get; set; }
      // public bool isRetired { get; set; }
      public string shortDescription { get; set; }
      // public int sequence { get; set; }
      // public string playerUrl { get; set; }
   }
}
