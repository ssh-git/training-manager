using System.Collections.Generic;
using System.Web.Mvc;

namespace TM.UI.MVC.ControllableModel
{
   public interface IControllableView
   {
      IControllableViewModelParams Params { get; }
      IEnumerable<SelectListItem> FilterSelectList { get; }
      IEnumerable<SelectListItem> PageSizeSelectList { get; }
      string SearchBoxTextPlaceholder { get; }
   }
}