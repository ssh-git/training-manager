namespace TM.UI.MVC.ControllableModel
{
   public class PageableViewModelParams :IPageParam
   {
      public string Page { get; set; }
      public string PageSize { get; set; }
   }
}