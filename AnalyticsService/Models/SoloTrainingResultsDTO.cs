using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace AnalyticsService.Models;

public class SoloTrainingResultsDTO
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [BsonElement("_id")]
    public string? Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public List<Exercise> Exercises { get; set; }
    public string TrainingType { get; set; } = string.Empty;
    public double DurationMinutes { get; set; }
}

public class Exercise
{
    public ExerciseType ExerciseType { get; set; }
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

public class Set
{
    public int Repetitions { get; set; }
    public double Weight { get; set; }
}

