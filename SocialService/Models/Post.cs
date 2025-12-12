using System.ComponentModel.DataAnnotations;

namespace SocialService.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Post
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    
    public string UserId { get; set; }
    public string? FitnessClassId { get; set; }
    public string? WorkoutId { get; set; }
    public DateTime PostDate { get; set; }
    
    [Required]
    public string PostTitle { get; set; }
    
    [Required]
    public string PostContent { get; set; }
    
    public PostType Type { get; set; } = PostType.Generic;

    public WorkoutStatsSnapshot? WorkoutStats { get; set; }
    
    public List<Comment> Comments { get; set; } = new();
}

public enum PostType
{
    Generic = 0,
    Workout = 1
}

public class WorkoutStatsSnapshot
{
    public double? DistanceKm { get; set; }
    public int? DurationSeconds { get; set; }
    public int? Calories { get; set; }
    public int? AverageHeartRate { get; set; }
    public int? MaxHeartRate { get; set; }
    public double? AvgPaceMinPerKm { get; set; }
}