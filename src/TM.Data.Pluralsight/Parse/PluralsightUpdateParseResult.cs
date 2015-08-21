using TM.Shared.Parse;

namespace TM.Data.Pluralsight
{
   internal class PluralsightUpdateParseResult :
      IUpdateContentParseResult<PluralsightCategory, PluralsightCourse, PluralsightAuthor>
   {
      public PluralsightUpdateParseResult(
         ICategoriesParseResult<PluralsightCategory> categoriesParseResult,
         ICoursesParseResult<PluralsightCourse> coursesParseResult,
         IAuthorsParseResult<PluralsightAuthor> authorsParseResult)
      {
         CategoriesParseResult = categoriesParseResult;
         CoursesParseResult = coursesParseResult;
         AuthorsParseResult = authorsParseResult;
      }

      public ICategoriesParseResult<PluralsightCategory> CategoriesParseResult { get; private set; }
      public ICoursesParseResult<PluralsightCourse> CoursesParseResult { get; private set; }
      public IAuthorsParseResult<PluralsightAuthor> AuthorsParseResult { get; private set; }
   }
}