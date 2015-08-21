using System;
using System.Runtime.Serialization;

namespace TM.Shared.DownloadManager
{
   [Serializable]
   public class DownloadException<T> : Exception where T : class
   {
      public DownloadResult<T> DownloadResult { get; private set; }
      public DownloadException()
      {
      }

      public DownloadException(string message, DownloadResult<T> downloadResult)
         : base(message)
      {
         DownloadResult = downloadResult;
      }

      public DownloadException(string message, DownloadResult<T> downloadResult, Exception inner)
         : base(message, inner)
      {
         DownloadResult = downloadResult;
      }

      /// <exception cref="SerializationException">The class name is null or <see cref="P:System.Exception.HResult" /> is zero (0). </exception>
      /// <exception cref="ArgumentNullException">The <paramref name="info" /> parameter is null. </exception>
      protected DownloadException(SerializationInfo info,StreamingContext context)
         : base(info, context)
      {
      }
   }
}
