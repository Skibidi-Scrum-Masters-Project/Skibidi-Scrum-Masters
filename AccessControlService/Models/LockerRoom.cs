using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace AccessControlService.Models;

public class LockerRoom
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonElement("_id")]
    public string? Id { get; set; }
   
    public int Capacity { get; set; }
    public List<Locker>? Lockers { get; set; }
}