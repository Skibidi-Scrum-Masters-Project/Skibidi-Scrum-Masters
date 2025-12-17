using FitnessApp.Shared.Models;
using AuthService.Models;

public interface IAuthRepository
{
    Task<LoginResponse?> Login(LoginRequest request);
    string _GenerateJWT(UserDTO user);

    string RefreshToken(string token, string userId, Role role);
    
    string Logout (string userId);
}