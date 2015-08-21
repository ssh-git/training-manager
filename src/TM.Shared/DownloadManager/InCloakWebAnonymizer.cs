using System;
using System.Configuration;
using System.Text;

namespace TM.Shared.DownloadManager
{
   public class InCloakWebAnonymizer : WebAnonymizer
   {
      /// <exception cref="ConfigurationErrorsException">Configuration appSettings section does not contains TM.WebAnonymizer.Host settings.</exception>
      public InCloakWebAnonymizer() { }

      /// <exception cref="ArgumentException"><paramref name="host" /> is <see langword="null" /> or whitespace..</exception>
      public InCloakWebAnonymizer(string host)
         : base(host) { }

      /// <exception cref="ObjectDisposedException"></exception>
      protected override Uri GenerateProxyUri(Uri sourceUri)
      {
         if (Disposed) { throw new ObjectDisposedException(GetType().FullName); }

         var uriBuilder = new UriBuilder(sourceUri);
         var resultHost = string.Concat(uriBuilder.Host, ".", Host);
         uriBuilder.Host = resultHost;
         return uriBuilder.Uri;
      }

      /// <exception cref="ObjectDisposedException"></exception>
      protected override void FixUrls(StringBuilder documentContainer)
      {
         if (Disposed) { throw new ObjectDisposedException(GetType().FullName); }

         var stringToReplace = string.Concat(".", Host);
         documentContainer.Replace(stringToReplace, string.Empty);
      }
   }
}