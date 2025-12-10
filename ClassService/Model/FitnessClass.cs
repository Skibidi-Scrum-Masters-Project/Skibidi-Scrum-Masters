using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ClassService.Model
{
    public class FitnessClass
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("_id")]
        public string? Id { get; set; }

        [Required]
        public string InstructorId { get; set; } = string.Empty;
        [Required]
        public string CenterId { get; set; } = string.Empty;
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public Category Category { get; set; }
        [Required]
        public Intensity Intensity { get; set; }
        [Required]
        public string Description { get; set; } = string.Empty;
        [Required]
        public DateTime StartTime { get; set; }
        [Required]
        public int Duration { get; set; } // Duration in minutes
        [Required]
        public int MaxCapacity { get; set; }
        public List<Booking> BookingList { get; set; } = new List<Booking>();
        public List<string> WaitlistUserIds { get; set; } = new List<string>();
        public bool IsActive { get; set; }
        public bool SeatBookingEnabled { get; set; } = false;
        public int[]? SeatMap { get; set; }
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