using System;
using System.Runtime.Serialization;

namespace TM.Data.Update
{
   [Serializable]
   public class UpdateServiceException : Exception
   {
      public UpdateServiceException()
      {
      }

      public UpdateServiceException(string message)
         : base(message)
      {
      }

      public UpdateServiceException(string message, Exception inner)
         : base(message, inner)
      {
      }

      /// <exception cref="ArgumentNullException">The <paramref name="info" /> parameter is null. </exception>
      /// <exception cref="SerializationException">The class name is null or <see cref="P:System.Exception.HResult" /> is zero (0). </exception>
      protected UpdateServiceException(SerializationInfo info, StreamingContext context)
         : base(info, context)
      {
      }
   }
}