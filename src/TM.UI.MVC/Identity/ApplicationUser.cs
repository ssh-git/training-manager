using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using TM.Data;
using TM.Shared;
using WebGrease.Css.Extensions;

namespace TM.UI.MVC
{
   public class ApplicationUser : IdentityUser
   {
      public DateTime RegisteredOnUtc { get; set; }
      public DateTime LastLoginOnUtc { get; set; }

      public Specializations SpecializationsValue
      {
         get
         {
            if (SpecializationList == null)
            {
               return Specializations.None;
            }

            var specializations = SpecializationList
               .Aggregate(default(Specializations),
                  (accumulator, courseSpecialization) =>
                     accumulator | courseSpecialization.Specialization);

            return specializations;
         }

         set
         {
            var listToSet = value.GetFlags().Select(x => new UserSpecializations
               {
                  UserId = Id,
                  Specialization = x
               }).ToList();

            if (SpecializationList == null || SpecializationList.Count == 0)
            {
               SpecializationList = listToSet;
            } else
            {
               var deletedItems = SpecializationList.Except(listToSet, UserSpecializations.SpecializationComparer);
               var newItems = listToSet.Except(SpecializationList, UserSpecializations.SpecializationComparer);

               deletedItems.ForEach(x => SpecializationList.Remove(x));
               newItems.ForEach(x => SpecializationList.Add(x));
            }
         }
      }

      public virtual ICollection<UserSpecializations> SpecializationList { get; set; }

      public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
      {
         // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
         var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);

         return userIdentity;
      }
   }
}