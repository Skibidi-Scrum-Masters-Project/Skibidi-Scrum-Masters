using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace CoachingService.Models;

    public class BookingForm
    {
        public DateTime CreatedAt { get; set; }
        public string Goals { get; set; }
        public string Notes { get; set; }
        public Experience Experience { get; set; }
    }

    public enum Experience
    {
        Begynder,
        Øvet,
        Ekspert
    }

