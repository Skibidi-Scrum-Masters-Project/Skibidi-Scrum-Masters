namespace FitLifeFitness.Services;

public class AnalyticsService
{
    private readonly HttpClient _httpClient;

    public AnalyticsService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // GET crowd count
    public async Task<HttpResponseMessage> GetCrowdCountAsync()
    {
        return await _httpClient.GetAsync("/api/analytics/crowd");
    }

    // GET class training results
    public async Task<HttpResponseMessage> GetClassResultsAsync(string userId)
    {
        return await _httpClient.GetAsync($"/api/analytics/classresult/{userId}");
    }

    // GET solo training results
    public async Task<HttpResponseMessage> GetSoloTrainingResultsAsync(string userId)
    {
        return await _httpClient.GetAsync($"/api/analytics/solotrainingresult/{userId}");
    }
}