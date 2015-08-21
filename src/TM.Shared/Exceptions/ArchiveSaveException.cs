using System;
using System.Runtime.Serialization;

namespace TM.Shared
{
   [Serializable]
   public class ArchiveSaveException : Exception
   {
      public ArchiveSaveException()
      {
      }

      public ArchiveSaveException(string message) : base(message)
      {
      }

      public ArchiveSaveException(string message, Exception inner) : base(message, inner)
      {
      }

      /// <exception cref="ArgumentNullException">The <paramref name="info" /> parameter is null. </exception>
      /// <exception cref="SerializationException">The class name is null or <see cref="P:System.Exception.HResult" /> is zero (0). </exception>
      protected ArchiveSaveException(SerializationInfo info,StreamingContext context) 
         : base(info, context)
      {
      }
   }
}