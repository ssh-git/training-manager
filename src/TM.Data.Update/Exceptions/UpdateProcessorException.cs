using System;
using System.Runtime.Serialization;

namespace TM.Data.Update
{
   [Serializable]
   public class UpdateProcessorException : Exception
   {
      public UpdateProcessorException()
      {
      }

      public UpdateProcessorException(string message) : base(message)
      {
      }

      public UpdateProcessorException(string message, Exception inner) : base(message, inner)
      {
      }

      /// <exception cref="ArgumentNullException">The <paramref name="info" /> parameter is null. </exception>
      /// <exception cref="SerializationException">The class name is null or <see cref="P:System.Exception.HResult" /> is zero (0). </exception>
      protected UpdateProcessorException(SerializationInfo info,StreamingContext context) 
         :base(info, context)
      {
      }
   }
}