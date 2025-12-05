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
        public Role Role { get; set; } = Role.Member;
        public string? Salt { get; set; }
        public string? HashedPassword { get; set; }


    }
}