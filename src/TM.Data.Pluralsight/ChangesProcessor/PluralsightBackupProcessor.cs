using TM.Data.Update;

namespace TM.Data.Pluralsight
{
   internal class PluralsightBackupProcessor :
      ICatalogBackupProcessor<PluralsightCategory, PluralsightCourse, PluralsightAuthor>
   {
      public PluralsightBackupProcessor()
      {
         CategoryBackupCreator = new PluralsightCategoryBackupCreator();
         CourseBackupCreator = new PluralsightCourseBackupCreator();
         AuthorBackupCreator = new PluralsightAuthorBackupCreator();
      }

      public IBackupCreator<Category, PluralsightCategory, CategoryBackup> CategoryBackupCreator { get; private set; }
      public IBackupCreator<Course, PluralsightCourse, CourseBackup> CourseBackupCreator { get; private set; }
      public IBackupCreator<TrainingProviderAuthor, PluralsightAuthor, AuthorBackup> AuthorBackupCreator { get; private set; }
   }
}