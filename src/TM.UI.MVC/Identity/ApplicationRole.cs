using Microsoft.AspNet.Identity.EntityFramework;

namespace TM.UI.MVC
{
   public class ApplicationRole:IdentityRole
   {
      public ApplicationRole()
      {
            
      }

      public ApplicationRole(string name):base(name)
      {
            
      }
   }
}