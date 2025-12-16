using FitnessApp.Shared.Models;
using FitnessApp.SoloTrainingService.Models;
public interface ISoloTrainingRepository
{
    Task<WorkoutProgram> CreateWorkoutProgram(WorkoutProgram workoutProgram);
    Task<SoloTrainingSession> CreateSoloTraining(string userId, SoloTrainingSession soloTraining, string programId);

    Task<List<SoloTrainingSession>> GetAllSoloTrainingsForUser(string userId);

    Task<SoloTrainingSession?> GetMostRecentSoloTrainingForUser(string userId);
    Task<SoloTrainingSession?> GetMostRecentSoloTrainingForUserAndProgram(string userId, string programId);
    Task<List<WorkoutProgram>> GetAllWorkoutPrograms();
    Task<WorkoutProgram?> GetWorkoutProgramById(string programId);

    Task DeleteSoloTraining(string trainingId);
    // TBA
}