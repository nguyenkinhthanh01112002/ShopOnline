using ShopWebAPI.Interfaces.Auth;
using System.Net.Mail;
using System.Net;

namespace ShopWebAPI.Services.Auth
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            using (var client = new SmtpClient())
            {
                var credential = new NetworkCredential
                {
                    UserName = _configuration["Email:Email"],
                    Password = _configuration["Email:Password"]
                };

                client.Credentials = credential;
                client.Host = _configuration["Email:Host"];
                client.Port = int.Parse(_configuration["Email:Port"]);
                client.EnableSsl = bool.Parse(_configuration["Email:EnableSsl"]);

                using (var emailMessage = new MailMessage())
                {
                    emailMessage.To.Add(new MailAddress(email));
                    emailMessage.From = new MailAddress(_configuration["Email:Email"]);
                    emailMessage.Subject = subject;
                    emailMessage.Body = message;
                    emailMessage.IsBodyHtml = true;

                    await client.SendMailAsync(emailMessage);
                }
            }
        }
    }
}
