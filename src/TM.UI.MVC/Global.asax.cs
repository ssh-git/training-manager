using System.Data.Entity;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Serilog;
using TM.Data;
using TM.Data.Update;

namespace TM.UI.MVC
{
   public class MvcApplication : HttpApplication
   {
      public static Timer Timer;

      protected void Application_Start()
      {
         LoggerConfig.RegisterLogger();

         Log.Information("Application start");

         AreaRegistration.RegisterAllAreas();
         FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
         RouteConfig.RegisterRoutes(RouteTable.Routes);
         BundleConfig.RegisterBundles(BundleTable.Bundles);

         Database.SetInitializer<CatalogDbContext>(null);
         Database.SetInitializer<UpdateDbContext>(null);
         Database.SetInitializer<IdentityDbContext>(null);
      }

      protected void Application_End()
      {
         Log.Information("Application shutdown");
      }
   }
}
