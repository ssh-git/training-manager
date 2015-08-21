using System.Diagnostics.CodeAnalysis;
using System.Net.Http;

namespace TM.Shared.DownloadManager
{
   [SuppressMessage("ReSharper", "IntroduceOptionalParameters.Global")]
   public class DownloadResult<TResult> where TResult : class
   {
      public DownloadResult(TResult result)
         : this(result, true, null, null) { }

      public DownloadResult(HttpRequestMessage httpRequestMessage, HttpResponseMessage httpResponseMessage)
         : this(null, false, httpRequestMessage, httpResponseMessage) { }

      public DownloadResult(TResult result, bool isSuccess,
         HttpRequestMessage httpRequestMessage, HttpResponseMessage httpResponseMessage)
      {
         Result = result;
         IsSuccess = isSuccess;
         HttpRequestMessage = httpRequestMessage;
         HttpResponseMessage = httpResponseMessage;
      }

      public TResult Result { get; private set; }
      public bool IsSuccess { get; private set; }
      public HttpRequestMessage HttpRequestMessage { get; private set; }
      public HttpResponseMessage HttpResponseMessage { get; private set; }
   }
}