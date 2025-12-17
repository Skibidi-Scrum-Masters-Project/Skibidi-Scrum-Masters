using System.ComponentModel.DataAnnotations;
using FitnessApp.Shared.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace SoloTrainingService.Models
{
    public class SoloTrainingSession
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("_id")]
        public string? Id { get; set; }
        [Required]
        [BsonElement("workoutProgramId")]
        public string? WorkoutProgramId { get; set; }
        [Required]
        [BsonElement("workoutProgramName")]
        public string? WorkoutProgramName { get; set; }

        [BsonElement("userId")]
        public string UserId { get; set; } = string.Empty;
        [Required]
        [BsonElement("date")]
        public DateTime Date { get; set; }
        [BsonElement("durationMinutes")]
        public int DurationMinutes { get; set; }
        [Required]
        [BsonElement("exercises")]
        public List<Exercise> Exercises { get; set; } = new List<Exercise>();
    }
    public enum TrainingType
    {
        Cardio,
        UpperBody,
        LowerBody,
        SixPack
    }
}