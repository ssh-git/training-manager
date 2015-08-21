using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using TM.Shared.DownloadManager;
using TM.Shared.Properties;

namespace TM.Shared.HtmlContainer
{
   public class HtmlLoader<THtmlContainer> : IHtmlLoader
      where THtmlContainer : IHtmlContainer, new()
   {
      private readonly Encoding _contentEncoding;

      public HtmlLoader()
         : this(Encoding.UTF8) { }


      /// <exception cref="ArgumentNullException"><paramref name="encoding"/> is <see langword="null" />.</exception>
      public HtmlLoader(Encoding encoding)
      {
         if (encoding == null) throw new ArgumentNullException("encoding");

         _contentEncoding = encoding;
      }


      /// <exception cref="HtmlLoadException">Error while loading the content.</exception>
      /// <exception cref="ArgumentNullException"><paramref name="contentUrlOrPath"/> is <see langword="null" />.</exception>
      public IHtmlContainer Load(string contentUrlOrPath, LocationType location)
      {
         return LoadAsync(contentUrlOrPath, location).Result;
      }


      /// <exception cref="HtmlLoadException">Error while loading the content.</exception>
      /// <exception cref="ArgumentNullException"><paramref name="contentUrlOrPath"/> is <see langword="null" />.</exception>
      public async Task<IHtmlContainer> LoadAsync(string contentUrlOrPath, LocationType location)
      {
         if (contentUrlOrPath == null) throw new ArgumentNullException("contentUrlOrPath");

         try
         {
            var container = new THtmlContainer();
            if (location == LocationType.Local)
            {
               await LoadFromFileAsync(container, contentUrlOrPath);
            } else if (location == LocationType.Remote)
            {
               await LoadFromWebAsync(container, contentUrlOrPath);
            }

            return container;
         }
         catch (Exception ex)
         {

            if (ex is AggregateException)
            {
               ex = ((AggregateException)ex).GetBaseException();
            }

            throw new HtmlLoadException(Resources.HtmlLoadException_Message, ex);
         }
      }


      private async Task LoadFromFileAsync(IHtmlContainer container, string path)
      {
         const int bufferSize = 4096;
         var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize,
            useAsync: true);

         using (var streamReader = new StreamReader(fileStream, _contentEncoding))
         {
            var content = await streamReader.ReadToEndAsync();
            container.LoadHtml(content);
         }
      }


      private async Task LoadFromWebAsync(IHtmlContainer container, string url)
      {
         using (var httpDownloadManager = new HttpDownloadManager())
         {
            var download = await httpDownloadManager.DownloadAsStringAsync(new Uri(url),
               acceptMediaType: @"text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
            if (download.IsSuccess)
            {
               container.LoadHtml(download.Result);
            } else
            {
               throw new DownloadException<string>(Resources.HtmlContentDownloadException_Message, download);
            }
         }
      }
   }
}