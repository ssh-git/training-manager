using System.Web.Mvc;
using System.Web.Routing;

namespace TM.UI.MVC.ControllableModel
{
   public interface IControllableViewModelParams : IPageParam, ISearchParam, ISortParam, IFilterParam
   {
      RouteValueDictionary ToRouteValueDictionary();
      RouteValueDictionary ToRouteValueDictionary(object sortKeyState);
      RouteValueDictionary ToRouteValueDictionary(int pageNumber);

      ControllableViewModelParams ToRouteValueObject(int pageNumber);
      ControllableViewModelParams ToRouteValueObject(object sortKeyState);

      MvcHtmlString ToHiddenFields();
      MvcHtmlString ToHiddenFields(object sortKeyState);
   }
  

   public interface ISearchParam
   {
      string SearchKey { get; }
   }

   public interface ISortParam
   {
      string SortKey { get; }
   }

   public interface IFilterParam
   {
      string FilterKey { get; }
   }

   public interface IPageParam
   {
      string Page { get; set; }
      string PageSize { get; set; }
   }
}