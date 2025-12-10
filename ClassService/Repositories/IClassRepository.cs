using ClassService.Model;
using Microsoft.AspNetCore.Mvc;

public interface IClassRepository
{
   public Task<FitnessClass> CreateClassAsync(FitnessClass fitnessClass);
   public Task<IEnumerable<FitnessClass>> GetAllActiveClassesAsync();
   public Task<FitnessClass> BookClassForUserNoSeatAsync(string classId, string userId);
   public Task<FitnessClass> BookClassForUserWithSeatAsync(string classId, string userId, int seatNumber);
   public Task<FitnessClass> AddUserToClassWaitlistAsync(string classId, string userId);
   public Task<FitnessClass> GetClassByIdAsync(string classId);
   public Task<FitnessClass> CancelClassBookingForUserAsync(string classId, string userId);
   public Task MoveWaitlistToBookingWithSeat(string classId, int seatNumber);
   public Task MoveWaitlistToBookingWithNoSeat(string classId);
   public Task DeleteClassAsync(string classId);
   public Task FinishClass(string classId);

}