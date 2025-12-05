using FitnessApp.Shared.Models;
using Microsoft.Extensions.Options;
using AuthService.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;

public class AuthRepository : IAuthRepository
{
    private readonly JwtSettings _jwtSettings;

    public AuthRepository(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }
    //TBA
    public async Task<LoginResponse?> Login(LoginRequest request)
    {
        try
        {
            using var httpClient = new HttpClient();
            // Get user by username from UserService
            var response = await httpClient.GetAsync($"http://userservice:8080/api/users/username/{request.Username}");
            
            if (response.IsSuccessStatusCode)
            {
                var user = await response.Content.ReadFromJsonAsync<User>();
                // Verify password using BCrypt (salt is embedded in the hash)
                if (user != null && BCrypt.Net.BCrypt.Verify(request.Password, user.HashedPassword))
                {
                    var token = _GenerateJWT(user);
                    return new LoginResponse
                    {
                        Token = token,
                        User = user,
                        ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes)
                    };
                }
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Login error: {ex.Message}");
            return null;
        }
    }

    public string _GenerateJWT(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.Secret);
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id ?? ""),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            Issuer = _jwtSettings.Issuer,
            Audience = _jwtSettings.Audience,
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}