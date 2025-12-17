using System.Net.Http;
using System.Net.Http.Headers;

namespace FitLifeFitness.Services;

public class AnalyticsService
{
    private readonly HttpClient _httpClient;

    public AnalyticsService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    private void AddJwtHeader(string jwt)
    {
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", jwt);
    }

    // GET crowd count
    public async Task<HttpResponseMessage> GetCrowdCountAsync(string jwt)
    {
        AddJwtHeader(jwt);
        return await _httpClient.GetAsync("/api/analytics/crowd");
    }

    // GET class training results
    public async Task<HttpResponseMessage> GetClassResultsAsync(string userId, string jwt)
    {
        AddJwtHeader(jwt);
        return await _httpClient.GetAsync($"/api/analytics/classresult/{userId}");
    }

    // GET solo training results
    public async Task<HttpResponseMessage> GetSoloTrainingResultsAsync(string userId, string jwt)
    {
        AddJwtHeader(jwt);
        return await _httpClient.GetAsync($"/api/analytics/solotrainingresult/{userId}");
    }

    // samlet dashboard
    public async Task<HttpResponseMessage> GetDashboardAsync(string userId, string jwt)
    {
        AddJwtHeader(jwt);
        return await _httpClient.GetAsync($"/api/analytics/dashboard/{userId}");
    }

    public async Task<HttpResponseMessage> GetCompareMonthAsync(string userId, string jwt)
    {
        AddJwtHeader(jwt);
        return await _httpClient.GetAsync($"/api/analytics/compare/month/{userId}");
    }
}
