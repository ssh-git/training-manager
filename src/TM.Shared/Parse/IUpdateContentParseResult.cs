namespace TM.Shared.Parse
{
   public interface IUpdateContentParseResult<TCategoryParseModel, TCourseParseModel, TAuthorParseModel>
      where TCategoryParseModel : ICategoryParseModel
      where TCourseParseModel : ICourseParseModel
      where TAuthorParseModel : IAuthorParseModel
   {
      ICategoriesParseResult<TCategoryParseModel> CategoriesParseResult { get; }
      ICoursesParseResult<TCourseParseModel> CoursesParseResult { get; }
      IAuthorsParseResult<TAuthorParseModel> AuthorsParseResult { get; }
   }
}