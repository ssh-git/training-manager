using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Security;
using Microsoft.AspNet.Identity.EntityFramework;

namespace TM.UI.MVC
{
   public class IdentityDbContext : IdentityDbContext<ApplicationUser>
   {
      public IdentityDbContext()
         : base("TrainingManagerDb", throwIfV1Schema: false)
      {
      }

      public virtual DbSet<UserSpecializations> Specializations { get; set; }

      protected override void OnModelCreating(DbModelBuilder builder)
      {
         base.OnModelCreating(builder);

         Configuration.LazyLoadingEnabled = true;

         builder.HasDefaultSchema("Identity");

         builder.Properties()
            .Where(p => p.Name == "RowVersion")
            .Configure(p => p.IsRowVersion());

         builder.Conventions.Remove<PluralizingTableNameConvention>();

         builder.Entity<ApplicationUser>()
            .Property(x => x.RegisteredOnUtc)
            .IsRequired()
            .HasColumnType("datetime2")
            .HasPrecision(0);

         builder.Entity<ApplicationUser>()
            .Property(x => x.LastLoginOnUtc)
            .IsRequired()
            .HasColumnType("datetime2")
            .HasPrecision(0);

         builder.Entity<ApplicationUser>()
            .Ignore(x => x.SpecializationsValue)
            .HasMany(x => x.SpecializationList)
            .WithRequired()
            .HasForeignKey(x => x.UserId);


         builder.Entity<UserSpecializations>()
            .HasKey(x => new { x.UserId, x.Specialization })
            .Property(x => x.UserId)
            .HasMaxLength(128);

      }

      public static IdentityDbContext Create()
      {
         return new IdentityDbContext();
      }

#if DEBUG

      /// <exception cref="SecurityException">The <see cref="T:System.Security.Permissions.UIPermission" /> is not set to break into the debugger. </exception>
      protected override DbEntityValidationResult ValidateEntity(DbEntityEntry entityEntry, IDictionary<object, object> items)
      {
         var result = base.ValidateEntity(entityEntry, items);
         if (!result.IsValid)
         {
            Debugger.Break();
         }
         return result;
      }

#endif
   }
}