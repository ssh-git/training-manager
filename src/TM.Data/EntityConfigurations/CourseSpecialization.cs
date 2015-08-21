using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace TM.Data
{
   internal class CourseSpecializationConfiguration : EntityTypeConfiguration<CourseSpecialization>
   {
      public CourseSpecializationConfiguration()
      {
         HasKey(x => new { x.CourseId, x.Specialization });

         Property(x => x.Specialization)
            .HasColumnAnnotation(IndexAnnotation.AnnotationName, new IndexAnnotation(new IndexAttribute("NCI_Specialization")));
      }
   }
}
