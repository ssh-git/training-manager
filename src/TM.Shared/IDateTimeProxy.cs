using System;

namespace TM.Shared
{
   public interface IDateTimeProxy
   {
      DateTime UtcNow { get; }
   }

   public class DateTimeProxy : IDateTimeProxy
   {
      private static readonly DateTimeProxy ProxyInstance = new DateTimeProxy();

      public static DateTimeProxy Instance
      {
         get { return ProxyInstance; }
      }

      private DateTimeProxy()
      {
         
      }

      public DateTime UtcNow
      {
         get { return DateTime.UtcNow; }
      }
   }
}