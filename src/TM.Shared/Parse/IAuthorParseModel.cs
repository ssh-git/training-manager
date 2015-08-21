namespace TM.Shared.Parse
{
   public interface IAuthorParseModel:IAuthorUrlNameNaturalKey, IAuthorFullNameNaturalKey
   {
      int Id { get; set; }
   }
}
