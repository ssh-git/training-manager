using System.Data.Entity;

namespace TM.Data
{
   public interface IDbContextFactory<out TContext> 
      where TContext : DbContext, new()
   {
      TContext CreateDbContext();
   }
}