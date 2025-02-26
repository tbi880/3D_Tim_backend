using _3D_Tim_backend.Repositories;
using _3D_Tim_backend.Services;
using _3D_Tim_backend.Consumers;
using RabbitMQ.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using _3D_Tim_backend.Utils;
using Microsoft.IdentityModel.Tokens;


namespace _3D_Tim_backend.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRepository(this IServiceCollection services)
        {
            services.AddScoped<IEmailContactRepository, EmailContactRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            return services;
        }

        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddSingleton<ISessionManager, SessionManager>();
            services.AddScoped<IEmailContactService, EmailContactService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ITempUserService, TempUserService>();
            services.AddScoped<RoomManager>();
            services.AddScoped<BaccaratRoomManager>();
            return services;
        }

        public static IServiceCollection AddMessageQueueServices(this IServiceCollection services)
        {
            services.AddSingleton<IMessageQueueService, MessageQueueServiceForRabbitMQ>();
            services.AddSingleton<EmailService>();
            services.AddHostedService<EmailConsumer<IModel>>();
            return services;
        }

        public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, ConfigurationManager configuration)
        {
            string secretKey = configuration["JWT_SECRET"]
                                ?? throw new Exception("JWT_SECRET is not configured in .env or environment variables.");

            services.AddSingleton<JwtTokenGenerator>(_ => new JwtTokenGenerator(secretKey));
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes(secretKey)),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });
            return services;
        }

        public static IServiceCollection AddRoomStorage(this IServiceCollection services)
        {
            services.AddSingleton<RoomStorage>();
            return services;
        }
    }
}
