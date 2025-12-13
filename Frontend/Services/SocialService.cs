namespace FitLifeFitness.Services;

public class SocialService
{
    private readonly HttpClient _httpClient;

    public SocialService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://localhost:4000");
    }

    public async Task<HttpResponseMessage> GetFriendsAsync(string userId)
    {
        return await _httpClient.GetAsync($"/api/social/{userId}/friends");
    }

    public async Task<HttpResponseMessage> AddFriendAsync(string userId, string friendId)
    {
        return await _httpClient.PostAsync($"/api/social/{userId}/friends/{friendId}", null);
    }

    public async Task<HttpResponseMessage> GetPostsAsync()
    {
        return await _httpClient.GetAsync("/api/social/posts");
    }

    public async Task<HttpResponseMessage> CreatePostAsync(object postData)
    {
        return await _httpClient.PostAsJsonAsync("/api/social/posts", postData);
    }
}
