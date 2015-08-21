using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace TM.UI.MVC.Helpers
{
   public static class EnumExtensions
   {
      public static IHtmlString GetDisplayName(this Enum enumValue)
      {
         var displayAttribute = enumValue.GetType()
            .GetMember(enumValue.ToString())
            .First()
            .GetCustomAttributes<DisplayAttribute>().SingleOrDefault();

         MvcHtmlString displayName;
         if (displayAttribute == null)
         {
            displayName = MvcHtmlString.Create(enumValue.ToString());
         }
         else
         {
            displayName = MvcHtmlString.Create(displayAttribute.GetName());
         }
         return displayName;
      }
   }
}