using System;
using System.Runtime.Serialization;

namespace TM.Data.Update
{
   [Serializable]
   public class TrainingCatalogUpdateException : Exception
   {
      public TrainingCatalogUpdateException()
      {
      }

      public TrainingCatalogUpdateException(string message) : base(message)
      {
      }

      public TrainingCatalogUpdateException(string message, Exception inner) : base(message, inner)
      {
      }

      /// <exception cref="ArgumentNullException">The <paramref name="info" /> parameter is null. </exception>
      /// <exception cref="SerializationException">The class name is null or <see cref="P:System.Exception.HResult" /> is zero (0). </exception>
      protected TrainingCatalogUpdateException(SerializationInfo info,StreamingContext context) 
         : base(info, context)
      {
      }
   }
}