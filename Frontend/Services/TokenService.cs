using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using FitLifeFitness.Models;

namespace FitLifeFitness.Services;

public class TokenService
{
    private readonly ProtectedLocalStorage _localStorage;
    private const string TokenKey = "authToken";
    private const string UserIdKey = "userId";
    private const string UsernameKey = "username";
    private const string UserRoleKey = "userRole";
    private const string RefreshTokenKey = "refreshToken";

    public TokenService(ProtectedLocalStorage localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task SaveTokenAsync(string token, string userId, string username, UserRole role, string refreshToken)
    {
        Console.WriteLine("=== TokenService.SaveTokenAsync START ===");
        Console.WriteLine($"Token: {token?.Substring(0, Math.Min(20, token?.Length ?? 0))}...");
        Console.WriteLine($"UserId: {userId}");
        Console.WriteLine($"Username: {username}");
        Console.WriteLine($"Role: {role}");
        Console.WriteLine($"RefreshToken: {refreshToken?.Substring(0, Math.Min(20, refreshToken?.Length ?? 0))}...");
        
        try
        {
            await _localStorage.SetAsync(TokenKey, token);
            Console.WriteLine("✓ Token saved");
            
            await _localStorage.SetAsync(UserIdKey, userId);
            Console.WriteLine("✓ UserId saved");
            
            await _localStorage.SetAsync(UsernameKey, username);
            Console.WriteLine("✓ Username saved");
            
            await _localStorage.SetAsync(UserRoleKey, role.ToString());
            Console.WriteLine("✓ UserRole saved");
            
            await _localStorage.SetAsync(RefreshTokenKey, refreshToken);
            Console.WriteLine("✓ RefreshToken saved");

            // Verify immediately
            var verifyToken = await GetTokenAsync();
            var verifyUserId = await GetUserIdAsync();
            Console.WriteLine($"VERIFY - Token retrieved: {verifyToken?.Substring(0, Math.Min(20, verifyToken?.Length ?? 0))}...");
            Console.WriteLine($"VERIFY - UserId retrieved: {verifyUserId}");
            Console.WriteLine("=== TokenService.SaveTokenAsync SUCCESS ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"!!! TokenService.SaveTokenAsync FAILED: {ex.Message}");
            Console.WriteLine($"!!! Exception Type: {ex.GetType().Name}");
            Console.WriteLine($"!!! Stack: {ex.StackTrace}");
            throw;
        }
    }

    public async Task<string?> GetTokenAsync()
    {
        try
        {
            Console.WriteLine("TokenService.GetTokenAsync called");
            var result = await _localStorage.GetAsync<string>(TokenKey);
            Console.WriteLine($"GetTokenAsync result - Success: {result.Success}, HasValue: {!string.IsNullOrEmpty(result.Value)}");
            return result.Success ? result.Value : null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"!!! GetTokenAsync error: {ex.Message}");
            return null;
        }
    }

    public async Task<string?> GetUserIdAsync()
    {
        try
        {
            Console.WriteLine("TokenService.GetUserIdAsync called");
            var result = await _localStorage.GetAsync<string>(UserIdKey);
            Console.WriteLine($"GetUserIdAsync result - Success: {result.Success}, Value: {result.Value}");
            return result.Success ? result.Value : null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"!!! GetUserIdAsync error: {ex.Message}");
            return null;
        }
    }

    public async Task<UserRole?> GetUserRoleAsync()
    {
        try
        {
            Console.WriteLine("TokenService.GetUserRoleAsync called");
            var result = await _localStorage.GetAsync<string>(UserRoleKey);
            if (result.Success && Enum.TryParse<UserRole>(result.Value, out var role))
            {
                Console.WriteLine($"GetUserRoleAsync result - Success: true, Role: {role}");
                return role;
            }
            Console.WriteLine($"GetUserRoleAsync result - Success: {result.Success}, ParseFailed: {!string.IsNullOrEmpty(result.Value)}");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"!!! GetUserRoleAsync error: {ex.Message}");
            return null;
        }
    }

    public async Task<string?> GetUsernameAsync()
    {
        try
        {
            var result = await _localStorage.GetAsync<string>(UsernameKey);
            return result.Success ? result.Value : null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<string?> GetRefreshTokenAsync()
    {
        try
        {
            var result = await _localStorage.GetAsync<string>(RefreshTokenKey);
            return result.Success ? result.Value : null;
        }
        catch
        {
            return null;
        }
    }

    public async Task ClearAsync()
    {
        Console.WriteLine("=== TokenService.ClearAsync START ===");
        try
        {
            await _localStorage.DeleteAsync(TokenKey);
            await _localStorage.DeleteAsync(UserIdKey);
            await _localStorage.DeleteAsync(UsernameKey);
            await _localStorage.DeleteAsync(UserRoleKey);
            await _localStorage.DeleteAsync(RefreshTokenKey);
            Console.WriteLine("=== TokenService.ClearAsync SUCCESS ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"!!! ClearAsync error: {ex.Message}");
        }
    }
}