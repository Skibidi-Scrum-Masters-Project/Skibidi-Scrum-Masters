using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SocialService.Models;

public class Friendship
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? FriendshipId { get; set; }
    public string SenderId { get; set; }
    public string ReceiverId { get; set; }
    public FriendshipStatus FriendShipStatus { get; set; }

    
}

public enum FriendshipStatus
{
    None,
    Pending, 
    Accepted,
    Declined
}