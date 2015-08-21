using System;
using System.Data.Entity.Migrations;
using System.Linq;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using TM.Shared;

namespace TM.UI.MVC.Migrations
{
   internal sealed class Configuration : DbMigrationsConfiguration<IdentityDbContext>
   {
      public Configuration()
      {
         AutomaticMigrationsEnabled = false;
      }

      protected override void Seed(IdentityDbContext context)
      {
         const string adminRoleName = AppConstants.UserRole.Administrator;
         using (var roleManager = new ApplicationRoleManager(new RoleStore<ApplicationRole>(context)))
         {
            var adminRole = roleManager.FindByName(adminRoleName);

            if (adminRole == null)
            {
               roleManager.Create(new ApplicationRole(adminRoleName));
            }

            // if admin already exists no need to create default admin
            if (adminRole != null && adminRole.Users.Any())
            {
               return;
            }

         }


         using (var userManager = new ApplicationUserManager(new UserStore<ApplicationUser>(context)))
         {
            userManager.PasswordValidator = new PasswordValidator(); // default validator (any password valid)

            userManager.UserValidator = new UserValidator<ApplicationUser>(userManager)
            {
               AllowOnlyAlphanumericUserNames = false,
               RequireUniqueEmail = false
            };

            const string userName = "admin";
            const string userPassword = "admin";
            var user = userManager.FindByName(userName);

            if (user != null) return;

            user = new ApplicationUser
            {
               UserName = userName,
               LockoutEnabled = true,
               SpecializationsValue = Specializations.AllSpecializations,
               RegisteredOnUtc = DateTime.UtcNow
            };

            userManager.Create(user, userPassword);

            userManager.AddToRole(user.Id, adminRoleName);
         }
      }
   }
}
