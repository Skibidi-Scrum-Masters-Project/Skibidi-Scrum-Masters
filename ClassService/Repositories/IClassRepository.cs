using ClassService.Model;
using Microsoft.AspNetCore.Mvc;

public interface IClassRepository
{
   public Task<FitnessClass> CreateClassAsync(FitnessClass fitnessClass);
   public Task<IEnumerable<FitnessClass>> GetAllActiveClassesAsync();
   public Task<FitnessClass> BookClassForUserNoSeatAsync(string classId, string userId);
   public Task<FitnessClass> AddUserToClassWaitlistAsync(string classId, string userId);
   public Task<FitnessClass> GetClassByIdAsync(string classId);
}