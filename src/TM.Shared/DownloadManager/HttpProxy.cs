using System;
using System.Configuration;
using System.Net;
using TM.Shared.Properties;

namespace TM.Shared.DownloadManager
{
   public class HttpProxy : ProxyBase
   {
      /// <exception cref="ConfigurationErrorsException">Configuration appSettings section does not contains HttpProxy settings.</exception>
      public HttpProxy()
      {
         var hostSettings = ConfigurationManager.AppSettings["TM.HttpPoxy.Host"];
         if (hostSettings == null)
         {
            throw new ConfigurationErrorsException(Resources.ConfigurationEntyNotFound_HttpProxyHost);
         }

         var portSettings = ConfigurationManager.AppSettings["TM.HttpPoxy.Port"];
         if (portSettings == null)
         {
            throw new ConfigurationErrorsException(Resources.ConfigurationEntyNotFound_HttpProxyPort);
         }

         var bypassOnLocalSettings = ConfigurationManager.AppSettings["TM.HttpPoxy.BypassOnLocal"] ?? bool.FalseString;

         HttpClientHandler.Proxy = new WebProxy(hostSettings, Convert.ToInt32(portSettings))
         {
            BypassProxyOnLocal = Convert.ToBoolean(bypassOnLocalSettings)
         };
      }


      /// <exception cref="ArgumentNullException"><paramref name="host"/> is <see langword="null" />.</exception>
      public HttpProxy(string host, int port, bool bypassOnLocal = false)
      {
         if (host == null)
            throw new ArgumentNullException("host");
         
         HttpClientHandler.Proxy = new WebProxy(host, port)
         {
            BypassProxyOnLocal = bypassOnLocal
         };

         HttpClientHandler.UseProxy = true;
      }
   }
}