using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FitnessApp.Shared.Models
{
    public class FitnessClass
    {
        [Required]
        public string ClassId { get; set; } = string.Empty;
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public string Description { get; set; } = string.Empty;
        [Required]
        public string InstructorId { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int MaxCapacity { get; set; }
        public List<string> EnrolledUserIds { get; set; } = new List<string>();
        public List<string> WaitlistUserIds { get; set; } = new List<string>();
        public bool IsActive { get; set; }
    }
}