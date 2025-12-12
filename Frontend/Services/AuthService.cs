namespace FitLifeFitness.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://localhost:4000");
    }

    public async Task<HttpResponseMessage> LoginAsync(string email, string password)
    {
        var loginData = new { email, password };
        return await _httpClient.PostAsJsonAsync("/api/auth/login", loginData);
    }

    public async Task<HttpResponseMessage> RegisterAsync(string email, string password, string name)
    {
        var registerData = new { email, password, name };
        return await _httpClient.PostAsJsonAsync("/api/auth/register", registerData);
    }
}
