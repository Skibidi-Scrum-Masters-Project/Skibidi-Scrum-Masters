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

    public async Task SaveTokenAsync(string token, string userId, string username, UserRole  role, string refreshToken)
    {
        try
        {
            await _localStorage.SetAsync(TokenKey, token);
            await _localStorage.SetAsync(UserIdKey, userId);
            await _localStorage.SetAsync(UsernameKey, username);
            await _localStorage.SetAsync(UserRoleKey, role.ToString());
            await _localStorage.SetAsync(RefreshTokenKey, refreshToken);


            // Clear any pending in-memory values on success
            _pendingToken = null;
            _pendingUserId = null;
            _pendingUsername = null;
            _pendingUserRole = null;
            _pendingRefreshToken = null;
        }
        catch (InvalidOperationException)
        {
            // JS interop isn't available (prerender). Buffer values in memory and try later.
            _pendingToken = token;
            _pendingUserId = userId;
            _pendingUsername = username;
            _pendingUserRole = role;
            _pendingRefreshToken = refreshToken;
        }
    }

    // Buffer token in memory without invoking JS interop (useful during prerendering).
    // (removed) Use SaveTokenAsync which will buffer on interop failures

    public async Task<string?> GetTokenAsync()
    {
        if (!string.IsNullOrEmpty(_pendingToken)) return _pendingToken;
        try
        {
            var result = await _localStorage.GetAsync<string>(TokenKey);
            return result.Success ? result.Value : null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<string?> GetUserIdAsync()
    {
        if (!string.IsNullOrEmpty(_pendingUserId)) return _pendingUserId;
        try
        {
            var result = await _localStorage.GetAsync<string>(UserIdKey);
            return result.Success ? result.Value : null;
        }
        catch
        {
            return null;
        }
    }
    public async Task<UserRole?> GetUserRoleAsync()
    {
        if (_pendingUserRole != null) return _pendingUserRole;
        try
        {
            var result = await _localStorage.GetAsync<string>(UserRoleKey);
            if (result.Success && Enum.TryParse<UserRole>(result.Value, out var role))
            {
                return role;
            }
            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<string?> GetUsernameAsync()
    {
        if (!string.IsNullOrEmpty(_pendingUsername)) return _pendingUsername;
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
        if (!string.IsNullOrEmpty(_pendingRefreshToken)) return _pendingRefreshToken;
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
        try
        {
            await _localStorage.DeleteAsync(TokenKey);
            await _localStorage.DeleteAsync(UserIdKey);
            await _localStorage.DeleteAsync(UsernameKey);
            await _localStorage.DeleteAsync(UserRoleKey);
            await _localStorage.DeleteAsync(RefreshTokenKey);
        }
        catch (InvalidOperationException)
        {
            // If interop not available, still clear pending values
        }

        _pendingToken = null;
        _pendingUserId = null;
        _pendingUsername = null;
        _pendingUserRole = null;
        _pendingRefreshToken = null;
    }

    // In-memory buffer for values that couldn't be persisted because JS interop is not available.
    private string? _pendingToken;
    private string? _pendingUserId;
    private string? _pendingUsername;
    private UserRole? _pendingUserRole;
    private string? _pendingRefreshToken;

    // Attempt to persist any pending values to ProtectedLocalStorage.
    public async Task FlushPendingAsync()
    {
        if (_pendingToken == null && _pendingUserId == null && _pendingUsername == null && _pendingUserRole == null)
            return;

        try
        {
            if (_pendingToken != null) await _localStorage.SetAsync(TokenKey, _pendingToken);
            if (_pendingUserId != null) await _localStorage.SetAsync(UserIdKey, _pendingUserId);
            if (_pendingUsername != null) await _localStorage.SetAsync(UsernameKey, _pendingUsername);
            if (_pendingUserRole != null) await _localStorage.SetAsync(UserRoleKey, _pendingUserRole.ToString()!);
            if (_pendingRefreshToken != null) await _localStorage.SetAsync(RefreshTokenKey, _pendingRefreshToken);

            _pendingToken = null;
            _pendingUserId = null;
            _pendingUsername = null;
            _pendingUserRole = null;
            _pendingRefreshToken = null;
        }
        catch (InvalidOperationException)
        {
            // Still not available; caller may retry later
        }
    }
}   