using System.Collections.Generic;

// ReSharper disable All

namespace TM.Data.Pluralsight.Json
{
   public class JsonModule
   {
      public string title { get; set; }
      public string description { get; set; }
      public string duration { get; set; }
      public List<JsonTopic> clips { get; set; }
   }
}