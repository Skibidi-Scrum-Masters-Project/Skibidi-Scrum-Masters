using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace AccessControlService.Models;

public class EntryPoint
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonElement("_id")]
    public string? Id { get; set; }
    public string? UserId { get; set; }
    public DateTime EnteredAt { get; set; }
    public DateTime ExitedAt { get; set; }
}