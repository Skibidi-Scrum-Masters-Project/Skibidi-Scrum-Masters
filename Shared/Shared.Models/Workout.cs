using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FitnessApp.Shared.Models
{
    public class Workout
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Description { get; set; } = string.Empty;
        public List<WorkoutSession> Sessions { get; set; } = new List<WorkoutSession>();
    }
}