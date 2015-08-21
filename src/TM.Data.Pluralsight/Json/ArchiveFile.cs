using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace TM.Data.Pluralsight.Json
{
   [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
   public class ArchiveFile<TValue>
   {
      public string TrainingProviderName { get; set; }
      public DateTime Date { get; set; }
      public Dictionary<string, TValue> Content { get; set; }
   }
}