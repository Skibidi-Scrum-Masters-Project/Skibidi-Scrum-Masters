namespace FitLifeFitness.Models;

public class SoloTrainingResultsDTO
{
    
    public string? Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public List<Exercise> Exercises { get; set; }
    public TrainingTypes TrainingType { get; set; } 
    public double DurationMinutes { get; set; }
}

public class Exercise
{
    public ExerciseType ExerciseType { get; set; }
    public double Volume { get; set; } 
    public List<Set> Sets { get; set; } = new List<Set>();
}




public enum TrainingTypes
{
    Cardio,
    UpperBody,
    LowerBody,
    SixPack
}