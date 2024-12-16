using MailKit.Net.Smtp;
using MimeKit;


namespace _3D_Tim_backend.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string recipientEmail, string recipientName, string vCode)
        {
            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "EmailTemplate.html");
            string htmlTemplate = await File.ReadAllTextAsync(templatePath);
            string subject = "Thanks for your visiting Tim Bi's world! Here is your verification code.";
            string body = htmlTemplate
                .Replace("{{Subject}}", subject)
                .Replace("{{Name}}", recipientName)
                .Replace("{{VCode}}", vCode);

            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(_configuration["SENDER_NAME"], _configuration["SENDER_EMAIL"]));
            emailMessage.To.Add(new MailboxAddress(recipientEmail, recipientEmail));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart("html") { Text = body };
            using var client = new SmtpClient();

            try
            {
                client.Timeout = 10000;
                await client.ConnectAsync(_configuration["SMTP_SERVER"], int.Parse(_configuration["SMTP_PORT"]), MailKit.Security.SecureSocketOptions.Auto);
                await client.AuthenticateAsync(_configuration["SENDER_EMAIL"], _configuration["EMAIL_PASSWORD"]);
                await client.SendAsync(emailMessage);

            }
            catch (TimeoutException ex)
            {
                Console.WriteLine("SMTP connection timed out.");
                Console.WriteLine(ex.Message);
            }
            finally
            {
                if (client != null && client.IsConnected)
                {
                    await client.DisconnectAsync(true);
                }
                client?.Dispose();
            }

        }
    }
}
