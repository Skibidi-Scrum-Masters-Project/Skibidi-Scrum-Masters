using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using System.Threading;
using System.Collections.Generic;

namespace FitLifeFitness.Services;

public class TokenAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly TokenService _tokenService;
    private readonly List<TaskCompletionSource<bool>> _waiters = new();

    public TokenAuthenticationStateProvider(TokenService tokenService)
    {
        _tokenService = tokenService;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _tokenService.GetTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        try
        {
            var claims = ParseClaimsFromJwt(token);
            var identity = new ClaimsIdentity(claims, "jwt");
            var user = new ClaimsPrincipal(identity);
            return new AuthenticationState(user);
        }
        catch
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }
    }

    public Task NotifyAuthenticationStateChangedAsync()
    {
        var authStateTask = GetAuthenticationStateAsync();
        NotifyAuthenticationStateChanged(authStateTask);
        // If the new state is authenticated, complete any waiters.
        _ = Task.Run(async () =>
        {
            try
            {
                var state = await authStateTask;
                if (state?.User?.Identity?.IsAuthenticated == true)
                {
                    lock (_waiters)
                    {
                        foreach (var w in _waiters.ToArray())
                        {
                            try { w.TrySetResult(true); } catch { }
                        }
                        _waiters.Clear();
                    }
                }
            }
            catch { }
        });

        return Task.CompletedTask;
    }

    /// <summary>
    /// Waits until the authentication state reports an authenticated user or timeout elapses.
    /// Returns true if authenticated, false if timed out or an error occurred.
    /// </summary>
    public async Task<bool> WaitForAuthenticationAsync(TimeSpan timeout)
    {
        try
        {
            var current = await GetAuthenticationStateAsync();
            if (current?.User?.Identity?.IsAuthenticated == true)
                return true;

            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            lock (_waiters)
            {
                _waiters.Add(tcs);
            }

            using var cts = new CancellationTokenSource(timeout);
            using (cts.Token.Register(() =>
            {
                if (tcs.TrySetResult(false))
                {
                    lock (_waiters) { _waiters.Remove(tcs); }
                }
            }))
            {
                return await tcs.Task.ConfigureAwait(false);
            }
        }
        catch
        {
            return false;
        }
    }

    public void MarkUserLoggedOut()
    {
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()))));
    }

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var parts = jwt.Split('.');
        if (parts.Length < 2) return Array.Empty<Claim>();

        var payload = parts[1];
        // Replace URL-safe characters
        payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
        payload = payload.Replace('-', '+').Replace('_', '/');

        var bytes = Convert.FromBase64String(payload);
        var json = Encoding.UTF8.GetString(bytes);

        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var claims = new List<Claim>();

            foreach (var property in root.EnumerateObject())
            {
                var name = property.Name;
                if (name == "role" || name == "roles")
                {
                    if (property.Value.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var role in property.Value.EnumerateArray())
                        {
                            claims.Add(new Claim(ClaimTypes.Role, role.GetString() ?? string.Empty));
                        }
                    }
                    else
                    {
                        claims.Add(new Claim(ClaimTypes.Role, property.Value.GetString() ?? string.Empty));
                    }
                }
                else
                {
                    var val = property.Value.ToString();
                    if (!string.IsNullOrEmpty(val)) claims.Add(new Claim(name, val));
                }
            }

            // Ensure a name identifier exists
            if (!claims.Any(c => c.Type == ClaimTypes.NameIdentifier) && root.TryGetProperty("sub", out var sub))
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, sub.GetString() ?? string.Empty));
            }

            return claims;
        }
        catch
        {
            return Array.Empty<Claim>();
        }
    }
}
