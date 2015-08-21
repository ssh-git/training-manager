using Ninject;
using Ninject.Extensions.Conventions;
using Ninject.Web.Common;
using TM.Data;
using TM.Data.Update;

namespace TM.UI.MVC.Infrastructure
{
   public class NinjectMappings
   {
      public static void Register(IKernel kernel)
      {
         kernel.Bind<CatalogDbContext>()
            .ToSelf()
            .InRequestScope();

         kernel.Bind<UpdateDbContext>()
            .ToSelf()
            .InRequestScope();

         kernel.Bind(
            bind =>
               bind.FromThisAssembly()
                  .SelectAllClasses()
                  .InheritedFrom<CatalogManagerBase>()
                  .BindToSelf()
                  .Configure(config => config.InRequestScope()));
      }
   }
}