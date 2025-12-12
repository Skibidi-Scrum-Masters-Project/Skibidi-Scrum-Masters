namespace FitLifeFitness.Services;

public class ClassService
{
    private readonly HttpClient _httpClient;
    private readonly TokenService _tokenService;

    public ClassService(HttpClient httpClient, TokenService tokenService)
    {
        _httpClient = httpClient;
        _tokenService = tokenService;
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
    public async Task<HttpResponseMessage> GetClassesByUserIdAsync(string userId)
    {
        return await _httpClient.GetAsync($"/api/classes/classes/user/{userId}");
    }
    public async Task<HttpResponseMessage> CancelClassBookingForUserAsync(string classId, string userId)
    {
        return await _httpClient.PutAsync($"/api/classes/classes/{classId}/{userId}/cancel", null);
    }
    public async Task<HttpResponseMessage> GetAllAvailableClassesAsync()
    {
        return await _httpClient.GetAsync("/api/classes/classes");
    }
    public async Task<HttpResponseMessage> BookSeatForClassAsync(string classId,string userId, int seatNumber)
    {
        return await _httpClient.PutAsync($"/api/classes/classes/{classId}/{userId}/{seatNumber}", null);
    }


}
