using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Security;

namespace TM.Data.Update
{
   [SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
   [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
   [DbConfigurationType(typeof(TrainingManagerDbConfiguration))]
   public class UpdateDbContext : CatalogDbContext
   {
      public UpdateDbContext()
         : this("name=TrainingManagerDb")
      {
      }

      public UpdateDbContext(string nameOrConnectionstring)
         : base(nameOrConnectionstring)
      {
         Configuration.LazyLoadingEnabled = false;
      }

      public virtual DbSet<UpdateEvent> UpdateEvents { get; set; }

      public virtual DbSet<CategoryUpdate> CategoriesUpdates { get; set; }
      public virtual DbSet<CategoryBackup> CategoriesBackups { get; set; }

      public virtual DbSet<CourseUpdate> CoursesUpdates { get; set; }
      public virtual DbSet<CourseBackup> CoursesBackups { get; set; }

      public virtual DbSet<AuthorUpdate> AuthorsUpdates { get; set; }
      public virtual DbSet<AuthorBackup> AuthorsBackups { get; set; }

      public virtual DbSet<AuthorResolve> AuthorsResolves { get; set; }


      protected override void OnModelCreating(DbModelBuilder modelBuilder)
      {
         base.OnModelCreating(modelBuilder);

         modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

         modelBuilder.Entity<UpdateEvent>()
            .ToTable("UpdateEvent", "Update")
            .HasRequired(x => x.TrainingProvider)
            .WithMany()
            .WillCascadeOnDelete(false);

         modelBuilder.Entity<UpdateEvent>()
            .Property(x => x.StartedOn)
            .HasColumnType("datetime2")
            .HasPrecision(0);

         modelBuilder.Entity<UpdateEvent>()
            .Property(x => x.EndedOn)
            .HasColumnType("datetime2")
            .HasPrecision(0);


         modelBuilder.Entity<AuthorResolve>()
            .ToTable("AuthorResolve", "Update")
            .HasOptional(x => x.ResolvedAuthor)
            .WithMany()
            .WillCascadeOnDelete(true);

         modelBuilder.Entity<AuthorResolve>()
            .HasRequired(x => x.TrainingProvider)
            .WithMany()
            .WillCascadeOnDelete(false);


         modelBuilder.Entity<AuthorUpdate>()
            .ToTable("AuthorUpdate", "Update")
            .HasKey(x => new {x.UpdateEventId, x.TrainingProviderId, x.AuthorId})
            .HasOptional(x => x.AuthorBackup)
            .WithRequired(x => x.AuthorUpdate)
            .WillCascadeOnDelete(true);

         modelBuilder.Entity<AuthorUpdate>()
            .HasRequired(x => x.TrainingProviderAuthor)
            .WithMany()
            .WillCascadeOnDelete(true);

         modelBuilder.Entity<AuthorBackup>()
            .ToTable("AuthorBackup", "Update")
            .HasKey(x => new {x.UpdateEventId, x.TrainingProviderId, x.AuthorId});


         modelBuilder.Entity<CategoryUpdate>()
            .ToTable("CategoryUpdate", "Update")
            .HasKey(x => new {x.UpdateEventId, x.CategoryId})
            .HasOptional(x => x.CategoryBackup)
            .WithRequired(x => x.CategoryUpdate)
            .WillCascadeOnDelete(true);

         modelBuilder.Entity<CategoryBackup>()
            .ToTable("CategoryBackup", "Update")
            .HasKey(x => new {x.UpdateEventId, x.CategoryId});


         modelBuilder.Entity<CourseUpdate>()
            .ToTable("CourseUpdate", "Update")
            .HasKey(x => new {x.UpdateEventId, x.CourseId})
            .HasOptional(x => x.CourseBackup)
            .WithRequired(x => x.CourseUpdate)
            .WillCascadeOnDelete(true);

         modelBuilder.Entity<CourseBackup>()
            .ToTable("CourseBackup", "Update")
            .HasKey(x => new { x.UpdateEventId, x.CourseId })
            .Property(x => x.Duration)
            .IsOptional()
            .HasColumnType("time")
            .HasPrecision(0);

         modelBuilder.Entity<CourseBackup>()
            .Property(x => x.ReleaseDate)
            .IsOptional()
            .HasColumnType("date");
         

         modelBuilder.Entity<CourseBackup>()
            .HasOptional(x => x.Category)
            .WithMany()
            .WillCascadeOnDelete(false);

         modelBuilder.Entity<CourseBackup>()
            .HasMany(x => x.CourseAuthorBackups)
            .WithRequired(x => x.CourseBackup);


         modelBuilder.Entity<CourseAuthorBackup>()
            .ToTable("CourseAuthorBackup", "Update")
            .HasKey(x => new {x.UpdateEventId, x.CourseId, x.AuthorId})
            .HasRequired(x => x.TrainingProviderAuthor)
            .WithMany()
            .HasForeignKey(x =>  new{x.TrainingProviderId, x.AuthorId})
            .WillCascadeOnDelete(false);
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