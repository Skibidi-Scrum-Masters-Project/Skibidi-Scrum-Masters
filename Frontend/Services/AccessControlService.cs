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

    private void AddJwtHeader(string jwt)
    {
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", jwt);
    }

    public Task<HttpResponseMessage> OpenDoorAsync(string userId, string jwt)
    {
        AddJwtHeader(jwt);
        return _httpClient.PostAsync($"/api/accesscontrol/door/{userId}", null);
    }

    public Task<HttpResponseMessage> CloseDoorAsync(string userId, string jwt)
    {
        AddJwtHeader(jwt);
        return _httpClient.PutAsync($"/api/accesscontrol/door/{userId}/close", null);
    }

    public Task<HttpResponseMessage> GetCrowdAsync(string jwt)
    {
        AddJwtHeader(jwt);
        return _httpClient.GetAsync("/api/accesscontrol/crowd");
    }

    public Task<HttpResponseMessage> GetAvailableLockersAsync(string lockerRoomId, string jwt)
    {
        AddJwtHeader(jwt);
        return _httpClient.GetAsync($"/api/accesscontrol/{lockerRoomId}/available");
    }

    public Task<HttpResponseMessage> GetUserStatusAsync(string userId, string jwt)
    {
        AddJwtHeader(jwt);
        return _httpClient.GetAsync($"/api/accesscontrol/userstatus/{userId}");
    }

    public Task<HttpResponseMessage> GetLockerForUserAsync(string lockerRoomId, string userId, string jwt)
    {
        AddJwtHeader(jwt);
        return _httpClient.GetAsync($"/api/accesscontrol/{lockerRoomId}/{userId}");
    }

    public Task<HttpResponseMessage> LockLockerAsync(string lockerRoomId, string lockerId, string userId, string jwt)
    {
        AddJwtHeader(jwt);
        return _httpClient.PutAsync($"/api/accesscontrol/{lockerRoomId}/{lockerId}/{userId}", null);
    }

    public Task<HttpResponseMessage> OpenLockerAsync(string lockerRoomId, string lockerId, string userId, string jwt)
    {
        AddJwtHeader(jwt);
        return _httpClient.PutAsync(
            $"/api/accesscontrol/{lockerRoomId}/{lockerId}/{userId}/open",
            null
        );
    }

    public Task<HttpResponseMessage> GetLockerRoomIdAsync(string jwt)
    {
        AddJwtHeader(jwt);
        return _httpClient.GetAsync("/api/accesscontrol/LockerRoomId");
    }
}
