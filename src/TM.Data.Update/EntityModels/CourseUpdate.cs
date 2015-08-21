using System.Diagnostics.CodeAnalysis;

namespace TM.Data.Update
{
   [SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
   [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
   public class CourseUpdate
   {
      public int UpdateEventId { get; set; }
      public int CourseId { get; set; }

      public OperationType OperationType { get; set; }

      public virtual UpdateEvent UpdateEvent { get; set; }
      public virtual Course Course { get; set; }
      public virtual CourseBackup CourseBackup { get; set; }
   }
}