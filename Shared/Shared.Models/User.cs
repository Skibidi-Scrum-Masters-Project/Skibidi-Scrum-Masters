using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FitnessApp.Shared.Models
{
    public class User
    {
        [Required]
        public string UserId { get; set; } = string.Empty;
        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        public string LastName { get; set; } = string.Empty;
        [Required]
        public string Email { get; set; } = string.Empty;
        public string? LicensePlate { get; set; }
        public List<Workout> Workouts { get; set; } = new List<Workout>();
        public List<Role> Roles { get; set; } = new List<Role>();
        public string? Salt { get; set; }
        public string? HashedPassword { get; set; }
        public List<string> FriendIds { get; set; } = new List<string>();
        public List<string> Classes { get; set; } = new List<string>();
        public List<string> Waitlist { get; set; } = new List<string>();
        public string? LockerId { get; set; }
    }
}