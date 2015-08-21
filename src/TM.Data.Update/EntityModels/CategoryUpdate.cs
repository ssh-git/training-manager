using System.Diagnostics.CodeAnalysis;

namespace TM.Data.Update
{
   [SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
   [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
   public class CategoryUpdate
   {
      public int UpdateEventId { get; set; }
      public int CategoryId { get; set; }

      public OperationType OperationType { get; set; }

      public virtual UpdateEvent UpdateEvent { get; set; }
      public virtual Category Category { get; set; }
      public virtual CategoryBackup CategoryBackup { get; set; }
   }
}