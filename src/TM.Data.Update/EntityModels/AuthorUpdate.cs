using System.Diagnostics.CodeAnalysis;

namespace TM.Data.Update
{
   [SuppressMessage("ReSharper", "ClassWithVirtualMembersNeverInherited.Global")]
   [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
   public class AuthorUpdate
   {
      public int UpdateEventId { get; set; }
      public int TrainingProviderId { get; set; }
      public int AuthorId { get; set; }

      public OperationType OperationType { get; set; }

      public virtual UpdateEvent UpdateEvent { get; set; }
      public virtual TrainingProviderAuthor TrainingProviderAuthor { get; set; }
      public virtual AuthorBackup AuthorBackup { get; set; }
   }
}