namespace FitLifeFitness.Models;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public UserDto User { get; set; } = new();
    public DateTime ExpiresAt { get; set; }
}

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public int Role { get; set; }
    public string Salt { get; set; } = string.Empty;
    public string HashedPassword { get; set; } = string.Empty;
}
