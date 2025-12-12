using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;


namespace FitlifeFitness.Models
{
    public class FitnessClass
    {

        public string? Id { get; set; }


        public string InstructorId { get; set; } = string.Empty;

        public string CenterId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public Category Category { get; set; }
        public Intensity Intensity { get; set; }

        public string Description { get; set; } = string.Empty;

        public DateTime StartTime { get; set; }

        public int Duration { get; set; } // Duration in minutes
        public int MaxCapacity { get; set; }
        public List<Booking> BookingList { get; set; } = new List<Booking>();
        public List<string> WaitlistUserIds { get; set; } = new List<string>();
        public bool IsActive { get; set; }
        public bool SeatBookingEnabled { get; set; } = false;
        public bool[]? SeatMap { get; set; }

        // Convenience properties used by the UI
        public DateOnly Date => DateOnly.FromDateTime(StartTime);
        public string Time => StartTime.ToString("HH:mm");
        public string Location => CenterId;
        public string Status => IsActive ? "Active" : "Inactive";
    }
    public enum Category
    {
        Yoga,
        Pilates,
        Crossfit,
        Spinning
    }
    public enum Intensity
    {
        Easy,
        Medium,
        Hard
    }
}