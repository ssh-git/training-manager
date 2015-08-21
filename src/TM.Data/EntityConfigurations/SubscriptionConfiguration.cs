using System.Data.Entity.ModelConfiguration;

namespace TM.Data
{
   internal class SubscriptionConfiguration: EntityTypeConfiguration<Subscription>
   {
      public SubscriptionConfiguration()
      {
         HasKey(x => new {x.UserId, x.CourseId});

         Property(x => x.UserId)
            .HasMaxLength(128);

         Property(x => x.AddDate)
            .HasColumnType("date");

         Property(x => x.StartDate)
            .HasColumnType("date");

         Property(x => x.FinishDate)
            .HasColumnType("date");
      }
   }
}
