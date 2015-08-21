using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;

namespace TM.Data
{
   internal class TrainingProviderAuthorConfiguration : EntityTypeConfiguration<TrainingProviderAuthor>
   {
      public TrainingProviderAuthorConfiguration()
      {
         HasKey(x => new { x.TrainingProviderId, x.AuthorId });

         Property(x => x.UrlName)
            .HasColumnAnnotation(IndexAnnotation.AnnotationName,
               new IndexAnnotation(new IndexAttribute("NCI_UrlName_TrainingProviderId", 0)
               {
                  IsUnique = true
               }));

         Property(x => x.TrainingProviderId)
            .HasColumnAnnotation(IndexAnnotation.AnnotationName,
               new IndexAnnotation(new IndexAttribute("NCI_UrlName_TrainingProviderId", 1)
               {
                  IsUnique = true
               }));

         HasMany(x => x.AuthorCourses)
            .WithRequired(x => x.TrainingProviderAuthor)
            .WillCascadeOnDelete(false);
      }
   }
}
