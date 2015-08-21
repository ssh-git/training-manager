using System;
using System.Web.Mvc;
using TM.UI.MVC.ControllableModel;

namespace TM.UI.MVC.Helpers
{
   public static class ViewDataDictionaryExtensions
   {
      public const string ControllableViewModelParamsViewDataKey = "ControllableViewModelParams";

      /// <exception cref="ArgumentNullException"><paramref name="viewData"/> or
      /// <paramref name="viewModelParams"/> is <see langword="null" />.</exception>
      /// <exception cref="InvalidOperationException">attempt to assign value to the existing <paramref name="viewData"/> key</exception>
      public static void SetControllableViewModelParams(this ViewDataDictionary viewData,
         IControllableViewModelParams viewModelParams)
      {
         if (viewData == null)
            throw new ArgumentNullException("viewData");

         if (viewModelParams == null)
            throw new ArgumentNullException("viewModelParams");

         if (viewData.ContainsKey(ControllableViewModelParamsViewDataKey))
            throw new InvalidOperationException("attempt to assign value to the existing viewDataDictionary key");

         viewData[ControllableViewModelParamsViewDataKey] = viewModelParams;
      }


      /// <exception cref="ArgumentNullException"><paramref name="viewData"/> is <see langword="null" />.</exception>
      public static IControllableViewModelParams GetControllableViewModelParams(this ViewDataDictionary viewData)
      {
         if (viewData == null)
            throw new ArgumentNullException("viewData");

         return (IControllableViewModelParams)viewData[ControllableViewModelParamsViewDataKey];
      }
   }
}