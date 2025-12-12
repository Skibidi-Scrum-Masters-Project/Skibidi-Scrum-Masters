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