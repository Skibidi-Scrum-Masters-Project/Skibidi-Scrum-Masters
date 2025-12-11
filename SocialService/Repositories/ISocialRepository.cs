using FitnessApp.Shared.Models;
using SocialService.Models;

namespace SocialService.Repositories;

public interface ISocialRepository
{
    
    //Sender friendRequest mellem 2 users
    Task<Friendship> SendFriendRequestAsync(int userId, int receiverId);
    
    //Afviser en igangværende friendRequest
    Task<Friendship> DeclineFriendRequestAsync (int userId, int receiverId);
    
    //Metode til at hente AllFriends på en bruger.
    Task<IEnumerable<Friendship?>> GetAllFriends(int userId);
    
    //Metode til at hente en specfik bruger.
    Task<Friendship?> GetFriendById(int userId, int receiverId);
    
    //Metode til at Cancel en pending request.
    Task<Friendship> CancelFriendRequest (int userId, int receiverId);
    
    //Metode til at hente alle ens friend requests.
    Task<IEnumerable<Friendship>?> GetAllFriendRequests (int userId);

    Task<Friendship?> AcceptFriendRequest (int userId, int receiverId);
}