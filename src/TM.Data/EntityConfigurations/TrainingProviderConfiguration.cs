using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace TM.Data
{
   internal class TrainingProviderConfiguration : EntityTypeConfiguration<TrainingProvider>
   {
      public TrainingProviderConfiguration()
      {
         Ignore(x => x.UpdateFrequency);
         Ignore(x => x.AllowedUpdateUtcHours);

         Property(x => x.Name)
            .HasColumnAnnotation(IndexAnnotation.AnnotationName,
               new IndexAnnotation(new IndexAttribute("NCI_Name", 0)
               {
                  IsUnique = true
               }));

         HasMany(x => x.Courses)
            .WithRequired(x => x.TrainingProvider)
            .WillCascadeOnDelete(false);
      }
   }
}
