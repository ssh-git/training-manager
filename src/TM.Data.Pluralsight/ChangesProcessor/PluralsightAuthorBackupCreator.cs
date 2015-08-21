using TM.Data.Update;

namespace TM.Data.Pluralsight
{
   internal class PluralsightAuthorBackupCreator : IBackupCreator<TrainingProviderAuthor, PluralsightAuthor, AuthorBackup>
   {
      public bool IsDifferent(TrainingProviderAuthor existing, PluralsightAuthor processing)
      {
         return existing.FullName != processing.FullName ||
                existing.SiteUrl != processing.SiteUrl ||
                existing.IsDeleted;
      }

      public AuthorBackup ApplayChanges(int updateEventId, TrainingProviderAuthor existing, PluralsightAuthor processing)
      {
         var differ = false;
         var backup = new AuthorBackup
         {
            UpdateEventId = updateEventId,
            TrainingProviderId = existing.TrainingProviderId,
            AuthorId = existing.AuthorId
         };

         if (existing.FullName != processing.FullName)
         {
            backup.FullName = existing.FullName;
            existing.FullName = processing.FullName;
            differ = true;
         }

         if (existing.SiteUrl != processing.SiteUrl)
         {
            backup.SiteUrl = existing.SiteUrl;
            existing.SiteUrl = processing.SiteUrl;
            differ = true;
         }

         existing.IsDeleted = false;

         return differ ? backup : null;
      }
   }
}