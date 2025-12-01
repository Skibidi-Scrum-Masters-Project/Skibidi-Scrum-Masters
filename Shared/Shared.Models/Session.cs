using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FitnessApp.Shared.Models
{
    public class WorkoutSession
    {
        [Required]
        public DateTime Date { get; set; }
        public List<Exercise> Exercises { get; set; } = new List<Exercise>();
    }
}