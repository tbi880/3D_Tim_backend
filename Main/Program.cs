using _3D_Tim_backend.Data;
using Microsoft.EntityFrameworkCore;
using _3D_Tim_backend.Extensions;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Services.Configure<HostOptions>(o =>
{
    o.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
});

// Load environment variables from .env file
var envFilePath = Path.Combine(Directory.GetCurrentDirectory(), "../.env");
if (File.Exists(envFilePath))
{
    var envVariables = File.ReadAllLines(envFilePath)
                           .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("#"))
                           .Select(line => line.Split('=', 2))
                           .ToDictionary(
                               parts => parts[0].Trim(),
                               parts => parts.Length > 1 ? parts[1].Trim('"').Trim() : string.Empty);

    foreach (var envVariable in envVariables)
    {
        builder.Configuration[envVariable.Key] = envVariable.Value;
    }
}
else
{
    Console.WriteLine("Warning: .env file not found. Using environment variables or defaults.");
}

// MySQL configuration
var connectionString = builder.Configuration["DB_CONNECTION_STRING"];
if (string.IsNullOrEmpty(connectionString))
{
    throw new Exception("Database connection string is not configured. Please check your .env or appsettings.json.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString: connectionString,
        new MySqlServerVersion(new Version(8, 0, 31))
    )
);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddRepository();
builder.Services.AddServices();
builder.Services.AddMessageQueueServices();
builder.Services.AddAuthenticationServices(builder.Configuration);
builder.Services.AddRoomStorage();


// CORS configuration
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<Dictionary<string, string[]>>();
if (allowedOrigins == null || !allowedOrigins.ContainsKey("Production"))
{
    throw new Exception("AllowedOrigins:Production configuration is missing.");
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("_developmentOrigins", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });

    options.AddPolicy("_productionOrigins", policy =>
    {
        policy.WithOrigins(allowedOrigins["Production"])
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "3D_Tim_backend API",
        Version = "v1"
    });
});

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddSimpleConsole(options =>
{
    options.TimestampFormat = "[yyyy-MM-dd HH:mm:ss.fff] ";
});

var app = builder.Build();

// Apply migrations automatically
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("_developmentOrigins");
}
else
{
    app.UseHttpsRedirection();
    app.UseCors("_productionOrigins");
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers(); // Add controllers for routing
app.Run();
