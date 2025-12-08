namespace SocialService.Models;

public class Friendship
{
    public int FriendshipId { get; set; }
    public int SenderId { get; set; }
    public int ReceiverId { get; set; }
    public FriendshipStatus FriendShipStatus { get; set; }

    
}