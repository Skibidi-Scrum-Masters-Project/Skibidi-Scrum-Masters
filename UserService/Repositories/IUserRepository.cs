using FitnessApp.Shared.Models;

public interface IUserRepository
{
    User CreateUser(User user);
    User HashPassword(User user);
    List<User> GetAllUsers();
}