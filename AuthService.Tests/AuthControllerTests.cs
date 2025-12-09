using Microsoft.AspNetCore.Mvc;
using FitnessApp.Shared.Models;
using AuthService.Models;
using AuthService.Controllers;
using Moq;
using System.Threading.Tasks;

namespace AuthService.Tests;

[TestClass]
public class AuthControllerTests
{
    private AuthController _controller = null!;
    private Mock<IAuthRepository> _mockRepository = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new Mock<IAuthRepository>();
        _controller = new AuthController(_mockRepository.Object);
    }

    [TestMethod]
    public async Task Login_WithValidCredentials_ShouldReturnSuccess()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Username = "testuser",
            Password = "testpassword"
        };

        var expectedResponse = new LoginResponse
        {
            Token = "mock-jwt-token",
            User = new User
            {
                Id = "1",
                Username = "testuser",
                Email = "test@example.com"
            },
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        // Setup the mock to return the expected response when Login is called
        _mockRepository
            .Setup(repo => repo.Login(It.IsAny<LoginRequest>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _controller.Login(loginRequest);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult, "Expected OkObjectResult");
        Assert.AreEqual(200, okResult.StatusCode);

        var response = okResult.Value as LoginResponse;
        Assert.IsNotNull(response, "Expected LoginResponse");
        Assert.AreEqual(expectedResponse.Token, response.Token);
        Assert.AreEqual(expectedResponse.User.Username, response.User.Username);

        // Verify that the repository's Login method was called exactly once
        _mockRepository.Verify(repo => repo.Login(It.IsAny<LoginRequest>()), Times.Once);
    }

    [TestMethod]
    public async Task Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        //Arrange
        var loginRequest = new LoginRequest
        {
            Username = "real username",
            Password = "wrong password"
        };

        //setup mock
        _mockRepository.Setup(repo => repo.Login(It.IsAny<LoginRequest>())).ReturnsAsync((LoginResponse?)null);

        //act
        var result = await _controller.Login(loginRequest);

        //Assert
        var unauthorizedResult = result as UnauthorizedObjectResult;
        Assert.IsNotNull(unauthorizedResult);
        Assert.AreEqual(401, unauthorizedResult.StatusCode);

        _mockRepository.Verify(repo => repo.Login(It.IsAny<LoginRequest>()), Times.Once);
    }

    [TestMethod]
    public void Login_WithEmptyRequest_ShouldReturnBadRequest()
    {
        //Arrange
        LoginRequest? loginRequest = null;
        //Act
        var result =  _controller.Login(loginRequest!);
        //Assert
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);

        //Verify
        _mockRepository.Verify(repo => repo.Login(It.IsAny<LoginRequest>()), Times.Never);


        //Setup Mock
        

    }
}