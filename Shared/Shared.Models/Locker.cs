using System;
using System.ComponentModel.DataAnnotations;

namespace FitnessApp.Shared.Models
{
    public class Locker
    {
        [Required]
        public string LockerId { get; set; } = string.Empty;
        [Required]
        public int LockerNumber { get; set; }
        [Required]
        //måske skal det her ændres, snak med de andre
        public string UserId { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}