using System.ComponentModel.DataAnnotations;

namespace TM.Data.Update
{
   public enum UpdateResult:byte
   {
      [Display(Name = "Unknown")]
      None = 0,
      Error,
      Success,
      [Display(Name = "Need Manual Resolve")]
      NeedManualResolve,
      Resolved
   }
}