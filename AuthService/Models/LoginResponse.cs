using FitnessApp.Shared.Models;

namespace AuthService.Models
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public UserDTO User { get; set; } = null!;
        public DateTime ExpiresAt { get; set; }
    }
}