namespace FitLifeFitness.Services;

public class AccessControlService
{
    private readonly HttpClient _httpClient;

    public AccessControlService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://localhost:4000");
    }

    public async Task<HttpResponseMessage> OpenDoorAsync(string userId)
    {
        return await _httpClient.PostAsync($"/api/accesscontrol/door/{userId}", null);
    }

    public async Task<HttpResponseMessage> CloseDoorAsync(string userId)
    {
        return await _httpClient.PutAsync($"/api/accesscontrol/door/{userId}/close", null);
    }

    public async Task<HttpResponseMessage> GetCrowdAsync()
    {
        return await _httpClient.GetAsync("/api/accesscontrol/crowd");
    }

    public async Task<HttpResponseMessage> GetAvailableLockersAsync(string lockerRoomId)
    {
        return await _httpClient.GetAsync($"/api/accesscontrol/{lockerRoomId}/available");
    }

    public async Task<HttpResponseMessage> LockLockerAsync(string lockerRoomId, string lockerId, string userId)
    {
        return await _httpClient.PutAsync($"/api/accesscontrol/{lockerRoomId}/{lockerId}/{userId}", null);
    }
}
