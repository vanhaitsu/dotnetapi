using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Services.Interfaces;

namespace Services.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Task SendEmailAsync(string toEmail, string subject, string body, bool isBodyHTML)
        {
            string mailServer = _configuration["EmailSettings:MailServer"]!;
            string fromEmail = _configuration["EmailSettings:FromEmail"]!;
            string password = _configuration["EmailSettings:Password"]!;
            int port = int.Parse(_configuration["EmailSettings:MailPort"]!);
            var client = new SmtpClient(mailServer, port)
            {
                Credentials = new NetworkCredential(fromEmail, password),
                EnableSsl = true,
            };

            MailMessage mailMessage = new MailMessage(fromEmail, toEmail, subject, body)
            {
                IsBodyHtml = isBodyHTML
            };

            return client.SendMailAsync(mailMessage);
        }
    }
}