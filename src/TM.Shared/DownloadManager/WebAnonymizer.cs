using System;
using System.Configuration;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TM.Shared.Properties;

namespace TM.Shared.DownloadManager
{
   public abstract class WebAnonymizer : ProxyBase
   {
      protected string Host;
      /// <exception cref="ConfigurationErrorsException">Configuration appSettings section does not contains TM.WebAnonymizer.Host settings.</exception>
      protected WebAnonymizer()
      {
         var hostSetting = ConfigurationManager.AppSettings["TM.WebAnonymizer.Host"];
         if (string.IsNullOrWhiteSpace(hostSetting))
         {
            throw new ConfigurationErrorsException(Resources.ConfigurationEntyNotFound_WebAnonymizerHost);
         }

         Host = hostSetting;
      }


      /// <exception cref="ArgumentException"><paramref name="host"/> is <see langword="null" /> or whitespace..</exception>
      protected WebAnonymizer(string host)
      {
         if (string.IsNullOrWhiteSpace(host))
         {
            throw new ArgumentException(Resources.ArgumentNullOrWhitespace_Host, "host");
         }

         Host = host;
      }


      protected abstract Uri GenerateProxyUri(Uri sourceUri);

      protected abstract void FixUrls(StringBuilder documentContainer);


      /// <exception cref="ObjectDisposedException"></exception>
      public override HttpRequestMessage GenerateRequestMessage(HttpMethod httpMethod, Uri requestUri, string acceptMediaType)
      {
         if (Disposed) { throw new ObjectDisposedException(GetType().FullName); }

         var proxyRequestUri = GenerateProxyUri(requestUri);
         var httpRequestMessage = base.GenerateRequestMessage(HttpMethod.Get, proxyRequestUri, acceptMediaType);

         return httpRequestMessage;
      }

      /// <exception cref="ObjectDisposedException"></exception>
      public override async Task<string> GetResponseContentStringAsync(HttpResponseMessage httpResponseMessage)
      {
         if (Disposed) { throw new ObjectDisposedException(GetType().FullName); }

         var responseString = await base.GetResponseContentStringAsync(httpResponseMessage);

         var mediaType = httpResponseMessage.Content.Headers.ContentType.MediaType;
         // no need to fix json data
         if (mediaType.Equals("application/json"))
         {
            return responseString;
         }

         var sb = new StringBuilder(responseString);

         FixUrls(sb);

         return sb.ToString();
      }
   }
}