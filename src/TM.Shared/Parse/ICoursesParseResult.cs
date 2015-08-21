using System.Collections.Generic;

namespace TM.Shared.Parse
{
   public interface ICoursesParseResult<TCourseParseModel>
      where TCourseParseModel : ICourseParseModel
   {
      Dictionary<ICourseUrlNameNaturalKey, TCourseParseModel> CourseContainer { get;  }
   }
}