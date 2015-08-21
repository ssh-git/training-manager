using System;
using System.Runtime.Serialization;

namespace TM.Shared.HtmlContainer
{
   [Serializable]
   public class HtmlLoadException : Exception
   {
      public HtmlLoadException()
      {
      }

      public HtmlLoadException(string message) : base(message)
      {
      }

      public HtmlLoadException(string message, Exception inner) : base(message, inner)
      {
      }

      /// <exception cref="SerializationException">The class name is null or <see cref="P:System.Exception.HResult" /> is zero (0). </exception>
      /// <exception cref="ArgumentNullException">The <paramref name="info" /> parameter is null. </exception>
      protected HtmlLoadException(SerializationInfo info,StreamingContext context) 
         : base(info, context)
      {
      }
   }
}