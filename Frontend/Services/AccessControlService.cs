namespace FitLifeFitness.Services;

public class AccessControlService
{
    private readonly HttpClient _httpClient;

    public AccessControlService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<HttpResponseMessage> OpenDoorAsync(string userId)
        => _httpClient.PostAsync($"/api/accesscontrol/door/{userId}", null);

    public Task<HttpResponseMessage> CloseDoorAsync(string userId)
        => _httpClient.PutAsync($"/api/accesscontrol/door/{userId}/close", null);

    public Task<HttpResponseMessage> GetCrowdAsync()
        => _httpClient.GetAsync("/api/accesscontrol/crowd");

    public Task<HttpResponseMessage> GetAvailableLockersAsync(string lockerRoomId)
        => _httpClient.GetAsync($"/api/accesscontrol/{lockerRoomId}/available");
    
    public Task<HttpResponseMessage> EvaluateStatus(string userId)
        => _httpClient.GetAsync($"/api/userstatus/{userId}");
    
    public Task<HttpResponseMessage> GetLockerForUserAsync(
        string lockerRoomId,
        string userId)
        => _httpClient.GetAsync(
            $"/api/accesscontrol/locker/{lockerRoomId}/user/{userId}");
    

    public Task<HttpResponseMessage> LockLockerAsync(
        string lockerRoomId,
        string lockerId,
        string userId)
        => _httpClient.PutAsync(
            $"/api/accesscontrol/{lockerRoomId}/{lockerId}/{userId}",
            null);

    public Task<HttpResponseMessage> OpenLockerAsync(
        string lockerRoomId,
        string lockerId,
        string userId)
        => _httpClient.PutAsync(
            $"/api/accesscontrol/{lockerRoomId}/{lockerId}/{userId}/open",
            null);
}