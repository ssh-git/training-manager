﻿using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace TM.Shared
{
   public static class AsyncHelper
   {
      private static readonly TaskFactory _myTaskFactory = new TaskFactory(CancellationToken.None,
          TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default);

      public static TResult RunSync<TResult>(Func<Task<TResult>> func)
      {
         var cultureUi = CultureInfo.CurrentUICulture;
         var culture = CultureInfo.CurrentCulture;
         return _myTaskFactory.StartNew(() =>
         {
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = cultureUi;
            return func();
         }).Unwrap().GetAwaiter().GetResult();
      }

      public static void RunSync(Func<Task> func)
      {
         var culture = CultureInfo.CurrentCulture;
         var cultureUi = CultureInfo.CurrentUICulture;
         _myTaskFactory.StartNew(() =>
         {
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = cultureUi;
            return func();
         }).Unwrap().GetAwaiter().GetResult();
      }
   }
}
