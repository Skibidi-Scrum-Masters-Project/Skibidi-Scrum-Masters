using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace FitLifeFitness.Models
{
    public class UserDto
    {

        public string? Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public UserRole Role { get; set; } = UserRole.Member;

        
    }
    public enum UserRole
        {
            Member,
            Coach,
            Admin
        }
}