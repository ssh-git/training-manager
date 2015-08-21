namespace TM.Shared.HtmlContainer
{
   public interface INode
   {
      string InnerText { get; }
      string OuterText { get; }
      string GetAttributeValue(string attributeName);
   }
}