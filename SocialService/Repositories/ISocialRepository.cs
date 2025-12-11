using FitnessApp.Shared.Models;
using SocialService.Models;

namespace SocialService.Repositories;

public interface ISocialRepository
{
    
    //Sender friendRequest mellem 2 users
    Task<Friendship> SendFriendRequestAsync(int userId, int receiverId);
    
    //Metode til at acceptere en friend request
    Task<Friendship?> AcceptFriendRequest (int userId, int receiverId);
    
    //Metode til at afvise en friend request
    Task<Friendship> DeclineFriendRequestAsync (int userId, int receiverId);
    
    //Metode til at hente AllFriends på en bruger.
    Task<IEnumerable<Friendship?>> GetAllFriends(int userId);
    
    //Metode til at hente en specfik bruger.
    Task<Friendship?> GetFriendById(int userId, int receiverId);
    
    //Metode til at Cancel en pending request.
    Task<Friendship> CancelFriendRequest (int userId, int receiverId);
    
    //Metode til at hente alle ens sendte friend requests.
    Task<IEnumerable<Friendship>?> GetOutgoingFriendRequestsAsync (int userId);
    
    //Metode til at hente alle friend requests der er sendt til user
    Task<IEnumerable<Friendship>?> GetAllIncomingFriendRequests (int userId);
    
    
    
    
    
    
    
    //Post delen af SocialService
    
    
    //Metode til at oprette et post
    Task<Post> PostAPost(Post post);
    
    //Metode til at fjerne et post
    Task<Post> RemoveAPost(string postId);
    
    //Metode til at redigere et post
    Task<Post> EditAPost(Post post, int currentUserId);
}