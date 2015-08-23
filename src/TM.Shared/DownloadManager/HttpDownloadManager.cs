using System;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace TM.Shared.DownloadManager
{

   public sealed class HttpDownloadManager : IHttpDownloadManager
   {
      private HttpClient _httpClient;
      private ProxyBase _proxy;

      private const int DefaultRequestDelay = 10000;
      private const int DefaultFileRequestDelay = 1000;
      private const int DefaultRetryAttempts = 3;
      private static readonly ProxyBase DefaultProxy = new NullProxy();

      private readonly int _retryAttempts;
      private readonly int _requestDelay;
      private readonly int _fileRequestDelay;


      #region Constructors

      public HttpDownloadManager()
         : this(ActivatorProxy.Instance) { }


      /// <exception cref="ArgumentNullException"><paramref name="activatorProxy"/> is <see langword="null" />.</exception>
      [SuppressMessage("ReSharper", "ExceptionNotDocumented")]
      internal HttpDownloadManager(IActivatorProxy activatorProxy)
      {
         if (activatorProxy == null)
            throw new ArgumentNullException("activatorProxy");

         var requestDelaySetting = ConfigurationManager.AppSettings["TM.HttpDownloadManager.RequestDelay"];
         if (requestDelaySetting == null)
         {
            _requestDelay = DefaultRequestDelay;
         }

         _requestDelay = Convert.ToInt32(requestDelaySetting);


         var fileRequestDelaySetting = ConfigurationManager.AppSettings["TM.HttpDownloadManager.FileRequestDelay"];
         if (fileRequestDelaySetting == null)
         {
            _fileRequestDelay = DefaultFileRequestDelay;
         }

         _fileRequestDelay = Convert.ToInt32(fileRequestDelaySetting);

         var retryAttemptsSetting = ConfigurationManager.AppSettings["TM.HttpDownloadManager.RetryAttempts"];
         if (retryAttemptsSetting == null)
         {
            _retryAttempts = DefaultRetryAttempts;
         }

         _retryAttempts = Convert.ToInt32(retryAttemptsSetting);

         var proxyTypeSetting = ConfigurationManager.AppSettings["TM.HttpDownloadManager.ProxyType"];
         if (proxyTypeSetting == null)
         {
            _proxy = DefaultProxy;
         } else
         {
            _proxy = activatorProxy.CreateInstance<ProxyBase>(proxyTypeSetting);
         }

      }

      public HttpDownloadManager(int requestDelay = DefaultRequestDelay, int fileRequestDelay = DefaultFileRequestDelay,
         int retryAttempts = DefaultRetryAttempts, ProxyBase proxy = null)
      {
         _requestDelay = requestDelay;
         _fileRequestDelay = fileRequestDelay;
         _retryAttempts = retryAttempts;

         _proxy = proxy ?? DefaultProxy;
      }

      #endregion




      /// <exception cref="ObjectDisposedException"></exception>
      public async Task<DownloadResult<byte[]>> DownloadFileAsync(Uri requestUri)
      {
         if (_disposed)
            throw new ObjectDisposedException(GetType().FullName);

         var response = await DownloadAsync(requestUri, _fileRequestDelay, acceptMediaType: "*/*");

         if (response == null || !response.IsSuccessStatusCode)
         {
            var request = _proxy.GenerateRequestMessage(HttpMethod.Get, requestUri, acceptMediaType: "*/*");
            return new DownloadResult<byte[]>(request, response);
         }

         var memoryStream = new MemoryStream();
         await response.Content.CopyToAsync(memoryStream);

         var downloadResult = new DownloadResult<byte[]>(memoryStream.ToArray());

         return downloadResult;
      }

      /// <exception cref="ObjectDisposedException"></exception>
      public async Task<DownloadResult<string>> DownloadAsStringAsync(Uri requestUri, string acceptMediaType = "*/*")
      {
         if (_disposed)
            throw new ObjectDisposedException(GetType().FullName);

         var response = await DownloadAsync(requestUri, _requestDelay, acceptMediaType);

         if (response == null || !response.IsSuccessStatusCode)
         {
            var request = _proxy.GenerateRequestMessage(HttpMethod.Get, requestUri, acceptMediaType);
            return new DownloadResult<string>(request, response);
         }

         var responseString = await _proxy.GetResponseContentStringAsync(response);

         var downloadResult = new DownloadResult<string>(responseString);

         return downloadResult;
      }


      private async Task<HttpResponseMessage> DownloadAsync(Uri requestUri, int requestDelay, string acceptMediaType)
      {
         if (_httpClient == null)
         {
            _httpClient = _proxy.CreateHttpClient();
         }

         var requestAttemptNumber = 1;

         var request = _proxy.GenerateRequestMessage(HttpMethod.Get, requestUri, acceptMediaType);

         HttpResponseMessage response;

         do
         {
            await Task.Delay(requestDelay);
            response = await GetHttpResponseMessage(request);
            if (response != null && response.IsSuccessStatusCode && response.Content.Headers.ContentLength > 0)
            {
               break;
            }
            request = _proxy.GenerateRequestMessage(HttpMethod.Get, requestUri, acceptMediaType);
         } while (requestAttemptNumber++ <= _retryAttempts);

         return response;
      }

      private async Task<HttpResponseMessage> GetHttpResponseMessage(HttpRequestMessage requestMessage)
      {
         try
         {
            var response = await _httpClient.SendAsync(requestMessage);
            return response;
         }
         catch
         {
            return null;
         }
      }


      #region IDisposable Implementation

      private bool _disposed;

      public void Dispose()
      {
         if (!_disposed)
         {
            if (_httpClient != null)
            {
               _httpClient.Dispose();
               _httpClient = null;
            }

            if (_proxy != null)
            {
               _proxy.Dispose();
               _proxy = null;
            }

            _disposed = true;
         }
      }

      #endregion
   }
}
