using System.Threading.Tasks;

namespace TM.Shared
{
   public interface IEmailNotification
   {
      Task SendAsync(string subject, string message);
   }
}
