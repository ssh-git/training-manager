using System.Collections.Generic;
using TM.Shared;
using TM.Shared.Parse;

namespace TM.Data.Pluralsight
{
   internal class PluralsightAuthorsParseResult : IAuthorsParseResult<PluralsightAuthor>
   {
      public PluralsightAuthorsParseResult(Dictionary<IAuthorUrlNameNaturalKey, PluralsightAuthor> authorsExceptWhoseUrlNullContainer)
      {
         AuthorsExceptWhoseUrlNullContainer = authorsExceptWhoseUrlNullContainer;
      }

      public Dictionary<IAuthorUrlNameNaturalKey, PluralsightAuthor> AuthorsExceptWhoseUrlNullContainer { get; private set; }
   }
}