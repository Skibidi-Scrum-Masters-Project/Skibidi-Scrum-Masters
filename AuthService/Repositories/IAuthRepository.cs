using FitnessApp.Shared.Models;
using AuthService.Models;

public interface IAuthRepository
{
    Task<LoginResponse?> Login(LoginRequest request);
    string _GenerateJWT(User user);
}