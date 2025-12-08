using SocialService.Models;

namespace SocialService.Repositories;

public interface IFriendshipRepository
{
    
    //Sender friendRequest mellem 2 users
    Task<Friendship> SendFriendRequestAsync(int senderId, int receiverId);
    
    //Afviser en igangværende friendRequest
    Task<Friendship> DeclineFriendRequestAsync (int senderId, int receiverId);
}