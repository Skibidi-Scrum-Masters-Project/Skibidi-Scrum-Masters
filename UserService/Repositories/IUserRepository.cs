using FitnessApp.Shared.Models;

public interface IUserRepository
{
    User CreateUser(User user);
    User HashPassword(User user);
    List<User> GetAllUsers();
    User? GetUserById(string id);
    User? GetUserByUsername(string username);
    bool DeleteUser(string id);
    User UpdateUser(User updatedUser);
    List<User> GetUsersByRole(Role role);
}