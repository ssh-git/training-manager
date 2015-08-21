using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Diagnostics.CodeAnalysis;

namespace TM.Data
{
   public interface IDbContextTestHelper
   {
      void SetStateToModified(object entity);
      void SetStateToAdded(object entity);
      void SetStateToDeleted(object entity);
      void SetStateToDetached(object entity);
   }

   [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
   [DbConfigurationType(typeof(TrainingManagerDbConfiguration))]
   public class CatalogDbContext : DbContext, IDbContextTestHelper
   {
      public CatalogDbContext()
         : this("name=TrainingManagerDb")
      {
      }

      public CatalogDbContext(string nameOrConnectionstring)
         : base(nameOrConnectionstring)
      {
         Configuration.LazyLoadingEnabled = false;
      }

      public virtual void SetStateToModified(object entity)
      {
         Entry(entity).State = EntityState.Modified;
      }

      public virtual void SetStateToAdded(object entity)
      {
         Entry(entity).State = EntityState.Added;
      }

      public virtual void SetStateToDeleted(object entity)
      {
         Entry(entity).State = EntityState.Deleted;
      }

      public virtual void SetStateToDetached(object entity)
      {
         Entry(entity).State = EntityState.Detached;
      }


      public virtual DbSet<TrainingProvider> TrainingProviders { get; set; }
      public virtual DbSet<Category> Categories { get; set; }
      public virtual DbSet<Course> Courses { get; set; }
      public virtual DbSet<CourseSpecialization> CourseSpecializations { get; set; }
      public virtual DbSet<TrainingProviderAuthor> TrainingProviderAuthors { get; set; }
      public virtual DbSet<CourseAuthor> CourseAuthors { get; set; }
      public virtual DbSet<Author> Authors { get; set; }
      public virtual DbSet<Subscription> Subscriptions { get; set; }


      protected override void OnModelCreating(DbModelBuilder modelBuilder)
      {
         base.OnModelCreating(modelBuilder);
         modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

         modelBuilder.HasDefaultSchema("Catalog");

         modelBuilder.Properties()
           .Where(p => p.Name == "Id")
           .Configure(p => p.HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity));

         modelBuilder.Properties()
            .Where(p => p.Name == "RowVersion")
            .Configure(p => p.IsRowVersion());

         modelBuilder.Entity<CourseAuthor>()
            .HasKey(x => new { x.CourseId, x.AuthorId });

         modelBuilder.Entity<Module>()
            .ToTable("ToC_Module")
            .Property(x => x.Duration)
            .HasColumnType("time")
            .HasPrecision(0);

         modelBuilder.Entity<Topic>()
            .ToTable("ToC_Topic")
            .Property(x => x.Duration)
            .HasColumnType("time")
            .HasPrecision(0);

         modelBuilder.Configurations.Add(new TrainingProviderConfiguration());
         modelBuilder.Configurations.Add(new CategoryConfiguration());
         modelBuilder.Configurations.Add(new CourseConfiguration());
         modelBuilder.Configurations.Add(new CourseSpecializationConfiguration());
         modelBuilder.Configurations.Add(new CourseRatingConfiguration());
         modelBuilder.Configurations.Add(new TrainingProviderAuthorConfiguration());
         modelBuilder.Configurations.Add(new SubscriptionConfiguration());
      }
   }
}
