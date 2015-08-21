namespace TM.Data.Update
{
   public interface IChangesDetector<in TExisting, in TProcessing>
   {
      bool IsDifferent(TExisting existing, TProcessing processing);
   }
}