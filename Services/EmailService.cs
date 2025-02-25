using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace MyField.Services
{
    public class EmailService
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUsername;
        private readonly string _smtpPassword;

        public EmailService()
        {
            _smtpServer = "smtp.gmail.com";
            _smtpPort = 587;
            _smtpUsername = "khayalethu.ngema@xetgroup.com";
            _smtpPassword = "Ngema@12";
        }

        public async Task SendEmailAsync(string to, string subject, string body, string senderName)
        {
            using (var client = new SmtpClient(_smtpServer, _smtpPort))
            {
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
                client.EnableSsl = true;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_smtpUsername, senderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(to);

                try
                {
                    await client.SendMailAsync(mailMessage);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send email: {ex.Message}");
                    throw;
                }
            }
        }

        public async Task SendEmailWithAttachmentsAsync(string to, string subject, string body, List<string> attachmentPaths, string senderName)
        {
            using (var client = new SmtpClient(_smtpServer, _smtpPort))
            {
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(_smtpUsername, _smtpPassword);
                client.EnableSsl = true;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_smtpUsername, senderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(to);

                foreach (var attachmentPath in attachmentPaths)
                {
                    if (File.Exists(attachmentPath))
                    {
                        var attachment = new Attachment(attachmentPath);
                        mailMessage.Attachments.Add(attachment);
                    }
                }

                try
                {
                    await client.SendMailAsync(mailMessage);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send email: {ex.Message}");
                    throw;
                }
            }
        }

    }
}