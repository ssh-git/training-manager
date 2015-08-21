using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace TM.UI.MVC.Models
{
   [SuppressMessage("ReSharper", "InconsistentNaming")]
   public class NavigationViewModel
   {
      public const string ALLToken = "All";
      public string SelectedToken { get; set; }
      public List<string> TokenCatalog { get; set; }
   }
}