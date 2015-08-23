using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace TM.Shared.DownloadManager
{
   public abstract class ProxyBase : IDisposable
   {
      protected readonly HttpClientHandler HttpClientHandler;

      protected ProxyBase()
      {
         HttpClientHandler = new HttpClientHandler
         {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            CookieContainer = new CookieContainer()
         };
      }

      /// <exception cref="ObjectDisposedException"></exception>
      public virtual HttpClient CreateHttpClient()
      {
         if (Disposed) { throw new ObjectDisposedException(GetType().FullName); }

         return new HttpClient(HttpClientHandler, disposeHandler: false)
         {
            Timeout = TimeSpan.FromMinutes(3.0)
         };
      }

      /// <exception cref="ObjectDisposedException"></exception>
      protected virtual void SetDefaultRequestHeaders(HttpRequestMessage requestMessage)
      {
         if (Disposed) { throw new ObjectDisposedException(GetType().FullName); }

         var headers = requestMessage.Headers;
         headers.AcceptEncoding.ParseAdd("gzip, deflate");
         headers.AcceptLanguage.ParseAdd("en,en-US;q=0.8,ru-RU;q=0.5,ru;q=0.3");
         headers.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 6.1; rv:38.0) Gecko/20100101 Firefox/38.0");
         headers.Referrer = new UriBuilder(requestMessage.RequestUri.Host).Uri;
         headers.Add("DNT", "1");
      }

      /// <exception cref="ObjectDisposedException"></exception>
      public virtual HttpRequestMessage GenerateRequestMessage(HttpMethod httpMethod, Uri requestUri, string acceptMediaType)
      {
         if (Disposed) { throw new ObjectDisposedException(GetType().FullName); }

         var httpRequestMessage = new HttpRequestMessage(httpMethod, requestUri);
         SetDefaultRequestHeaders(httpRequestMessage);

         httpRequestMessage.Headers.Accept.ParseAdd(acceptMediaType);

         return httpRequestMessage;
      }


      /// <exception cref="ObjectDisposedException"></exception>
      public virtual Task<string> GetResponseContentStringAsync(HttpResponseMessage httpResponseMessage)
      {
         if (Disposed) { throw new ObjectDisposedException(GetType().FullName); }

         if (httpResponseMessage.Content != null)
         {
            return httpResponseMessage.Content.ReadAsStringAsync();
         }
         return Task.FromResult(String.Empty);
      }


      #region IDisposable Implementation

      protected bool Disposed;

      public void Dispose()
      {
         Dispose(true);
         GC.SuppressFinalize(this);
      }

      protected virtual void Dispose(bool disposing)
      {
         if (!Disposed && disposing)
         {
            if (HttpClientHandler != null)
            {
               HttpClientHandler.Dispose();
            }

            Disposed = true;
         }
      }

      #endregion

   }
}