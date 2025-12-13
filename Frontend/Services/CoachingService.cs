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

    
    public async Task<HttpResponseMessage> GetAllSessionsAsync()
    {
        return await _httpClient.GetAsync("/api/coaching/AllSessions");
    }

    
    public async Task<HttpResponseMessage> BookSessionAsync(object sessionData)
    {
        return await _httpClient.PutAsJsonAsync("/api/coaching/Session", sessionData);
    }
    
    public async Task<HttpResponseMessage> GetSessionByIdAsync(string id)
    {
        return await _httpClient.GetAsync($"/api/coaching/Session/{id}");
    }
    

    public async Task<HttpResponseMessage> CancelSessionAsync(string id)
    {
        // PUT request typically doesn't need content, but SendAsync allows flexibility
        return await _httpClient.PutAsync($"/api/coaching/CancelSession/{id}", content: null);
    }
    
   
    public async Task<HttpResponseMessage> CompleteSessionAsync(string id)
    {
        // PUT request typically doesn't need content
        return await _httpClient.PutAsync($"/api/coaching/CompleteSession/{id}", content: null);
    }

    
    public async Task<HttpResponseMessage> CreateSessionAsCoachAsync(object sessionData)
    {
        return await _httpClient.PostAsJsonAsync("/api/coaching/MakeSessionAsCoach", sessionData);
    }

   
    public async Task<HttpResponseMessage> DeleteSessionAsCoachAsync(string id)
    {
        return await _httpClient.DeleteAsync($"/api/coaching/RemoveSessionAsCoach/{id}");
    }
    
    
    public async Task<HttpResponseMessage> GetAvailableSessionsAsync()
    {
        return await _httpClient.GetAsync("/api/coaching/AvailableSessions");
    }
    
   
    public async Task<HttpResponseMessage> GetAvailableSessionsForCoachIdAsync(string coachId)
    {
        return await _httpClient.GetAsync($"/api/coaching/AvailableSessions/{coachId}");
    }

   
    public async Task<HttpResponseMessage> GetAllSessionsByCoachIdAsync(string coachId)
    {
        return await _httpClient.GetAsync($"/api/coaching/AllSessions/{coachId}");
    }
    
}