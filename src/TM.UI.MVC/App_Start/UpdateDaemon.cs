using System;
using System.Threading.Tasks;
using Serilog;
using TM.Data;
using TM.Data.Update;
using TM.Shared;
using TM.Shared.Parse;

namespace TM.UI.MVC
{
   
   public static class UpdateDaemon
   {
      private static ILogger _updateLogger = LoggerConfig.CreateUpdateLogger();

      /// <summary>
      /// Update training catalogs in background. Use for HangFire recurrent job
      /// </summary>
      public static void ScheduledCatalogsUpdate()
      {
         AsyncHelper.RunSync(ScheduledCatalogsUpdateAsync);
      }


      private static async Task ScheduledCatalogsUpdateAsync()
      {
         using (_updateLogger.BeginTimedOperation("Scheduled database update"))
         {
            var updateResult = await UpdateTrainingCatalogsAsync();
            if (updateResult.Succeeded)
            {
               _updateLogger.Information("Operation {Result:l}", updateResult.Succeeded);
            } else
            {
               _updateLogger.Warning("Operation failed. {@UpdateResult}", updateResult);

               throw new UpdateServiceException(string.Join(";", updateResult.Errors), updateResult.Exception);
            }
         }
      }


      public static async Task<CallResult> UpdateTrainingCatalogsAsync()
      {
         try
         {
            var updateService = new UpdateService(AppConstants.ServerPaths.MediaDirectory, AppConstants.ServerPaths.ArchiveDirectory);
            return await updateService.UpdateTrainingCatalogsAsync();
         }
         catch (Exception ex)
         {
            return ProcessError(ex);
         }
      }

      public static async Task<CallResult> UpdateTrainingCatalogAsync(int trainingProviderId)
      {
         try
         {
            var updateService = new UpdateService(AppConstants.ServerPaths.MediaDirectory, AppConstants.ServerPaths.ArchiveDirectory);
            return await updateService.UpdateTrainingCatalogAsync(trainingProviderId);
         }
         catch (Exception ex)
         {
            return ProcessError(ex);
         }
      }

      public static async Task<CallResult> ReassignCourseSpecializations(TrainingProvider trainingProvider)
      {
         try
         {
            var updateService = new UpdateService(AppConstants.ServerPaths.MediaDirectory, AppConstants.ServerPaths.ArchiveDirectory);
            return await updateService.ReassignCourseSpecializationsAsync(trainingProvider);
         }
         catch (Exception ex)
         {
            return ProcessError(ex);
         }
      }

      private static CallResult ProcessError(Exception exception)
      {
         Log.Error(exception, "{UpdateService} crashed", typeof(UpdateService).AssemblyQualifiedName);
         return CallResult.Failed(exception);
      }
   }
}