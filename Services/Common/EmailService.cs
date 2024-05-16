using Microsoft.Extensions.Configuration;
using Services.Interfaces;
using System.Net.Mail;
using System.Net;

namespace Services.Common
{
	public class EmailService : IEmailService
	{
		private readonly IConfiguration _configuration;

		public EmailService(IConfiguration config)
		{
			_configuration = config;
		}

		public Task SendEmailAsync(string toEmail, string subject, string body, bool isBodyHTML)
		{
			string MailServer = _configuration["EmailSettings:MailServer"];
			string FromEmail = _configuration["EmailSettings:FromEmail"];
			string Password = _configuration["EmailSettings:Password"];
			int Port = int.Parse(_configuration["EmailSettings:MailPort"]);
			var client = new SmtpClient(MailServer, Port)
			{
				Credentials = new NetworkCredential(FromEmail, Password),
				EnableSsl = true,
			};

			MailMessage mailMessage = new MailMessage(FromEmail, toEmail, subject, body)
			{
				IsBodyHtml = isBodyHTML
			};

			return client.SendMailAsync(mailMessage);
		}
	}
}
