using FitnessApp.Shared.Models;

public interface IUserRepository
{
    User CreateUser(User user);
    User HashPassword(User user);
    List<UserDTO> GetAllUsers();
    UserDTO? GetUserById(string id);
    UserDTO? GetUserByUsername(string username);
    bool DeleteUser(string id);
    UserDTO UpdateUser(User updatedUser);
    List<UserDTO> GetUsersByRole(Role role);
    User? GetUserByUsernameSecure(string username);
}