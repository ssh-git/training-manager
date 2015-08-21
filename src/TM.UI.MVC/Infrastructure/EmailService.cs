using System.Configuration;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;

namespace TM.UI.MVC.Infrastructure
{
   public class EmailService : IIdentityMessageService
   {
      public Task SendAsync(IdentityMessage message)
      {
         // Plug in your email service here to send an email.
         return Task.FromResult(0);
      }

      public async Task SendToAdminAsync(string subject, string message)
      {
         var mail = new MailMessage
         {
            Subject = subject,
            Body = message,
            SubjectEncoding = Encoding.UTF8,
            BodyEncoding = Encoding.UTF8
         };

         var sendAddress = new MailAddress(ConfigurationManager.AppSettings["NotifyEmailAddress"]);
         mail.To.Add(sendAddress);

         using (var smtpClient = new SmtpClient())
         {
            await smtpClient.SendMailAsync(mail).ConfigureAwait(false);
         }
      }
   }
}