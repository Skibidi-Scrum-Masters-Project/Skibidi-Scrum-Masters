using FitnessApp.Shared.Models;

namespace CoachingService.Models
{
    // Inherits all properties from User
    public class Coach : User
    {
        // Extra attributes specific to coaches
        public string Specialty { get; set; } = string.Empty;
        public int YearsOfExperience { get; set; }
        public string Certification { get; set; } = string.Empty;
    }
}