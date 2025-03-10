using System.Text;
using RabbitMQ.Client;


namespace _3D_Tim_backend.Services
{
    public class MessageQueueServiceForRabbitMQ : IMessageQueueService
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public MessageQueueServiceForRabbitMQ(IConfiguration configuration)
        {
            var factory = new ConnectionFactory
            {
                HostName = configuration["RabbitMQ_Host"],
                UserName = configuration["RabbitMQ_Username"],
                Password = configuration["RabbitMQ_Password"]
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: "email_queue", durable: true, exclusive: false, autoDelete: false, arguments: null);
        }

        public void PublishMessage(string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            _channel.BasicPublish(exchange: "", routingKey: "email_queue", basicProperties: null, body: body);
        }

        public T GetChannel<T>()
        {
            return (T)_channel;
        }


        public void Dispose()
        {
            _channel.Close();
            _connection.Close();
        }

    }
}
