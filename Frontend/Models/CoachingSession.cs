namespace FitLifeFitness.Models;

public class CoachingSession
{
    public class SessionDto
    {
        public string? Id { get; set; }

        public string? CoachId { get; set; }

        public string? UserId { get; set; }

        public string? DescriptionForCoach { get; set; }
        
        public DateTime StartTime { get; set; }
        
        public DateTime EndTime { get; set; }

        public BookingForm? BookingForm { get; set; }

        public Status CurrentStatus { get; set; }

    }

    public enum Status
    {
        Available,
        Planned,
        Completed,
        Cancelled
    }
    
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
    
}