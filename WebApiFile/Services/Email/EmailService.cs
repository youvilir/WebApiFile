using MailKit.Net.Smtp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace WebApiFile.Services.Email
{
    public class EmailService : IEmailService
    {
        private IServiceProvider _services;
        private readonly ILogger<EmailService> _logger;
        private readonly Options _options = new Options();
        public EmailService(IServiceProvider services)
        {
            _services = services;
            _logger = services.GetRequiredService<ILogger<EmailService>>();
        }
        public async Task SendEmailAsync(string message)
        {
            MimeMessage mail = new MimeMessage();

            mail.From.Add(new MailboxAddress("Служба", _options.Mail));
            mail.To.Add(new MailboxAddress("", _options.Mail));
            mail.Body = new TextPart("Plain")
            {
                Text = $"ваш код для удаления файла: {message}",
            };

            try
            {
                var client = new SmtpClient();

                await client.ConnectAsync("smtp.yandex.com", 465, MailKit.Security.SecureSocketOptions.SslOnConnect);
                await client.AuthenticateAsync(_options.Mail, _options.Password);
                await client.SendAsync(mail);
                await client.DisconnectAsync(true);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
        }
    }

    // TODO вынести в конфиг
    public class Options
    {
        public string Mail { get; set; } = "";

        public string Password { get; set; } = "";
    }
}


