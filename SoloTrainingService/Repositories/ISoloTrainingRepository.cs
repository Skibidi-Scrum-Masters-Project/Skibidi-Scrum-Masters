using FitnessApp.Shared.Models;

public interface ISoloTrainingRepository
{
    public SoloTrainingSession CreateSoloTraining(string userId, SoloTrainingSession soloTraining);
    public List<SoloTrainingSession> GetAllSoloTrainingsForUser(string userId);
    public SoloTrainingSession GetMostRecentSoloTrainingForUser(string userId);
    //TBA
}