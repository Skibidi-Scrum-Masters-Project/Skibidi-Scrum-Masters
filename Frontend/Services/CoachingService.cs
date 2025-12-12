namespace FitLifeFitness.Services;

public class CoachingService
{
    private readonly HttpClient _httpClient;

    public CoachingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://localhost:4000");
    }

    public async Task<HttpResponseMessage> GetSessionsAsync(string userId)
    {
        return await _httpClient.GetAsync($"/api/coaching/sessions/{userId}");
    }

    public async Task<HttpResponseMessage> BookSessionAsync(object sessionData)
    {
        return await _httpClient.PostAsJsonAsync("/api/coaching/sessions", sessionData);
    }

    public async Task<HttpResponseMessage> GetCoachesAsync()
    {
        return await _httpClient.GetAsync("/api/coaching/coaches");
    }
}
