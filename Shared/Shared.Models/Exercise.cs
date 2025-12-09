using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FitnessApp.Shared.Models
{
    public class Exercise
    {
        [Required]
        [BsonElement("exerciseType")]
        public ExerciseType ExerciseType { get; set; }
        [Required]
        public double Volume { get; set; } 
        public List<Set> Sets { get; set; } = new List<Set>();
    }
    public enum ExerciseType
    {
        BenchPress,
        Squat,
        Deadlift,
        PullUp,
        PushUp
    }
}