namespace FitLifeFitness.Services;

public class AuthService
{
    private readonly HttpClient _http;
    private readonly TokenService _tokenService;
    
    public AuthService(HttpClient httpClient, TokenService tokenService)
    {
        _http = httpClient;
        _tokenService = tokenService;
    }
    
    public async Task<HttpResponseMessage> LoginAsync(string username, string password)
    {
        var loginData = new 
        { 
            Username = username,
            Password = password 
        };
        
        return await _http.PostAsJsonAsync("/api/auth/login", loginData);
    }
    
    public async Task<HttpResponseMessage> RegisterAsync(string email, string password, string name)
    {
        return await _http.PostAsJsonAsync("/api/auth/register", new { email, password, name });
    }
    
    public async Task LogoutAsync()
    {
        await _tokenService.ClearAsync();
    }
    
    // Helper method to add token to requests
    public async Task AddAuthorizationHeaderAsync()
    {
        var token = await _tokenService.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            _http.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }
    }
}