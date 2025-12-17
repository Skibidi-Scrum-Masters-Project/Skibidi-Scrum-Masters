using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FitnessApp.Shared.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SoloTrainingService.Models;

namespace FitnessApp.SoloTrainingService.Models
{
    public class WorkoutProgram
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("_id")]
        public string? Id { get; set; }
        [Required]
        [BsonElement("name")]
        public string Name { get; set; } = string.Empty;
        [Required]
        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;
        [Required]
        [BsonElement("exerciseTypes")]
        public List<ExerciseType> ExerciseTypes { get; set; } = new List<ExerciseType>();
    }
}