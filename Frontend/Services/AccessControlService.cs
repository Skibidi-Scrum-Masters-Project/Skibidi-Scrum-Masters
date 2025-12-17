using System.Net.Http;
using System.Net.Http.Headers;

namespace FitLifeFitness.Services;

public class AccessControlService
{
    private readonly HttpClient _httpClient;

    public AccessControlService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public Task<HttpResponseMessage> OpenDoorAsync(string userId)
    {
        return _httpClient.PostAsync($"/api/accesscontrol/door/{userId}", null);
    }

    public Task<HttpResponseMessage> CloseDoorAsync(string userId)
    {
        return _httpClient.PutAsync($"/api/accesscontrol/door/{userId}/close", null);
    }

    public Task<HttpResponseMessage> GetCrowdAsync()
    {
        return _httpClient.GetAsync("/api/accesscontrol/crowd");
    }

    public Task<HttpResponseMessage> GetAvailableLockersAsync(string lockerRoomId)
    {
        return _httpClient.GetAsync($"/api/accesscontrol/{lockerRoomId}/available");
    }

    public Task<HttpResponseMessage> GetUserStatus(string userId)
    {
        return _httpClient.GetAsync($"/api/accesscontrol/userstatus/{userId}");
    }

    public Task<HttpResponseMessage> GetLockerForUserAsync(string lockerRoomId, string userId)
    {
        return _httpClient.GetAsync($"/api/accesscontrol/{lockerRoomId}/{userId}");
    }

    public Task<HttpResponseMessage> LockLockerAsync(string lockerRoomId, string lockerId, string userId)
    {
        return _httpClient.PutAsync($"/api/accesscontrol/{lockerRoomId}/{lockerId}/{userId}", null);
    }

    public Task<HttpResponseMessage> OpenLockerAsync(string lockerRoomId, string lockerId, string userId)
    {
        return _httpClient.PutAsync($"/api/accesscontrol/{lockerRoomId}/{lockerId}/{userId}/open", null);
    }

    public Task<HttpResponseMessage> GetLockerRoomId()
    {
        return _httpClient.GetAsync("/api/accesscontrol/LockerRoomId");
    }
}