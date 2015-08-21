using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;

namespace TM.UI.MVC.ControllableModel
{
   [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
   [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
   public class ControllableViewModelParams : IControllableViewModelParams
   {
      public string Page { get; set; }
      public string PageSize { get; set; }
      public string SearchKey { get; set; }
      public string SortKey { get; set; }
      public string FilterKey { get; set; }


      public ControllableViewModelParams ToRouteValueObject(int pageNumber)
      {
         var clone = ((ControllableViewModelParams)MemberwiseClone());
         clone.Page = pageNumber.ToString();

         return clone;
      }

      public ControllableViewModelParams ToRouteValueObject(object sortKeyState)
      {
         var clone = ((ControllableViewModelParams)MemberwiseClone());
         clone.SortKey = sortKeyState == null ? null : sortKeyState.ToString();

         return clone;
      }


      public RouteValueDictionary ToRouteValueDictionary()
      {
         return new RouteValueDictionary(this);
      }

      public RouteValueDictionary ToRouteValueDictionary(int pageNumber)
      {
         return new RouteValueDictionary(ToRouteValueObject(pageNumber));
      }

      public RouteValueDictionary ToRouteValueDictionary(object sortKeyState)
      {
         return new RouteValueDictionary(ToRouteValueObject(sortKeyState));
      }


      public MvcHtmlString ToHiddenFields()
      {
         return ToHiddenFields(SortKey);
      }

      public MvcHtmlString ToHiddenFields(object sortKeyState)
      {
         var template = "<input type='hidden' name='{0}' value='{1}'/>";

         var builder = new StringBuilder();

         foreach (var field in ToRouteValueDictionary(sortKeyState))
         {
            builder.AppendFormat(template, field.Key, field.Value);
         }

         var html = MvcHtmlString.Create(builder.ToString());

         return html;
      }
   }
}