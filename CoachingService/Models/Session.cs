using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace CoachingService.Models;


public class Session
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public string CoachId { get; set; }
    
    public string UserId { get; set; }
    
    public string DescriptionForCoach { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    
    public BookingForm? BookingForm { get; set; }

    public Status CurrentStatus { get; set; }

    public enum Status
    {
        Available,
        Planned,
        Completed,
        Cancelled
    }
}
