using System;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;

namespace TM.Data
{
   public class RetryExecutionStrategy : DbExecutionStrategy
   {
      /// <summary>
      /// Creates a new instance of System.Data.Entity.Infrastructure.DbExecutionStrategy.
      /// </summary>
      /// <remarks>
      /// The default retry limit is 5, which means that the total amount of time spent
      /// between retries is 26 seconds plus the random factor.
      /// </remarks>
      public RetryExecutionStrategy()
      {
      }


      /// <summary>
      /// Creates a new instance of System.Data.Entity.Infrastructure.DbExecutionStrategy
      /// with the specified limits for number of retries and the delay between retries.
      /// </summary>
      /// <param name="maxRetryCount">The maximum number of retry attempts.</param>
      /// <param name="maxDelay">The maximum delay in milliseconds between retries.</param>
      public RetryExecutionStrategy(int maxRetryCount, TimeSpan maxDelay)
         : base(maxRetryCount, maxDelay)
      {
      }

      /// <summary>
      /// Determines whether the specified exception represents a transient failure that can be compensated by a retry.
      /// </summary>
      /// <param name="ex">The exception object to be verified.</param>
      /// <returns>
      /// <c>true</c> if the specified exception is considered as transient, otherwise <c>false</c>.
      /// </returns>
      protected override bool ShouldRetryOn(Exception ex)
      {
         var retry = false;

         var sqlException = ex as SqlException;
         if (sqlException != null)
         {
            int[] errorsToRetry =
            {
               701,  // There is insufficient system memory in resource pool to run this query
               1204, // The instance of the SQL Server Database Engine cannot obtain a LOCK resource at this time. Rerun your statement when there are fewer active users. Ask the database administrator to check the lock and memory configuration for this instance, or to check for
               1205, // Transaction (Process ID %d) was deadlocked on %.*ls resources with another process and has been chosen as the deadlock victim. Rerun the transaction.
               1222, // Lock request time out period exceeded
               8645, // A timeout occurred while waiting for memory resources to execute the query in resource pool '%ls' (%ld). Rerun the query.
               -2    //Timeout
            };
            if (sqlException.Errors.Cast<SqlError>().Any(x => errorsToRetry.Contains(x.Number)))
            {
               retry = true;
            }
         } else if (ex is TimeoutException)
         {
            retry = true;
         }

         return retry;
      }
   }
}