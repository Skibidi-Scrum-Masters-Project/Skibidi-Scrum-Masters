using FitnessApp.Shared.Models;
using Microsoft.Extensions.Options;
using AuthService.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MongoDB.Driver;

public class AuthRepository : IAuthRepository
{
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthRepository> _logger;
    private readonly IMongoCollection<RefreshToken> _refresh;

    public AuthRepository(IOptions<JwtSettings> jwtSettings, ILogger<AuthRepository> logger, IMongoDatabase database)
    {
        _jwtSettings = jwtSettings.Value;
        _logger = logger;
        _refresh = database.GetCollection<RefreshToken>("RefreshTokens");
    }
    //TBA
    public async Task<LoginResponse?> Login(LoginRequest request)
    {
        try
        {
            using var httpClient = new HttpClient();
            // Get user by username from UserService
            var response = await httpClient.GetAsync($"http://userservice:8080/api/users/username/{request.Username}/secure");
            
            if (response.IsSuccessStatusCode)
            {
                var user = await response.Content.ReadFromJsonAsync<User>();
                // Verify password using BCrypt (salt is embedded in the hash)
                if (user != null && BCrypt.Net.BCrypt.Verify(request.Password, user.HashedPassword))
                {
                    UserDTO userDto = new UserDTO
                    {
                        Id = user.Id,
                        Username = user.Username,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        Role = user.Role
                    };
                    var token = _GenerateJWT(userDto);
                    var refreshToken = _GenerateRefreshToken(userDto);
                    return new LoginResponse
                    {
                        Token = token,
                        User = userDto,
                        ExpiresAt = DateTime.UtcNow.AddDays(365), //Dette er kun for PoC da refreshtoken ikke er implementeret fuldt
                                                                  // Til når det skal implementeres burde det være omkring 15 minutter
                        RefreshToken = refreshToken
                    };
                }
                else if (user != null)
                {
                    _logger.LogWarning("Failed login attempt for username: {Username} - Invalid password", request.Username);
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

    public string _GenerateJWT(UserDTO user)
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

    public string _GenerateRefreshToken(UserDTO user)
    {
        var RandomBytes = RandomNumberGenerator.GetBytes(64);
        var refreshToken = new RefreshToken();
        refreshToken.UserId = user.Id;
        refreshToken.Token = Convert.ToBase64String(RandomBytes);
        _refresh.InsertOne(refreshToken);
        return refreshToken.Token;
    }

    public string RefreshToken(string token, string userId, Role role)
    { 
        var userDto = new UserDTO();
      RefreshToken refreshToken = _refresh.Find(c => c.UserId == userId).FirstOrDefault();
      if (refreshToken == null) return null;
      if (refreshToken.Token != token) return null;
      userDto.Id = userId;
      userDto.Role = role;
      return _GenerateJWT(userDto);
    }

    public string Logout(string userId)
    {
       _refresh.DeleteMany(c => c.UserId == userId);
        return "User logged out successfully";
    }
}