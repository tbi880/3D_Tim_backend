using _3D_Tim_backend.Repositories;
using _3D_Tim_backend.Services;
using _3D_Tim_backend.Consumers;
using RabbitMQ.Client;


namespace _3D_Tim_backend.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRepository(this IServiceCollection services)
        {
            services.AddScoped<IEmailContactRepository, EmailContactRepository>();
            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<IEmailContactService, EmailContactService>();
            return services;
        }

        public static IServiceCollection AddMessageQueueServices(this IServiceCollection services)
        {
            services.AddSingleton<IMessageQueueService, MessageQueueServiceForRabbitMQ>();
            services.AddSingleton<EmailService>();
            services.AddHostedService<EmailConsumer<IModel>>();
            return services;
        }
    }
}
