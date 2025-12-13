using FitLifeFitness.Components;
using FitLifeFitness.Services;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

// Add Razor Components with Interactive Server
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options =>
    {
        // Keep disconnected circuits alive for 3 minutes to handle reconnections
        options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromMinutes(3);
        options.DisconnectedCircuitMaxRetained = 100;
        options.JSInteropDefaultCallTimeout = TimeSpan.FromMinutes(1);
    });

// Anti-forgery services
builder.Services.AddAntiforgery();

// Protected Browser Storage (Scoped per circuit)
builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddScoped<ProtectedLocalStorage>();

// Application Services (Scoped per circuit)
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<AuthHeaderHandler>();

// API Base URL Configuration
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] 
    ?? Environment.GetEnvironmentVariable("API_BASE_URL") 
    ?? "http://nginx";

Console.WriteLine($"API Base URL: {apiBaseUrl}");

// HttpClient for AuthService (no auth header needed - used for login)
builder.Services.AddHttpClient<AuthService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// HttpClients for authenticated services (with AuthHeaderHandler)
builder.Services.AddHttpClient<UserService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<AuthHeaderHandler>();

builder.Services.AddHttpClient<ClassService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<AuthHeaderHandler>();

builder.Services.AddHttpClient<SocialService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<AuthHeaderHandler>();

builder.Services.AddHttpClient<AccessControlService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<AuthHeaderHandler>();

builder.Services.AddHttpClient<AnalyticsService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<AuthHeaderHandler>();

builder.Services.AddHttpClient<CoachingService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<AuthHeaderHandler>();

builder.Services.AddHttpClient<SoloTrainingService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(30);
}).AddHttpMessageHandler<AuthHeaderHandler>();

// Data Protection Configuration
ConfigureDataProtection(builder.Services, builder.Configuration);

// Forwarded Headers for reverse proxy (nginx)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor 
        | ForwardedHeaders.XForwardedProto 
        | ForwardedHeaders.XForwardedHost;
    
    // Trust all proxies (adjust for production security)
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

// Apply forwarded headers FIRST (before other middleware)
app.UseForwardedHeaders();

// Exception handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

// HTTPS redirection
app.UseHttpsRedirection();

// Static files
app.MapStaticAssets();

// Anti-forgery middleware
app.UseAntiforgery();

// Map Razor Components
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .DisableAntiforgery(); // Blazor Server uses SignalR, not traditional forms

Console.WriteLine("FitLife Fitness application started successfully");
app.Run();

// Helper method for Data Protection configuration
static void ConfigureDataProtection(IServiceCollection services, IConfiguration configuration)
{
    try
    {
        // Determine key storage path
        var keyPath = configuration["DataProtection:Path"] 
            ?? Environment.GetEnvironmentVariable("DATA_PROTECTION_PATH") 
            ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".aspnet", "DataProtection-Keys");

        // Ensure directory exists
        Directory.CreateDirectory(keyPath);
        Console.WriteLine($"Data Protection keys path: {keyPath}");

        // Configure Data Protection
        var dpBuilder = services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(keyPath))
            .SetApplicationName("FitLifeFitness")
            .SetDefaultKeyLifetime(TimeSpan.FromDays(90));

        // Optional: Encrypt keys at rest with certificate
        var certBase64 = Environment.GetEnvironmentVariable("DATA_PROTECTION_CERT_BASE64");
        if (!string.IsNullOrWhiteSpace(certBase64))
        {
            try
            {
                var certBytes = Convert.FromBase64String(certBase64);
                var cert = new X509Certificate2(certBytes);
                dpBuilder.ProtectKeysWithCertificate(cert);
                Console.WriteLine("Data Protection keys encrypted with certificate");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not load Data Protection certificate: {ex.Message}");
                Console.WriteLine("Continuing with unencrypted keys at rest");
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Warning: Data Protection configuration failed: {ex.Message}");
        Console.WriteLine("Continuing with ephemeral keys (dev fallback)");
    }
}