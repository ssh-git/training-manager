using System.Diagnostics;

namespace TM.Data.Pluralsight
{

   [DebuggerDisplay("Url = {Url}")]
   public class Sketch
   {
      public string Url { get; set; }
      public string FileName { get; set; }
   }
}
