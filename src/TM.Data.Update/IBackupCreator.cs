namespace TM.Data.Update
{
   public interface IBackupCreator<in TExisting, in TProcessing, out TBackup> : IChangesDetector<TExisting, TProcessing>
   {
      TBackup ApplayChanges(int updateEventId, TExisting existing, TProcessing processing);
   }
}