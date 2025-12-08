using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CoachingService.Models;

public class Session
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public BookingForm BookingForm { get; set; }
    public Status CurrentStatus { get; set; }
    
    public enum Status
    {
        Planned,
        Booked,
        Completed,
        Canceled
    }
}
