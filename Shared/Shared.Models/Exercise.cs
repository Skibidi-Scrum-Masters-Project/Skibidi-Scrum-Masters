using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FitnessApp.Shared.Models
{
    public class Exercise
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public double Volume { get; set; } 
        public List<Set> Sets { get; set; } = new List<Set>();
    }
}