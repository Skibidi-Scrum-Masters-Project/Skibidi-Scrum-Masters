using System.Net.Http;
using System.Net.Http.Json;

namespace FitLifeFitness.Services;

public class SocialService
{
    private readonly HttpClient _http;

    public SocialService(HttpClient http)
    {
        _http = http;
    }

    // friends
    public async Task<HttpResponseMessage> GetAllFriendsAsync(string userId)
    {
        return await _http.GetAsync($"/api/social/{userId}/friends");
    }

    public async Task<HttpResponseMessage> GetFriendByIdAsync(string userId, string receiverId)
    {
        return await _http.GetAsync($"/api/social/{userId}/friends/{receiverId}");
    }

    // friend requests
    public async Task<HttpResponseMessage> SendFriendRequestAsync(string userId, string receiverId)
    {
        return await _http.PostAsync($"/api/social/{userId}/sendFriendrequest/{receiverId}", content: null);
    }

    public async Task<HttpResponseMessage> AcceptFriendRequestAsync(string userId, string receiverId)
    {
        return await _http.PutAsync($"/api/social/accept/{userId}/{receiverId}", content: null);
    }

    public async Task<HttpResponseMessage> DeclineFriendRequestAsync(string userId, string receiverId)
    {
        return await _http.PutAsync($"/api/social/declineRequest/{userId}/{receiverId}", content: null);
    }

    public async Task<HttpResponseMessage> CancelFriendRequestAsync(string userId, string receiverId)
    {
        return await _http.PutAsync($"/api/social/{userId}/cancel/{receiverId}", content: null);
    }

    public async Task<HttpResponseMessage> GetOutgoingFriendRequestsAsync(string userId)
    {
        return await _http.GetAsync($"/api/social/friendrequests/outgoing/{userId}");
    }

    public async Task<HttpResponseMessage> GetIncomingFriendRequestsAsync(string userId)
    {
        return await _http.GetAsync($"/api/social/friendrequests/incoming/{userId}");
    }

    // posts
    public async Task<HttpResponseMessage> PostAPostAsync(object sessionData)
    {
        return await _http.PostAsJsonAsync("/api/social/PostAPost", sessionData);
    }

    public async Task<HttpResponseMessage> RemoveAPostAsync(string postId)
    {
        return await _http.DeleteAsync($"/api/social/RemoveAPost/{postId}");
    }

    public async Task<HttpResponseMessage> EditAPostAsync(object sessionData)
    {
        return await _http.PutAsJsonAsync("/api/social/EditAPost", sessionData);
    }

    public async Task<HttpResponseMessage> AddACommentToPostAsync(string postId, object sessionData)
    {
        return await _http.PutAsJsonAsync($"/api/social/AddACommentToPost/{postId}", sessionData);
    }

    public async Task<HttpResponseMessage> RemoveACommentFromPost(string postId, string commentId)
    {
        return await _http.DeleteAsync($"/api/social/RemoveACommentFromPost/{postId}/{commentId}");
    }
    
    public async Task<HttpResponseMessage> EditCommentAsync(string postId, object sessionData)
    {
        return await _http.PutAsJsonAsync($"/api/social/EditComment/{postId}", sessionData);
    }

    public async Task<HttpResponseMessage> SeeAllCommentForPostAsync(string postId)
    {
        return await _http.GetAsync($"/api/social/SeeAllCommentForPost/{postId}");
    }
    
    public async Task<HttpResponseMessage> SeeSpecficPostByPostId(string postId)
    {
        return await _http.GetAsync($"/api/social/SeeSpecficPostByPostId/{postId}");
    }

    public async Task<HttpResponseMessage> SeeAllPostsForUserAsync(string userId)
    {
        return await _http.GetAsync($"/api/social/SeeAllPostsForUser/{userId}");
    }

    public async Task<HttpResponseMessage> SeeAllDraftPostsForUserAsync(string userId)
    {
        return await _http.GetAsync($"/api/social/SeeAllDraftPostsForUser/{userId}");
    }

    public async Task<HttpResponseMessage> ChangeDraftStatusForPostAsync(string postId)
    {
        return await _http.PutAsync($"/api/social/ChangeDraftStatusForPost/{postId}", content: null);
    }

    public async Task<HttpResponseMessage> SeeAllFriendsPostsAsync(string userId)
    {
        return await _http.GetAsync($"/api/social/SeeAllFriendsPosts/{userId}");
    }
}
