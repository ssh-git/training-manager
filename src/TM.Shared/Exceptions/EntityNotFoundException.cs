﻿using System;
using System.Runtime.Serialization;

namespace TM.Shared
{
   [Serializable]
   public class EntityNotFoundException : Exception
   {
      public EntityNotFoundException()
      {
      }

      public EntityNotFoundException(string message) : base(message)
      {
      }

      public EntityNotFoundException(string message, Exception inner) : base(message, inner)
      {
      }

      protected EntityNotFoundException(SerializationInfo info,StreamingContext context) 
         : base(info, context)
      {
      }
   }
}
