using System;
using System.Diagnostics.CodeAnalysis;
using System.Web;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Ninject;
using Ninject.Web.Common;
using TM.UI.MVC;
using TM.UI.MVC.Infrastructure;
using WebActivatorEx;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(NinjectWebCommon), "Start")]
[assembly: ApplicationShutdownMethod(typeof(NinjectWebCommon), "Stop")]

namespace TM.UI.MVC
{
   [SuppressMessage("ReSharper", "UnusedMember.Global")]
   [SuppressMessage("ReSharper", "CatchAllClause")]
   [SuppressMessage("ReSharper", "ThrowingSystemException")]
   public static class NinjectWebCommon
   {
      private static readonly Bootstrapper Bootstrapper = new Bootstrapper();

      /// <summary>
      /// Starts the application
      /// </summary>
      public static void Start()
      {
         DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
         DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
         Bootstrapper.Initialize(CreateKernel);
      }

      /// <summary>
      /// Stops the application.
      /// </summary>
      public static void Stop()
      {
         Bootstrapper.ShutDown();
      }

      /// <summary>
      /// Creates the kernel that will manage your application.
      /// </summary>
      /// <returns>The created kernel.</returns>
      private static IKernel CreateKernel()
      {
         var kernel = new StandardKernel();
         try {
            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

            NinjectMappings.Register(kernel);

            return kernel;
         }
         catch {
            kernel.Dispose();
            throw;
         }
      }
   }
}
