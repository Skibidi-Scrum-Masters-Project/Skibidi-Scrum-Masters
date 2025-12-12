    namespace FitlifeFitness.Models
 {  
    public class Booking
    {
        public string UserId { get; set; } = string.Empty;
        public int SeatNumber { get; set; }
        public DateTime CheckedInAt { get; set; } = DateTime.MinValue;
    }
}