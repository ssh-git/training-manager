using System.Collections.Generic;

namespace TM.Shared.Parse
{
   public interface IAuthorsParseResult<TAuthorParseModel>
      where TAuthorParseModel : IAuthorParseModel
   {
      Dictionary<IAuthorUrlNameNaturalKey, TAuthorParseModel> AuthorsExceptWhoseUrlNullContainer { get;  }
   }
}