using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace TM.Data
{
   internal class CategoryConfiguration : EntityTypeConfiguration<Category>
   {
      public CategoryConfiguration()
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
      }
   }
}
