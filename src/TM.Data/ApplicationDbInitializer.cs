using System.Data.Entity;

namespace TM.Data
{
   
   public class ApplicationDbInitializer<TContext>: IDatabaseInitializer<TContext>
      where TContext : DbContext
   {
      private readonly IDatabaseInitializer<TContext> _initializationStrategy;

      public ApplicationDbInitializer(IDatabaseInitializer<TContext> initializationStrategy)
      {
         _initializationStrategy = initializationStrategy;
      }

      protected virtual void Seed(TContext context)
      {
         var enumMapper = new EnumToLookupTableMapper(context);
         enumMapper.MapEnums();
      }

      public void InitializeDatabase(TContext context)
      {
         _initializationStrategy.InitializeDatabase(context);
         Seed(context);
      }
   }
}
