using System.ComponentModel.DataAnnotations;

namespace SoloTrainingService.Models
{
    public class WorkoutSession
    {
        [Required]
        public DateTime Date { get; set; }
        public List<Exercise> Exercises { get; set; } = new List<Exercise>();
    }
}