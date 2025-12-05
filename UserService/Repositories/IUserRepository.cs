using FitnessApp.Shared.Models;

public interface IUserRepository
{
    User CreateUser(User user);
    User HashPassword(User user);
    List<User> GetAllUsers();
    User? GetUserById(string id);
    User? GetUserByUsername(string username);
}