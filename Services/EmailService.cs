using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace TicketingSystem_DotNetMVC.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                var host = _configuration["EmailSettings:Host"];
                var port = int.Parse(_configuration["EmailSettings:Port"] ?? "587");
                var senderEmail = _configuration["EmailSettings:SenderEmail"];
                var appPassword = _configuration["EmailSettings:AppPassword"];

                using (var client = new SmtpClient(host, port))
                {
                    client.EnableSsl = true;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(senderEmail, appPassword);

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(senderEmail!),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = false
                    };
                    mailMessage.To.Add(to);

                    await client.SendMailAsync(mailMessage);
                }
                
                Console.WriteLine($"[EMAIL SUCCESS] Sent to: {to}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EMAIL ERROR] Failed to send to {to}: {ex.Message}");
                // Fallback to simulation log if real send fails
                Console.WriteLine("--------------------------------------------------");
                Console.WriteLine($"[EMAIL SIMULATION] Sending to: {to}");
                Console.WriteLine($"[EMAIL SIMULATION] Subject: {subject}");
                Console.WriteLine($"[EMAIL SIMULATION] Body: {body}");
                Console.WriteLine("--------------------------------------------------");
            }
        }
    }
}