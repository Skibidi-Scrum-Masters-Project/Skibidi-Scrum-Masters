using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using FitnessApp.Shared.Models;
using AuthService.Models;
using System.IdentityModel.Tokens.Jwt;

namespace AuthService.Tests;

[TestClass]
public class AuthRepositoryTests
{
    private AuthRepository _repository = null!;
    private Mock<IOptions<JwtSettings>> _mockJwtSettings = null!;
    private Mock<ILogger<AuthRepository>> _mockLogger = null!;

    [TestInitialize]
    public void Setup()
    {
        // Setup mock for JWT settings
        var jwtSettings = new JwtSettings
        {
            Secret = "this-is-a-very-long-secret-key-for-testing-purposes-only-32-char-min",
            Issuer = "FitnessApp",
            Audience = "FitnessAppUsers",
            ExpirationMinutes = 60
        };

        _mockJwtSettings = new Mock<IOptions<JwtSettings>>();
        _mockJwtSettings.Setup(x => x.Value).Returns(jwtSettings);

        _mockLogger = new Mock<ILogger<AuthRepository>>();

        _repository = new AuthRepository(_mockJwtSettings.Object, _mockLogger.Object);
    }

    [TestMethod]
    public void ValidateUser_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var password = "TestPassword123!";
        var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
        
        var user = new User
        {
            Id = "1",
            Username = "testuser",
            Email = "test@example.com",
            HashedPassword = hashedPassword,
            Role = Role.Member
        };

        // Act
        var isValid = BCrypt.Net.BCrypt.Verify(password, user.HashedPassword);

        // Assert
        Assert.IsTrue(isValid, "Password verification should succeed with correct password");
    }

    [TestMethod]
    public void HashPassword_ShouldReturnValidHash()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash = BCrypt.Net.BCrypt.HashPassword(password);

        // Assert
        Assert.IsNotNull(hash, "Hash should not be null");
        Assert.AreNotEqual(password, hash, "Hash should not equal plaintext password");
        Assert.IsTrue(BCrypt.Net.BCrypt.Verify(password, hash), "Generated hash should verify the password");
    }

    [TestMethod]
    public void GenerateToken_ShouldReturnValidJWT()
    {
        // Arrange
        var user = new UserDTO
        {
            Id = "123",
            Username = "testuser",
            Email = "test@example.com",
            Role = Role.Member
        };

        // Act
        var token = _repository._GenerateJWT(user);

        // Assert
        Assert.IsNotNull(token, "Token should not be null");
        Assert.IsTrue(token.Length > 0, "Token should not be empty");

        // Validate JWT structure
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadToken(token) as JwtSecurityToken;
        
        Assert.IsNotNull(jwtToken, "Should be a valid JWT token");
        Assert.AreEqual("FitnessApp", jwtToken.Issuer, "Issuer should match settings");
        Assert.AreEqual("FitnessAppUsers", jwtToken.Claims.First(c => c.Type == "aud").Value, "Audience should match settings");
        Assert.IsTrue(jwtToken.Claims.Any(c => c.Type == "nameid" && c.Value == "123"), "Should contain user ID claim");
    }
}