using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace TM.Data
{
   internal class CourseConfiguration : EntityTypeConfiguration<Course>
   {
      public CourseConfiguration()
      {
         Property(x => x.UrlName)
            .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute("NCI_UrlName_TrainingProviderId", 0)
            {
               IsUnique = true
            }));

         Property(x => x.TrainingProviderId)
            .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute("NCI_UrlName_TrainingProviderId", 1)
            {
               IsUnique = true
            }));

         Property(x => x.Duration)
            .HasColumnType("time")
            .HasPrecision(0);

         Property(x => x.ReleaseDate)
            .HasColumnType("date");

         Ignore(x => x.Specializations);
      }
   }
}
