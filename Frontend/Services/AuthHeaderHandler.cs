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
        var token = await _tokenService.GetTokenAsync();
        if (!string.IsNullOrEmpty(token) && !IsTokenExpired(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        else if (!string.IsNullOrEmpty(token) && IsTokenExpired(token))
        {
            await RefreshTokenAsync();
            var newToken = await _tokenService.GetTokenAsync();
            if (!string.IsNullOrEmpty(newToken))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }
    public async Task RefreshTokenAsync()
    {
        var refreshToken = await _tokenService.GetRefreshTokenAsync();
        var userId = await _tokenService.GetUserIdAsync();
        var role = await _tokenService.GetUserRoleAsync();
        if (string.IsNullOrEmpty(refreshToken))
            return;
        if (string.IsNullOrEmpty(userId))
            return;
        if (role == null)
            return;
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
                }
            }
            if (!response.IsSuccessStatusCode)
            {
                await _tokenService.ClearAsync();
                await _authService.LogoutAsync(userId);
            }
        }
        finally
        {
            _refreshLock.Release();
        }


    }
    public bool IsTokenExpired(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        // JWT `ValidTo` is in UTC
        return jwt.ValidTo < DateTime.UtcNow;
    }
}
