// Services/EmailService.cs

using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace EmailSenderApi.Services
{
    public class EmailService
    {
        private readonly string _smtpServer = "smtp.gmail.com"; // Use your SMTP server
        private readonly int _smtpPort = 587; // Use the appropriate port
        private readonly string _smtpUser = "kristydurga@gmail.com"; // Your email
        private readonly string _smtpPass = "lncx wzvc vshz qnfv"; // Your email password or app password

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            using (var client = new SmtpClient(_smtpServer, _smtpPort))
            {
                client.Credentials = new NetworkCredential(_smtpUser, _smtpPass);
                client.EnableSsl = true;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_smtpUser),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
            }
        }
    }
}