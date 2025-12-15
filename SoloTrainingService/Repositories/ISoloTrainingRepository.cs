using FitnessApp.Shared.Models;

public interface ISoloTrainingRepository
{
    Task<SoloTrainingSession> CreateSoloTraining(string userId, SoloTrainingSession soloTraining);

    Task<List<SoloTrainingSession>> GetAllSoloTrainingsForUser(string userId);

    Task<SoloTrainingSession?> GetMostRecentSoloTrainingForUser(string userId);

    Task DeleteSoloTraining(string trainingId);
    // TBA
}