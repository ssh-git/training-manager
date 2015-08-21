namespace TM.Data.Update
{
   public interface ICatalogBackupProcessor<in TCategoryParseModel, in TCourseParseModel, in TAuthorParseModel>
   {
      IBackupCreator<Category, TCategoryParseModel, CategoryBackup> CategoryBackupCreator { get; }
      IBackupCreator<Course, TCourseParseModel, CourseBackup> CourseBackupCreator { get; }
      IBackupCreator<TrainingProviderAuthor, TAuthorParseModel, AuthorBackup> AuthorBackupCreator { get; }
   }
}