using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using _3D_Tim_backend.Services;

namespace _3D_Tim_backend.Consumers
{
    public class EmailConsumer : BackgroundService
    {
        private readonly IModel _channel;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public EmailConsumer(IServiceScopeFactory serviceScopeFactory, MessageQueueService messageQueueService)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _channel = messageQueueService.GetChannel();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);

                using var scope = _serviceScopeFactory.CreateScope();
                var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();

                // 假设消息内容为 JSON 格式 { "recipientEmail": "user@example.com", "recipientName": "Someone", "vCode": "AFQGSD213asdqwr12" }
                var emailData = JsonSerializer.Deserialize<EmailMessage>(message);
                await emailService.SendEmailAsync(emailData.recipientEmail, emailData.recipientName, emailData.vCode);
            };

            _channel.BasicConsume(queue: "email_queue", autoAck: true, consumer: consumer);
            return Task.CompletedTask;
        }
    }

    public class EmailMessage
    {
        public string recipientEmail { get; set; }
        public string recipientName { get; set; }
        public string vCode { get; set; }
    }
}
