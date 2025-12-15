namespace FitLifeFitness.Models;


    public class ClassResultDTO
    {
        
        public string? Id { get; set; }
        public string ClassId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public Category Category { get; set; }
        public Double CaloriesBurned { get; set; }
        public Double Watt { get; set; }
        public int DurationMin { get; set; }
        public DateTime Date { get; set; }
        
        
        public string EventId { get; set; } = string.Empty;
    }
    
    
    public enum Category
    {
        Yoga,
        Pilates,
        Crossfit,
        Spinning
    }
