using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net.Mail;

namespace KLMS.Services
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                MailMessage massage = new MailMessage();
                SmtpClient smtpServer = new SmtpClient();
                massage.From = new MailAddress("");
                massage.To.Add(email);
                massage.Subject = subject;
                massage.IsBodyHtml = true;
                massage.Body = htmlMessage;

                smtpServer.Port = 587;
                smtpServer.Host = "smtp.gmail.com";

                smtpServer.EnableSsl = true;
                smtpServer.UseDefaultCredentials = false;
                smtpServer.Credentials = new System.Net.NetworkCredential("email", "password");
                smtpServer.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtpServer.Send(massage);
                return Task.CompletedTask;

            }
            catch (Exception ex)
            {
                return Task.FromException(ex);
            }
        }
    }
}
