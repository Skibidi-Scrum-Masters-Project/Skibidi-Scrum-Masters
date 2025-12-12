namespace FitLifeFitness.Services;

public class UserService
{
    private readonly HttpClient _httpClient;

    public UserService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://localhost:4000");
    }

    public async Task<HttpResponseMessage> GetUserAsync(string userId)
    {
        return await _httpClient.GetAsync($"/api/users/{userId}");
    }

    public async Task<HttpResponseMessage> UpdateUserAsync(string userId, object userData)
    {
        return await _httpClient.PutAsJsonAsync($"/api/users/{userId}", userData);
    }

    public async Task<HttpResponseMessage> GetAllUsersAsync()
    {
        return await _httpClient.GetAsync("/api/users");
    }
}
