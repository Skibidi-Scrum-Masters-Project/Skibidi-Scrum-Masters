using SocialService.Models;

namespace SocialService.Repositories;

public interface ISocialRepository
{
    
    //Sender friendRequest mellem 2 users
    Task<Friendship> SendFriendRequestAsync(int senderId, int receiverId);
    
    //Afviser en igangværende friendRequest
    Task<Friendship> DeclineFriendRequestAsync (int senderId, int receiverId);
    
    //Metode til at hente AllFriends på en bruger.
    Task<IEnumerable<Friendship?>> GetAllFriends(int senderId);
    
    //Metode til at hente en specfik bruger.
    Task<Friendship?> GetFriendById(int senderId, int receiverId);
    
    //Metode til at Cancel en pending request.
    Task<Friendship> CancelFriendRequest (int senderId, int receiverId);

}