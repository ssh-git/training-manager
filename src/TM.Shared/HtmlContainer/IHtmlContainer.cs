using System.Text;

namespace TM.Shared.HtmlContainer
{
   public interface IHtmlContainer
   {
      void LoadHtml(string html);

      void Save(string path, Encoding encoding);
      
      IQueryableNode RootNode { get; }
   }
}
