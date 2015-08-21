using TM.Shared.HtmlContainer;

namespace TM.Shared.Parse
{
   public interface ITrainingCatalogParser<TCategoryParseModel, TCourseParseModel, TAuthorParseModel>
      where TCategoryParseModel : ICategoryParseModel
      where TCourseParseModel : ICourseParseModel
      where TAuthorParseModel : IAuthorParseModel
   {
      IUpdateContentParseResult<TCategoryParseModel, TCourseParseModel, TAuthorParseModel> Parse(IHtmlContainer catalog);
   }
}