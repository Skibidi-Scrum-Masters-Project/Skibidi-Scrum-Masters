using System.Net.Http.Headers;
using FitLifeFitness.Services;
using System.IdentityModel.Tokens.Jwt;
using FitLifeFitness.Models;

namespace FitLifeFitness.Services;

public class AuthHeaderHandler : DelegatingHandler
{
    private readonly TokenService _tokenService;
    private readonly AuthService _authService;
    private static readonly SemaphoreSlim _refreshLock = new(1, 1);

    public AuthHeaderHandler(TokenService tokenService, AuthService authService)
    {
        _tokenService = tokenService;
        _authService = authService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string? token = null;
        
        try
        {
            token = await _tokenService.GetTokenAsync();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("JavaScript interop"))
        {
            // During prerendering, JavaScript interop is not available
            // This is expected - just proceed without auth header
            Console.WriteLine("AuthHeaderHandler: Skipping auth (prerendering phase - JavaScript interop not available)");
            return await base.SendAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"AuthHeaderHandler: Error getting token: {ex.Message}");
            return await base.SendAsync(request, cancellationToken);
        }

        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("AuthHeaderHandler: No token found in storage.");
            return await base.SendAsync(request, cancellationToken);
        }

        Console.WriteLine("AuthHeaderHandler: Token retrieved successfully.");

        // Check if token is expired
        if (!IsTokenExpired(token))
        {
            // Token is valid, use it
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            // Token expired, try to refresh
            Console.WriteLine("AuthHeaderHandler: Token expired, attempting refresh...");
            var refreshed = await RefreshTokenAsync();
            
            if (refreshed)
            {
                try
                {
                    var newToken = await _tokenService.GetTokenAsync();
                    if (!string.IsNullOrEmpty(newToken))
                    {
                        Console.WriteLine("AuthHeaderHandler: Using refreshed token.");
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"AuthHeaderHandler: Error getting refreshed token: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("AuthHeaderHandler: Token refresh failed.");
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }

    public async Task<bool> RefreshTokenAsync()
    {
        string? refreshToken = null;
        string? userId = null;
        UserRole? role = null;

        try
        {
            refreshToken = await _tokenService.GetRefreshTokenAsync();
            userId = await _tokenService.GetUserIdAsync();
            role = await _tokenService.GetUserRoleAsync();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("JavaScript interop"))
        {
            Console.WriteLine("RefreshTokenAsync: Cannot refresh during prerendering");
            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"RefreshTokenAsync: Error getting token data: {ex.Message}");
            return false;
        }

        if (string.IsNullOrEmpty(refreshToken) || string.IsNullOrEmpty(userId) || role == null)
        {
            Console.WriteLine("RefreshTokenAsync: Missing required data for refresh");
            return false;
        }

        await _refreshLock.WaitAsync();
        try
        {
            var response = await _authService.RefreshTokenAsync(userId, refreshToken, role.Value);
            
            if (response.IsSuccessStatusCode)
            {
                var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if (loginResponse != null)
                {
                    await _tokenService.SaveTokenAsync(
                        loginResponse.Token,
                        loginResponse.User.Id,
                        loginResponse.User.Username,
                        loginResponse.User.Role,
                        loginResponse.RefreshToken);
                    
                    Console.WriteLine("RefreshTokenAsync: Token refreshed successfully");
                    return true;
                }
            }
            else
            {
                Console.WriteLine($"RefreshTokenAsync: Refresh failed with status {response.StatusCode}");
                await _tokenService.ClearAsync();
                
                try
                {
                    await _authService.LogoutAsync(userId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"RefreshTokenAsync: Error during logout: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"RefreshTokenAsync: Exception during refresh: {ex.Message}");
        }
        finally
        {
            _refreshLock.Release();
        }

        return false;
    }

    public bool IsTokenExpired(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            // JWT `ValidTo` is in UTC
            return jwt.ValidTo < DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"IsTokenExpired: Error parsing token: {ex.Message}");
            return true; // Treat invalid tokens as expired
        }
    }
}