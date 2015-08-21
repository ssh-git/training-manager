using System;
using System.Web.Routing;

namespace TM.UI.MVC.Helpers
{
   public static class RouteValueDictionaryExtensions
   {
      /// <exception cref="ArgumentNullException"><paramref name="routeValueDictionary"/> or
      /// <paramref name="dataToInclude"/> is <see langword="null" />.</exception>
      public static RouteValueDictionary Include(this RouteValueDictionary routeValueDictionary, object dataToInclude)
      {
         if(routeValueDictionary == null) 
            throw new ArgumentNullException("routeValueDictionary");

         if (dataToInclude == null) 
            throw new ArgumentNullException("dataToInclude");

         var resultDictionary = new RouteValueDictionary(routeValueDictionary);
         
         foreach (var entry in new RouteValueDictionary(dataToInclude))
         {
            resultDictionary.Add(entry.Key, entry.Value);
         }

         return resultDictionary;
      }
   }
}