using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace FitLifeFitness.Services;

public class CoachingService
{
    private readonly HttpClient _httpClient;

    public CoachingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    private void AddJwtHeader(string jwt)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
    }

    public async Task<HttpResponseMessage> GetAllSessionsAsync(string jwt)
    {
        AddJwtHeader(jwt);
        return await _httpClient.GetAsync("/api/coaching/AllSessions");
    }

    public async Task<HttpResponseMessage> BookSessionAsync(object sessionData, string jwt)
    {
        AddJwtHeader(jwt);
        return await _httpClient.PutAsJsonAsync("/api/coaching/Session", sessionData);
    }

    public async Task<HttpResponseMessage> GetSessionByIdAsync(string id, string jwt)
    {
        AddJwtHeader(jwt);
        return await _httpClient.GetAsync($"/api/coaching/Session/{id}");
    }

    public async Task<HttpResponseMessage> CancelSessionAsync(string id, string jwt)
    {
        AddJwtHeader(jwt);
        return await _httpClient.PutAsync($"/api/coaching/CancelSession/{id}", null);
    }

    public async Task<HttpResponseMessage> CompleteSessionAsync(string id, string jwt)
    {
        AddJwtHeader(jwt);
        return await _httpClient.PutAsync($"/api/coaching/CompleteSession/{id}", null);
    }

    public async Task<HttpResponseMessage> CreateSessionAsCoachAsync(object sessionData, string jwt)
    {
        AddJwtHeader(jwt);
        return await _httpClient.PostAsJsonAsync("/api/coaching/MakeSessionAsCoach", sessionData);
    }

    public async Task<HttpResponseMessage> DeleteSessionAsCoachAsync(string id, string jwt)
    {
        AddJwtHeader(jwt);
        return await _httpClient.DeleteAsync($"/api/coaching/RemoveSessionAsCoach/{id}");
    }

    public async Task<HttpResponseMessage> GetAvailableSessionsAsync(string jwt)
    {
        AddJwtHeader(jwt);
        return await _httpClient.GetAsync("/api/coaching/AvailableSessions");
    }

    public async Task<HttpResponseMessage> GetAvailableSessionsForCoachIdAsync(string coachId, string jwt)
    {
        AddJwtHeader(jwt);
        return await _httpClient.GetAsync($"/api/coaching/AvailableSessions/{coachId}");
    }

    public async Task<HttpResponseMessage> GetAllSessionsByCoachIdAsync(string coachId, string jwt)
    {
        AddJwtHeader(jwt);
        return await _httpClient.GetAsync($"/api/coaching/AllSessions/{coachId}");
    }
}
