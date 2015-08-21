using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Runtime.Remoting.Messaging;

namespace TM.Data
{
   /// <summary>
   /// The class can be placed in the same assembly as a class
   ///  derived from System.Data.Entity.DbContext to define Entity Framework configuration
   ///  for an application.  Configuration is set by calling protected methods and
   ///  setting protected properties of this class in the constructor of your derived
   ///  type.  The type to use can also be registered in the config file of the application.
   ///  See http://go.microsoft.com/fwlink/?LinkId=260883 for more information about
   ///  Entity Framework configuration.  
   /// </summary>
   public class TrainingManagerDbConfiguration: DbConfiguration
   {
      public TrainingManagerDbConfiguration()
      {
         SetExecutionStrategy("System.Data.SqlClient", () => SuspendExecutionStrategy
         ? (IDbExecutionStrategy) new DefaultExecutionStrategy()
         : new RetryExecutionStrategy());
      }

      /// <summary>
      /// Toggle execution strategy.
      /// See <see cref="https://msdn.microsoft.com/en-us/data/dn307226"/> for details
      /// </summary>
      /// <returns>
      /// <value>true</value> - will be used <see cref="System.Data.Entity.Infrastructure.DefaultExecutionStrategy"/>;
      /// <value>false</value> - will be used <see cref="TM.Data.RetryExecutionStrategy"/>
      /// </returns>
      public static bool SuspendExecutionStrategy
      {
         get
         {
            return (bool?)CallContext.LogicalGetData("SuspendExecutionStrategy") ?? false;
         }
         set
         {
            CallContext.LogicalSetData("SuspendExecutionStrategy", value);
         }
      }
   }
}
