public sealed class SoloTrainingCompletedEventDto
{
    public string? EventId { get; set; }
    public string? UserId { get; set; }
    public string? SoloTrainingSessionId { get; set; }

    public DateTime Date { get; set; }
    public string? WorkoutProgramName { get; set; }
    public int DurationMinutes { get; set; }
    public int ExerciseCount { get; set; }
}