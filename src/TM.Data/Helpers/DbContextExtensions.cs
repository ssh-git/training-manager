using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;

namespace TM.Data
{
   public static class DbContextExtensions
   {
      public static MetadataWorkspace GetMetadataWorkspace(this DbContext context)
      {
         var metadataWorkspace = GetObjectContext(context).MetadataWorkspace;
         return metadataWorkspace;
      }

      public static ObjectContext GetObjectContext(this DbContext context)
      {
         var objectContext = ((IObjectContextAdapter)context).ObjectContext;
         return objectContext;
      }
   }
}
