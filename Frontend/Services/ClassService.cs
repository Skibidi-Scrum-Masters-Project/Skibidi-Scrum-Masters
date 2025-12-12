namespace FitLifeFitness.Services;

public class ClassService
{
    private readonly HttpClient _httpClient;

    public ClassService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://localhost:4000");
    }

    public async Task<HttpResponseMessage> GetAllClassesAsync()
    {
        return await _httpClient.GetAsync("/api/classes/classes");
    }

    public async Task<HttpResponseMessage> GetClassByIdAsync(string classId)
    {
        return await _httpClient.GetAsync($"/api/classes/classes/{classId}");
    }

    public async Task<HttpResponseMessage> CreateClassAsync(object classData)
    {
        return await _httpClient.PostAsJsonAsync("/api/classes/classes", classData);
    }

    public async Task<HttpResponseMessage> JoinClassAsync(string classId, string userId)
    {
        return await _httpClient.PutAsync($"/api/classes/classes/{classId}/{userId}", null);
    }

    public async Task<HttpResponseMessage> FinishClassAsync(string classId)
    {
        return await _httpClient.PostAsync($"/api/classes/classes/{classId}/finish", null);
    }
}
