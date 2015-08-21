using System.Collections.Generic;
using TM.Shared;
using TM.Shared.Parse;

namespace TM.Data.Pluralsight
{
   internal class PluralsightCoursesParseResult : ICoursesParseResult<PluralsightCourse>
   {
      public PluralsightCoursesParseResult(Dictionary<ICourseUrlNameNaturalKey, PluralsightCourse> courseContainer)
      {
         CourseContainer = courseContainer;
      }

      public Dictionary<ICourseUrlNameNaturalKey, PluralsightCourse> CourseContainer { get; private set; }
   }
}