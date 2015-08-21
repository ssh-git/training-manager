using System.Data.Entity;

namespace TM.Data
{
   public class DbContextFactory<TContext> : IDbContextFactory<TContext>
      where TContext : DbContext, new()
   {
      public TContext CreateDbContext()
      {
         return new TContext();
      }
   }
}