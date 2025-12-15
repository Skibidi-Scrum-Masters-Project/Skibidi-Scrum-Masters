using FitnessApp.Shared.Models;

namespace SoloTrainingService.Models;

public sealed class SoloTrainingCompletedEvent
{
    public string EventId { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = default!;
    public string SoloTrainingSessionId { get; set; } = default!;

    public DateTime Date { get; set; }
    public string TrainingType { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }

    public int ExerciseCount { get; set; }
}