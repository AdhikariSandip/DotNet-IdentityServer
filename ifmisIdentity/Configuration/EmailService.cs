using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;

namespace ifmisIdentity.Configuration
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var smtpHost = _configuration["EmailSettings:SMTPHost"];
            var smtpPort = int.Parse(_configuration["EmailSettings:SMTPPort"]);
            var username = _configuration["EmailSettings:Username"];
            var password = _configuration["EmailSettings:Password"];

            using (var client = new SmtpClient(smtpHost, smtpPort))
            {
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(username, password);

                // Apply correct SSL/TLS settings
                if (smtpPort == 465)
                {
                    client.EnableSsl = true;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                }
                else if (smtpPort == 587)
                {
                    client.EnableSsl = false;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                }

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(username),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(toEmail);

                try
                {
                    await client.SendMailAsync(mailMessage);
                }
                catch (SmtpException smtpEx)
                {
                    throw new Exception($"SMTP Error: {smtpEx.StatusCode} - {smtpEx.Message}");
                }
                catch (Exception ex)
                {
                    throw new Exception($"General Error: {ex.Message}");
                }
            }
        }
    }
}
