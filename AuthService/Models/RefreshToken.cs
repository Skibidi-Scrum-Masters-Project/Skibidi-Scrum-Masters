namespace AuthService.Models;

public class RefreshToken
{
    public string Token { get; set; } = string.Empty;
    
    public string UserId { get; set; } = string.Empty;
}