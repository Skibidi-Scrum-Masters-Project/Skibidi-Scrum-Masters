using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SocialService.Models;

public class Friendship
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string FriendshipId { get; set; }
    public int SenderId { get; set; }
    public int ReceiverId { get; set; }
    public FriendshipStatus FriendShipStatus { get; set; }

    
}