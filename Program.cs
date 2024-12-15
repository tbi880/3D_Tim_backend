using _3D_Tim_backend.Data;
using Microsoft.EntityFrameworkCore;
using _3D_Tim_backend.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// load environment variables from .env file
var envFilePath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envFilePath))
{
    var envVariables = File.ReadAllLines(envFilePath)
                           .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("#"))
                           .Select(line => line.Split('=', 2))
                           .ToDictionary(parts => parts[0].Trim(), parts => parts[1].Trim('"').Trim());

    foreach (var envVariable in envVariables)
    {
        builder.Configuration[envVariable.Key] = envVariable.Value;
    }
}


builder.Services.AddControllers(); // add controllers

// MySQL configuration 
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString: builder.Configuration["DB_CONNECTION_STRING"],
        new MySqlServerVersion(new Version(8, 0, 31))
    )
);

builder.Services.AddRepository();
builder.Services.AddServices();
builder.Services.AddMessageQueueServices();


// CORS configuration
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<Dictionary<string, string[]>>();

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
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("_developmentOrigins");
}
else
{
    app.UseCors("_productionOrigins");
}

app.UseHttpsRedirection();
app.MapControllers(); // add controllers for routing



app.Run();

