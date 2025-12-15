namespace FitLifeFitness.Services;

public class UserService
{
    private readonly HttpClient _httpClient;

    public UserService(HttpClient httpClient)
    {
        _httpClient = httpClient;
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

    public async Task<HttpResponseMessage> GetUserByIdAsync(string userId)
    {
        return await _httpClient.GetAsync($"/api/users/{userId}");
    }

    public async Task<HttpResponseMessage> CreateUserAsync(object userData)
    {
        // Post to the trailing-slash path to avoid proxy redirects that can turn POST into GET
        return await _httpClient.PostAsJsonAsync("/api/users/", userData);
    }
}