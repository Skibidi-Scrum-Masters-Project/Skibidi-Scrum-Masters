using VaultSharp;
using VaultSharp.V1.AuthMethods.Token;
using VaultSharp.V1.AuthMethods;
using VaultSharp.V1.Commons;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using FitnessApp.Shared.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using NLog;
using NLog.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Configure NLog
var logger = NLog.LogManager.Setup().LoadConfigurationFromFile("NLog.config").GetCurrentClassLogger();
logger.Debug("Starting AnalyticsService application");

// Configure NLog with ASP.NET Core
builder.Logging.ClearProviders();
builder.Logging.AddNLog();

// Configure Vault client
var vaultAddress = builder.Configuration["Vault:Address"] ?? "https://test.fitlife.qzz.io:8201/";
var vaultToken = builder.Configuration["Vault:Token"] ?? "00000000-0000-0000-0000-000000000000";
var secretsPath = builder.Configuration["Vault:SecretsPath"] ?? "secret/data/mongodb";

var httpClientHandler = new HttpClientHandler();
httpClientHandler.ServerCertificateCustomValidationCallback =
    (message, cert, chain, sslPolicyErrors) => { return true; };

IAuthMethodInfo authMethod = new TokenAuthMethodInfo(vaultToken);
var vaultClientSettings = new VaultClientSettings(vaultAddress, authMethod)
{
    Namespace = "",
    MyHttpClientProviderFunc = handler
        => new HttpClient(httpClientHandler)
        {
            BaseAddress = new Uri(vaultAddress)
        }
};

IVaultClient vaultClient = new VaultClient(vaultClientSettings);

// Retrieve MongoDB connection string from Vault
string mongoConnectionString = "";
try
{
    Secret<SecretData> mongoSecret = await vaultClient.V1.Secrets.KeyValue.V2
        .ReadSecretAsync(path: "mongodb", mountPoint: "secret");
    
    mongoConnectionString = mongoSecret.Data.Data["connectionString"]?.ToString() ?? "mongodb://admin:secret123@mongodb:27017/FitnessAppDB?authSource=admin";
    Console.WriteLine($"MongoDB connection string retrieved from Vault successfully");
}
catch (Exception e)
{
    Console.WriteLine($"Failed to retrieve MongoDB connection string from Vault: {e.Message}");
    // Fallback to local development connection if Vault fails
    mongoConnectionString = "mongodb://admin:secret123@mongodb:27017/FitnessAppDB?authSource=admin";
}

//retrieve JWT secret from vault
string jwtSecret = "";
try
{
    Secret<SecretData> jwtSecretData = await vaultClient.V1.Secrets.KeyValue.V2
        .ReadSecretAsync(path: "jwt", mountPoint: "secret");
    jwtSecret = jwtSecretData.Data.Data["value"]?.ToString() ?? "";
    Console.WriteLine($"JWT secret retrieved from Vault successfully");
}
catch (Exception e)
{
    Console.WriteLine($"Failed to retrieve JWT secret from Vault: {e.Message}");
    // Fallback secret for development
    jwtSecret = "fallback-jwt-secret-for-development-only-not-for-production";
}

// Configure MongoDB GUID serialization
BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));

// Add JWT Authentication
var key = Encoding.UTF8.GetBytes(jwtSecret);
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "FitnessApp",
            ValidAudience = "FitnessApp-Users",
            IssuerSigningKey = new SymmetricSecurityKey(key)
        };
    });

// Add services to the container.
builder.Services.AddControllers();

// Register MongoDB
builder.Services.AddSingleton<IMongoClient>(serviceProvider =>
{
    return new MongoClient(mongoConnectionString);
});

builder.Services.AddScoped<IMongoDatabase>(serviceProvider =>
{
    var client = serviceProvider.GetRequiredService<IMongoClient>();
    var databaseName = builder.Configuration["MongoDB:DatabaseName"] ?? "FitnessAppDB";
    return client.GetDatabase(databaseName);
});

// Register HttpClient
builder.Services.AddHttpClient();

// Register repositories
builder.Services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();





