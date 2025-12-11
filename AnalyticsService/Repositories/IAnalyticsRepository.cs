using FitnessApp.Shared.Models;

public interface IAnalyticsRepository
{
   public Task<int> GetCrowd();

}