using SocialService.Models;

namespace SocialService.Repositories;

public interface IFriendshipRepository
{
    
    //Opretter en friend request mellem 2 user.
    Task<Friendship> SendFriendRequestAsync(int senderId, int receiverId);
}