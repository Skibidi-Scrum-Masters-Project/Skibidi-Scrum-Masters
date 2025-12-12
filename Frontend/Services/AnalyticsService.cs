namespace FitLifeFitness.Services;

public class AnalyticsService
{
    private readonly HttpClient _httpClient;

    public AnalyticsService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://localhost:4000");
    }

    public async Task<HttpResponseMessage> GetCrowdCountAsync()
    {
        return await _httpClient.GetAsync("/api/analytics/crowd");
    }

    public async Task<HttpResponseMessage> GetClassAnalyticsAsync(string classId, string userId, double calories, string category, int duration, DateTime date)
    {
        return await _httpClient.PostAsync($"/api/analytics/{classId}/{userId}/{calories}/{category}/{duration}/{date:yyyy-MM-dd}", null);
    }
}
