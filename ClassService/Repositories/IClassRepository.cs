using ClassService.Model;

public interface IClassRepository
{
   public Task<FitnessClass> CreateClassAsync(FitnessClass fitnessClass);
}