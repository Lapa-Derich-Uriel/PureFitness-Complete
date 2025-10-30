using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace PureFitness.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var mailSettings = _config.GetSection("MailSettings");
            var providerKey = mailSettings["Provider"];
            var provider = mailSettings.GetSection($"Providers:{providerKey}");

            if (provider == null)
                throw new Exception($"Mail provider '{providerKey}' not found in configuration.");

            // ✅ Safely retrieve configuration values with fallback defaults
            string? fromEmail = provider["Mail"];
            string? displayName = provider["DisplayName"];
            string? password = provider["Password"];
            string? host = provider["Host"];
            string? portValue = provider["Port"];
            string? sslValue = provider["EnableSSL"];

            // ✅ Validate required fields (to prevent runtime exceptions)
            if (string.IsNullOrWhiteSpace(fromEmail) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(host) ||
                string.IsNullOrWhiteSpace(portValue))
            {
                throw new InvalidOperationException("Mail configuration is incomplete or missing required values.");
            }

            // ✅ Parse with safe fallbacks
            int port = int.TryParse(portValue, out var parsedPort) ? parsedPort : 587;
            bool enableSsl = bool.TryParse(sslValue, out var parsedSsl) ? parsedSsl : true;

            using (var client = new SmtpClient(host, port))
            {
                client.EnableSsl = enableSsl;
                client.Credentials = new NetworkCredential(fromEmail, password);

                // ✅ Use null-forgiving operator only after checks
                var mail = new MailMessage
                {
                    From = new MailAddress(fromEmail!, displayName ?? "PureFitness"),
                    Subject = subject ?? "(No Subject)",
                    Body = body ?? "",
                    IsBodyHtml = true
                };

                mail.To.Add(toEmail ?? throw new ArgumentNullException(nameof(toEmail)));

                client.Send(mail);
            }
        }
    }
}
