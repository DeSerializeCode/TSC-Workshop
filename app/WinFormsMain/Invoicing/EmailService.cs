using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WinFormsMain.Invoicing
{
    public class EmailService
    {
        private readonly EmailOptions options;
        private readonly ILogger<EmailService> logger;

        public EmailService(IOptions<EmailOptions> options, ILogger<EmailService> logger)
        {
            this.options = options.Value;
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task SendInvoiceAsync(string to, string subject, string body, string attachmentPath)
        {
            using var client = new SmtpClient(options.Host, options.Port)
            {
                EnableSsl = options.EnableSsl,
            };

            if (!string.IsNullOrWhiteSpace(options.Username))
            {
                client.Credentials = new NetworkCredential(options.Username, options.Password);
            }

            using var message = new MailMessage
            {
                From = new MailAddress(options.FromAddress),
                Subject = subject,
                Body = body,
                IsBodyHtml = false,
            };

            message.To.Add(new MailAddress(to));
            message.Attachments.Add(new Attachment(attachmentPath));

            logger.LogInformation("Sending invoice email to {Recipient} via {Host}:{Port}", to, options.Host, options.Port);
            await client.SendMailAsync(message).ConfigureAwait(false);
        }
    }
}
