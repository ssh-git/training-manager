using TM.Data.Update;

namespace TM.Data.Pluralsight
{
   internal class PluralsightCategoryBackupCreator : IBackupCreator<Category, PluralsightCategory, CategoryBackup>
   {

      public bool IsDifferent(Category existingCategory, PluralsightCategory processingCategory)
      {
         return existingCategory.Title != processingCategory.Title ||
                existingCategory.LogoUrl != processingCategory.LogoUrl ||
                existingCategory.LogoFileName != processingCategory.LogoFileName ||
                existingCategory.IsDeleted; 
      }

      public CategoryBackup ApplayChanges(int updateEventId, Category existingCategory, PluralsightCategory processingCategory)
      {
         var differ = false;
         var backup = new CategoryBackup
         {
            UpdateEventId = updateEventId,
            CategoryId = existingCategory.Id
         };

         if (existingCategory.Title != processingCategory.Title)
         {
            backup.Title = existingCategory.Title;
            existingCategory.Title = processingCategory.Title;
            differ = true;
         }

         if (existingCategory.LogoUrl != processingCategory.LogoUrl)
         {
            backup.LogoUrl = existingCategory.LogoUrl;
            existingCategory.LogoUrl = processingCategory.LogoUrl;
            differ = true;
         }

         if (existingCategory.LogoFileName != processingCategory.LogoFileName)
         {
            backup.LogoFileName = existingCategory.LogoFileName;
            existingCategory.LogoFileName = processingCategory.LogoFileName;
            differ = true;
         }

         existingCategory.IsDeleted = false;

         return differ ? backup : null;
      }
   }
}