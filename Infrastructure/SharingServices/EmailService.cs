using Core.DTO;
using Core.Interfaces;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration configuration;
        public EmailService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public async Task SendEmailAsync(EmailDTO emailDTO)
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress("Task Management", configuration["EmailSetting:From"]));
            message.To.Add(new MailboxAddress(emailDTO.To, emailDTO.To));
            message.Subject = emailDTO.Subject;

            message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = emailDTO.Content
            };

            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            try
            {

                await smtp.ConnectAsync(
                                configuration["EmailSetting:Smtp"],
                                int.Parse(configuration["EmailSetting:Port"]),
                                MailKit.Security.SecureSocketOptions.StartTls);

                await smtp.AuthenticateAsync(
                    configuration["EmailSetting:Username"],
                    configuration["EmailSetting:Password"]);

                await smtp.SendAsync(message);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to send email: {ex.Message}");
            }
            finally
            {
                if (smtp.IsConnected)
                    await smtp.DisconnectAsync(true);
            }
        }
    }
}

