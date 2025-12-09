using FitnessApp.Shared.Models;

public interface ISoloTrainingRepository
{
    public SoloTrainingSession CreateSoloTraining(string userId, SoloTrainingSession soloTraining);
    //TBA
}