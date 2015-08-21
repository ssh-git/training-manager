using System.Text;
using HtmlAgilityPack;
using TM.Shared.HtmlContainer;

namespace TM.Data.Parse
{
   public class HtmlAgilityPackHtmlContainer : IHtmlContainer
   {
      private readonly HtmlDocument _document = new HtmlDocument();

      public void LoadHtml(string html)
      {
         _document.LoadHtml(html);
      }

      public void Save(string path, Encoding encoding)
      {
         _document.Save(path, encoding);
      }

      public IQueryableNode RootNode
      {
         get { return new HtmlAgilityPackNode(_document.DocumentNode); }
      }
   }
}