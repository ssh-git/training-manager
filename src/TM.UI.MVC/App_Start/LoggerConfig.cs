using System;
using System.Configuration;
using System.IO;
using System.Threading;
using System.Web.Hosting;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using SerilogWeb.Classic.Enrichers;
using TM.Shared.Parse;
using TM.UI.MVC.Properties;

namespace TM.UI.MVC
{
   public static class LoggerConfig
   {
      private static readonly LoggingLevelSwitch LoggingLevelController;
      private static readonly LoggingLevelSwitch UpdateLoggingLevelController;

      private static string _logDirectory;

      static LoggerConfig()
      {
         LoggingLevelController = new LoggingLevelSwitch();
         UpdateLoggingLevelController = new LoggingLevelSwitch();
         _logDirectory = HostingEnvironment.MapPath(ConfigurationManager.AppSettings["TM.Paths.Log"]);

         var file = File.CreateText(Path.Combine(_logDirectory, "serilog.debug.log"));
         Serilog.Debugging.SelfLog.Out = TextWriter.Synchronized(file);
      }

      public static CallResult ChangeLoggingLevel(LogEventLevel logEventLevel)
      {
         bool acquiredLock = false;
         try
         {
            Monitor.Enter(LoggingLevelController, ref acquiredLock);
            if (acquiredLock)
            {
               LoggingLevelController.MinimumLevel = logEventLevel;

               return CallResult.Success;
            }

            return CallResult.Failed(Resources.CannotAcquireLock);
         }
         finally
         {
            if (acquiredLock)
            {
               Monitor.Exit(LoggingLevelController);
            }
         }
      }

      public static CallResult ChangeUpdateLoggingLevel(LogEventLevel logEventLevel)
      {
         bool acquiredLock = false;
         try
         {
            Monitor.TryEnter(UpdateLoggingLevelController, ref acquiredLock);
            if (acquiredLock)
            {
               UpdateLoggingLevelController.MinimumLevel = logEventLevel;
               return CallResult.Success;
            }

            return CallResult.Failed(Resources.CannotAcquireLock);
         }
         finally
         {
            if (acquiredLock)
            {
               Monitor.Exit(UpdateLoggingLevelController);
            }
         }
      }

      public static void RegisterLogger()
      {
         Log.Logger = new LoggerConfiguration()
            .MinimumLevel.ControlledBy(LoggingLevelController)
            .WriteTo.RollingFile(
               pathFormat: Path.Combine(_logDirectory, "training-manager-http-{Date}.log"),
               outputTemplate:
                  @"{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] Username:{UserName} IP:{HttpRequestClientHostIP} Host:{HttpRequestClientHostName}{Message}{NewLine}{Exception}",
               fileSizeLimitBytes: 5 * 1024 * 1024,
               retainedFileCountLimit: 31)
            .Enrich.With(new UserNameEnricher(), new HttpRequestClientHostIPEnricher(true), new HttpRequestClientHostNameEnricher())
            .CreateLogger();
      }

      public static ILogger CreateUpdateLogger()
      {
         return new LoggerConfiguration()
            .MinimumLevel.ControlledBy(UpdateLoggingLevelController)
            .WriteTo.RollingFile(
               pathFormat: Path.Combine(_logDirectory, "training-manager-updates-{Date}.log"),
               outputTemplate:
                  "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] ThreadId:{ThreadId:00} {Message}{NewLine}{Exception}",
               fileSizeLimitBytes: 1 * 1024 * 1024,
               retainedFileCountLimit: 7)
            .Enrich.WithThreadId()
            .CreateLogger();
      }
   }
}