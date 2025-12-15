namespace FitLifeFitness.Models;

public sealed class Post
{
    public string Id { get; set; } = "";
    public string UserId { get; set; } = "";
    public string UserName { get; set; } = "";
    public string? FitnessClassId { get; set; }
    public string? WorkoutId { get; set; }

    public DateTime PostDate { get; set; }

    public string PostTitle { get; set; } = "";
    public string PostContent { get; set; } = "";

    public PostType Type { get; set; } = PostType.Generic;

    public WorkoutStatsSnapshot? WorkoutStats { get; set; }

    public List<Comment> Comments { get; set; } = new();

    public bool IsDraft { get; set; }
    public string? SourceEventId { get; set; }
}

public enum PostType
{
    Generic = 0,
    Workout = 1
}

public sealed class WorkoutStatsSnapshot
{
    public int? DurationSeconds { get; set; }
    public int? Calories { get; set; }
    public int? ExerciseCount { get; set; } 

}

public sealed class Comment
{
    public string? Id { get; set; }
    public string AuthorId { get; set; } = "";
    public string AuthorName { get; set; } = "";
    public string CommentText { get; set; } = "";
    public DateTime CommentDate { get; set; }
}