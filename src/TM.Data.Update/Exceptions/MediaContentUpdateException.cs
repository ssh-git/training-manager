using System;
using System.Runtime.Serialization;

namespace TM.Data.Update
{
   [Serializable]
   public class MediaContentUpdateException : Exception
   {
      public MediaContentUpdateException()
      {
      }

      public MediaContentUpdateException(string message) : base(message)
      {
      }

      public MediaContentUpdateException(string message, Exception inner) : base(message, inner)
      {
      }

      /// <exception cref="ArgumentNullException">The <paramref name="info" /> parameter is null. </exception>
      /// <exception cref="SerializationException">The class name is null or <see cref="P:System.Exception.HResult" /> is zero (0). </exception>
      protected MediaContentUpdateException(SerializationInfo info,StreamingContext context) 
         : base(info, context)
      {
      }
   }
}
