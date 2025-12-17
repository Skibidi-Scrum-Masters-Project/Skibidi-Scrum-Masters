using AnalyticsService.Models;

public interface IAnalyticsRepository
{

   public Task<int> GetCrowdCount();
   public Task<ClassResultDTO> PostClassesAnalytics(ClassResultDTO dto);
   public Task<string> PostEnteredUser(string userId, DateTime entryTime, DateTime exitTime);

   public Task<string> UpdateUserExitTime(string userId, DateTime exitTime);

   public Task<string> PostSoloTrainingResult(SoloTrainingResultsDTO dto);

   public Task<List<SoloTrainingResultsDTO>> GetSoloTrainingResult(string userId);

   public Task<List<ClassResultDTO>> GetClassResult(string userId);
   
   public Task<AnalyticsDashboardDTO> GetDashboardResult(string userId);
   
   Task<AnalyticsCompareDTO> GetCompareResultForCurrentMonth(string userId);
   
}; 