using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using _3D_Tim_backend.Services;
using MimeKit;
using MailKit.Net.Smtp;

namespace _3D_Tim_backend.Consumers
{
    public class EmailConsumer<T> : BackgroundService where T : class, IModel
    {
        private readonly T _channel;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        private readonly ILogger<EmailConsumer<T>> _logger;

        public EmailConsumer(IServiceScopeFactory serviceScopeFactory, IMessageQueueService messageQueueService, ILogger<EmailConsumer<T>> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _channel = messageQueueService.GetChannel<T>();
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                try
                {
                    if (stoppingToken.IsCancellationRequested) return;

                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                    var emailData = JsonSerializer.Deserialize<EmailMessage>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (emailData is null)
                    {
                        _logger.LogWarning("EmailMessage is null. Payload={Payload}", json);
                        return;
                    }

                    if (!MailboxAddress.TryParse(emailData.recipientEmail, out _))
                    {
                        _logger.LogWarning("Skip invalid email: {Email}. Payload={Payload}",
                            emailData.recipientEmail, json);
                        return;
                    }

                    using var scope = _serviceScopeFactory.CreateScope();
                    var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();

                    try
                    {
                        await emailService.SendEmailAsync(
                            emailData.recipientEmail,
                            emailData.recipientName,
                            emailData.vCode);

                        _logger.LogInformation("Email sent to {Email}", emailData.recipientEmail);
                    }
                    catch (SmtpCommandException ex)
                    {
                        _logger.LogWarning(ex, "SMTP command error. Skip email: {Email}", emailData.recipientEmail);
                    }
                    catch (SmtpProtocolException ex)
                    {
                        _logger.LogError(ex, "SMTP protocol error. Dropping message for {Email}", emailData.recipientEmail);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unexpected error while sending email to {Email}", emailData.recipientEmail);
                    }
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Bad JSON. Dropped.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled error in consumer. Dropped.");
                }
            };

            _channel.BasicConsume(queue: "email_queue", autoAck: true, consumer: consumer);
            return Task.CompletedTask;
        }
    }

    public record class EmailMessage
    {
        public required string recipientEmail { get; set; }
        public required string recipientName { get; set; }
        public required string vCode { get; set; }
    }
}
