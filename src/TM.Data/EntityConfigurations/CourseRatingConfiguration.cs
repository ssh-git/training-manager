using System.Data.Entity.ModelConfiguration;
using TM.Shared;

namespace TM.Data
{
   internal class CourseRatingConfiguration : EntityTypeConfiguration<CourseRating>
   {
      public CourseRatingConfiguration()
      {
         Property(x => x.Rating)
            .HasColumnType("decimal")
            .HasPrecision(3, 2);
      }
   }
}
