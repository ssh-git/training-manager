using Microsoft.Owin;
using Owin;
using TM.UI.MVC;

[assembly: OwinStartup(typeof(Startup))]
namespace TM.UI.MVC
{
   public partial class Startup
   {
      public void Configuration(IAppBuilder app)
      {
         ConfigureAuth(app);
         ConfigureHangFire(app);
      }
   }
}
