namespace Frontend.Models;

public class Friendship
{

    public string? FriendshipId { get; set; }
    public string? SenderId { get; set; }
    public string? ReceiverId { get; set; }
    public FriendshipStatus FriendShipStatus { get; set; }
    public enum FriendshipStatus
    {
        None,
        Pending,
        Accepted,
        Declined
    }

}