using FitLifeFitness.Components;
using FitLifeFitness.Services;
using System.IO;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add anti-forgery services
builder.Services.AddAntiforgery();

// Add protected browser storage for secure token storage
builder.Services.AddScoped<ProtectedSessionStorage>();
builder.Services.AddScoped<ProtectedLocalStorage>();

// Register TokenService
builder.Services.AddScoped<TokenService>();

// Configure the base address for HttpClient
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "http://nginx";

// Register HttpClient for services with base address
builder.Services.AddTransient<AuthHeaderHandler>();

builder.Services.AddHttpClient<AuthService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).AddHttpMessageHandler<AuthHeaderHandler>();

builder.Services.AddHttpClient<UserService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});
builder.Services.AddHttpClient<UserService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).AddHttpMessageHandler<AuthHeaderHandler>();

builder.Services.AddHttpClient<ClassService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});
builder.Services.AddHttpClient<ClassService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).AddHttpMessageHandler<AuthHeaderHandler>();

builder.Services.AddHttpClient<SocialService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});
builder.Services.AddHttpClient<SocialService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).AddHttpMessageHandler<AuthHeaderHandler>();

builder.Services.AddHttpClient<AccessControlService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});
builder.Services.AddHttpClient<AccessControlService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).AddHttpMessageHandler<AuthHeaderHandler>();

builder.Services.AddHttpClient<AnalyticsService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});
builder.Services.AddHttpClient<AnalyticsService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).AddHttpMessageHandler<AuthHeaderHandler>();

builder.Services.AddHttpClient<CoachingService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});
builder.Services.AddHttpClient<CoachingService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).AddHttpMessageHandler<AuthHeaderHandler>();

builder.Services.AddHttpClient<SoloTrainingService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});
builder.Services.AddHttpClient<SoloTrainingService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
}).AddHttpMessageHandler<AuthHeaderHandler>();

// Configure Data Protection key persistence to a path that can be mounted from Docker.
// This avoids ephemeral keys inside containers which break antiforgery/cookie decryption.
try
{
    var keyPath = builder.Configuration["DataProtection:Path"] ?? Environment.GetEnvironmentVariable("DATA_PROTECTION_PATH") ?? "/root/.aspnet/DataProtection-Keys";
    Directory.CreateDirectory(keyPath);
    var dpBuilder = builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(keyPath))
        .SetApplicationName("FitLifeFitness");

    // If a base64-encoded certificate is supplied via env var, use it to encrypt keys at rest.
    var certBase64 = Environment.GetEnvironmentVariable("DATA_PROTECTION_CERT_BASE64");
    if (!string.IsNullOrEmpty(certBase64))
    {
        try
        {
            var certBytes = Convert.FromBase64String(certBase64);
            var cert = new X509Certificate2(certBytes);
            dpBuilder.ProtectKeysWithCertificate(cert);
        }
        catch
        {
            // ignore cert load errors and fall back to unencrypted keys
        }
    }
}
catch
{
    // If DataProtection can't be configured at startup, continue without persistence (dev fallback).
}

// Configure forwarded headers for running behind nginx
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

// Use forwarded headers (must be before other middleware)
app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.MapStaticAssets();

// Add anti-forgery middleware
app.UseAntiforgery();

// Disable anti-forgery for Blazor Server endpoints (they use SignalR)
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .DisableAntiforgery();

app.Run();