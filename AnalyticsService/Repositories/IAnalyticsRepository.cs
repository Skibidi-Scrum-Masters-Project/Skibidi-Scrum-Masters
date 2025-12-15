using AnalyticsService.Models;
using TrainingType = FitnessApp.Shared.Models.TrainingType;

public interface IAnalyticsRepository
{

   public Task<int> GetCrowdCount();
   public Task<ClassResultDTO> PostClassesAnalytics(string classId, string userId, double totalCaloriesBurned, string category, int durationMin, DateTime date);
   public Task<string> PostEnteredUser(string userId, DateTime entryTime, DateTime exitTime);

   public Task<string> UpdateUserExitTime(string userId, DateTime exitTime);

   public Task<string> PostSoloTrainingResult(string userId, DateTime date, List<Exercise> exercises,
       TrainingTypes trainingType, double durationMinutes);

   public Task<List<SoloTrainingResultsDTO>> GetSoloTrainingResult(string userId);

   public Task<List<ClassResultDTO>> GetClassResult(string userId);

}; 