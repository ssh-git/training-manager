using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using TM.Data;
using TM.Shared;
using TM.UI.MVC.Infrastructure;

namespace TM.UI.MVC
{
   public class ApplicationUserManager : UserManager<ApplicationUser>
   {
      private bool _disposed;


      #region Constructors and Factories

      public ApplicationUserManager(IUserStore<ApplicationUser> store)
         : base(store)
      {
      }

      public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options,
         IOwinContext context)
      {
         var manager = new ApplicationUserManager(new UserStore<ApplicationUser>(context.Get<IdentityDbContext>()));
         // Configure validation logic for usernames
         manager.UserValidator = new UserValidator<ApplicationUser>(manager)
         {
            AllowOnlyAlphanumericUserNames = true,
            RequireUniqueEmail = false
         };

         // Configure validation logic for passwords
         manager.PasswordValidator = new PasswordValidator
         {
            RequiredLength = 6,
            RequireNonLetterOrDigit = false,
            RequireDigit = false,
            RequireLowercase = false,
            RequireUppercase = false
         };

         // Configure user lockout defaults
         manager.UserLockoutEnabledByDefault = true;
         manager.DefaultAccountLockoutTimeSpan = TimeSpan.FromMinutes(5.0);
         manager.MaxFailedAccessAttemptsBeforeLockout = 5;

         // Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
         // You can write your own provider and plug it in here.
         manager.RegisterTwoFactorProvider("Phone Code", new PhoneNumberTokenProvider<ApplicationUser>
         {
            MessageFormat = "Your security code is {0}"
         });
         manager.RegisterTwoFactorProvider("Email Code", new EmailTokenProvider<ApplicationUser>
         {
            Subject = "Security Code",
            BodyFormat = "Your security code is {0}"
         });
         manager.EmailService = new EmailService();
         manager.SmsService = new SmsService();
         var dataProtectionProvider = options.DataProtectionProvider;
         if (dataProtectionProvider != null)
         {
            manager.UserTokenProvider =
               new DataProtectorTokenProvider<ApplicationUser>(dataProtectionProvider.Create("ASP.NET Identity"));
         }
         return manager;
      }

      #endregion


      private IdentityDbContext Context
      {
         get
         {
            return ((IdentityDbContext)((UserStore<ApplicationUser>)Store).Context);
         }
      }

      public Specializations GetUserSpecializations(string userId)
      {
         return AsyncHelper.RunSync(() => GetUserSpecializationsAsync(userId));
      }

      public async Task<Specializations> GetUserSpecializationsAsync(string userId)
      {
         ThrowIfDisposed();

         var specializationsList = await Context.Specializations.Where(x => x.UserId == userId)
            .Select(x => x.Specialization)
            .ToListAsync();

         var specializations = specializationsList
            .Aggregate(default(Specializations),
               (accumulator, specialization) =>
                  accumulator | specialization);

         return specializations;
      }



      public IdentityResult SetUserSpecializations(string userId, Specializations selectedSpecializations)
      {
         return AsyncHelper.RunSync(() => SetUserSpecializationsAsync(userId, selectedSpecializations));
      }

      public async Task<IdentityResult> SetUserSpecializationsAsync(string userId, Specializations selectedSpecializations)
      {
         ThrowIfDisposed();

         var currentSpecializationList = await Context.Specializations
            .Where(x => string.Equals(x.UserId, userId))
            .ToListAsync();

         var selectedSpecializationsList = selectedSpecializations.GetFlags()
               .Select(x => new UserSpecializations
               {
                  UserId = userId,
                  Specialization = x
               });

         if (!currentSpecializationList.Any())
         {
            if (!Users.Any(x => string.Equals(x.Id, userId)))
            {
               return new IdentityResult(string.Format("User with Id = {0} not found", userId));
            }

            Context.Specializations.AddRange(selectedSpecializationsList);
         } else
         {
            var deletedSpecializations = currentSpecializationList
            .Except(selectedSpecializationsList, UserSpecializations.SpecializationComparer);

            var newSpecializations = selectedSpecializationsList
               .Except(currentSpecializationList, UserSpecializations.SpecializationComparer);

            Context.Specializations.RemoveRange(deletedSpecializations);
            Context.Specializations.AddRange(newSpecializations);
         }

         await Context.SaveChangesAsync();

         return IdentityResult.Success;
      }


      public async Task<IdentityResult> SaveUserLoginDateAsync(string userNickname)
      {
         var user = await FindByNameAsync(userNickname);

         user.LastLoginOnUtc = DateTime.UtcNow;

         await Context.SaveChangesAsync();

         return IdentityResult.Success;
      }

      private void ThrowIfDisposed()
      {
         if (_disposed)
         {
            throw new ObjectDisposedException(GetType().Name);
         }
      }

      protected override void Dispose(bool disposing)
      {
         if (disposing)
         {
            _disposed = true;
         }

         base.Dispose(disposing);
      }
   }
}