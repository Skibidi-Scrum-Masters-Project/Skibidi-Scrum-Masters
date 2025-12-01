using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FitnessApp.Shared.Models
{
    public class Friend
    {
        [Required]
        public string UserId { get; set; } = string.Empty;
        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        public string LastName { get; set; } = string.Empty;
        //Idk, NIce to have måske?
        // public DateTime FriendsSince { get; set; }
        public bool IsActive { get; set; }
    }
}