using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace FitLifeFitness.Services;

public class SocialService
{
    private readonly HttpClient _httpClient;

    public SocialService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    private void AddJwtHeader(string jwt)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
    }

    // friends
    public async Task<HttpResponseMessage> GetAllFriendsAsync(string userId, string jwt)
    {
        AddJwtHeader(jwt);
        return await _httpClient.GetAsync($"/api/social/{userId}/friends");
    }

    public async Task<HttpResponseMessage> GetFriendByIdAsync(string userId, string receiverId, string jwt)
    {
        AddJwtHeader(jwt);
        return await _httpClient.GetAsync($"/api/social/{userId}/friends/{receiverId}");
    }

    // friend requests
    public async Task<HttpResponseMessage> SendFriendRequestAsync(string userId, string receiverId, string jwt)
    {
        AddJwtHeader(jwt);
        return await _httpClient.PostAsync($"/api/social/{userId}/sendFriendrequest/{receiverId}", null);
    }

    public async Task<HttpResponseMessage> AcceptFriendRequestAsync(string userId, string receiverId, string jwt)
    {
        AddJwtHeader(jwt);
        return await _httpClient.PutAsync($"/api/social/accept/{userId}/{receiverId}", null);
    }

    public async Task<HttpResponseMessage> DeclineFriendRequestAsync(string userId, string receiverId, string jwt)
    {
        AddJwtHeader(jwt);
        return await _httpClient.PutAsync($"/api/social/declineRequest/{userId}/{receiverId}", null);
    }

    public async Task<HttpResponseMessage> CancelFriendRequestAsync(string userId, string receiverId, string jwt)
    {
        AddJwtHeader(jwt);
        return await _httpClient.DeleteAsync($"/api/social/{userId}/cancel/{receiverId}");
    }

    public async Task<HttpResponseMessage> GetOutgoingFriendRequestsAsync(string userId, string jwt)
    {
        AddJwtHeader(jwt);
        return await _httpClient.GetAsync($"/api/social/friendrequests/outgoing/{userId}");
    }

    public async Task<HttpResponseMessage> GetIncomingFriendRequestsAsync(string userId, string jwt)
    {
        AddJwtHeader(jwt);
        return await _httpClient.GetAsync($"/api/social/friendrequests/incoming/{userId}");
    }

    // posts
    public async Task<HttpResponseMessage> PostAPostAsync(object sessionData, string jwt)
    {
        AddJwtHeader(jwt);
        return await _httpClient.PostAsJsonAsync("/api/social/PostAPost", sessionData);
    }

    public async Task<HttpResponseMessage> RemoveAPostAsync(string postId, string jwt)
    {
        AddJwtHeader(jwt);
        return await _httpClient.DeleteAsync($"/api/social/RemoveAPost/{postId}");
    }

    public async Task<HttpResponseMessage> EditAPostAsync(object sessionData, string jwt)
    {
        AddJwtHeader(jwt);
        return await _httpClient.PutAsJsonAsync("/api/social/EditAPost", sessionData);
    }

    public async Task<HttpResponseMessage> AddACommentToPostAsync(string postId, object sessionData, string jwt)
    {
        AddJwtHeader(jwt);
        return await _httpClient.PutAsJsonAsync($"/api/social/AddACommentToPost/{postId}", sessionData);
    }

    public async Task<HttpResponseMessage> RemoveACommentFromPostAsync(string postId, string commentId, string jwt)
    {
        AddJwtHeader(jwt);
        return await _httpClient.DeleteAsync($"/api/social/RemoveACommentFromPost/{postId}/{commentId}");
    }

    public async Task<HttpResponseMessage> EditCommentAsync(string postId, object sessionData, string jwt)
    {
        AddJwtHeader(jwt);
        return await _httpClient.PutAsJsonAsync($"/api/social/EditComment/{postId}", sessionData);
    }

    public async Task<HttpResponseMessage> SeeAllCommentForPostAsync(string postId, string jwt)
    {
        AddJwtHeader(jwt);
        return await _httpClient.GetAsync($"/api/social/SeeAllCommentForPost/{postId}");
    }

    public async Task<HttpResponseMessage> SeeSpecificPostByPostIdAsync(string postId, string jwt)
    {
        AddJwtHeader(jwt);
        return await _httpClient.GetAsync($"/api/social/SeeSpecficPostByPostId/{postId}");
    }

    public async Task<HttpResponseMessage> SeeAllPostsForUserAsync(string userId, string jwt)
    {
        AddJwtHeader(jwt);
        return await _httpClient.GetAsync($"/api/social/SeeAllPostsForUser/{userId}");
    }

    public async Task<HttpResponseMessage> SeeAllDraftPostsForUserAsync(string userId, string jwt)
    {
        AddJwtHeader(jwt);
        return await _httpClient.GetAsync($"/api/social/SeeAllDraftPostsForUser/{userId}");
    }

    public async Task<HttpResponseMessage> ChangeDraftStatusForPostAsync(string postId, string jwt)
    {
        AddJwtHeader(jwt);
        return await _httpClient.PutAsync($"/api/social/ChangeDraftStatusForPost/{postId}", null);
    }

    public async Task<HttpResponseMessage> SeeAllFriendsPostsAsync(string userId, string jwt)
    {
        AddJwtHeader(jwt);
        return await _httpClient.GetAsync($"/api/social/SeeAllFriendsPosts/{userId}");
    }
}
