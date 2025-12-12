using System.Collections.Generic;
using System.Threading.Tasks;
using FitnessApp.Shared.Models;

public interface ISoloTrainingRepository
{
    Task<SoloTrainingSession> CreateSoloTraining(string userId, SoloTrainingSession soloTraining);
    Task DeleteSoloTraining(string trainingId);
    Task<List<SoloTrainingSession>> GetAllSoloTrainingsForUser(string userId);
    Task<SoloTrainingSession> GetMostRecentSoloTrainingForUser(string userId);
}