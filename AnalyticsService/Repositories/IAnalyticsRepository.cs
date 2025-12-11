using AnalyticsService.Models;

public interface IAnalyticsRepository
{

   public Task<int> GetCrowdCount();
   public Task<ClassResultDTO> GetClassesAnalytics(string classId, string userId, double totalcaloriesBurned, string category, int durationMin, DateTime date);
   public Task<string> PostEnteredUser(string userId, DateTime entryTime, DateTime exitTime);

   public Task<string> UpdateUserExitTime(string userId, DateTime exitTime);

}; 