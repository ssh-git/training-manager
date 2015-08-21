using System;
using System.Threading.Tasks;

namespace TM.Shared.DownloadManager
{
   public interface IHttpDownloadManager:IDisposable
   {
      Task<DownloadResult<string>> DownloadAsStringAsync(Uri requestUri, string acceptMediaType = "*/*");
      Task<DownloadResult<byte[]>> DownloadFileAsync(Uri requestUri);
   }
}