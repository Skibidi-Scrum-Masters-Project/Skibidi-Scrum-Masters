using FitnessApp.Shared.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FitnessApp.Shared.Models
{
    public class SoloTrainingSession
    {
         [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("_id")]
        public string? Id { get; set; }
       
        public string? EventId { get; set; }
        
        [BsonElement("userId")]
        public string UserId { get; set; } = string.Empty;
        [Required]
        [BsonElement("date")]
        public DateTime Date { get; set; }
        [Required]
        [BsonElement("trainingType")]
        public TrainingType TrainingType { get; set; }
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