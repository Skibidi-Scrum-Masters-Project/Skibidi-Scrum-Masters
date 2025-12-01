using Microsoft.AspNetCore.Mvc;
using FitnessApp.Shared.Models;
using AuthService.Controllers;
using Moq;

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
    public void Login_WithValidCredentials_ShouldReturnSuccess()
    {
        // TBA: Implement test for successful login
        Assert.Inconclusive("Test not implemented yet");
    }

    [TestMethod]
    public void Login_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        // TBA: Implement test for failed login
        Assert.Inconclusive("Test not implemented yet");
    }

    [TestMethod]
    public void Login_WithEmptyRequest_ShouldReturnBadRequest()
    {
        // TBA: Implement test for invalid request
        Assert.Inconclusive("Test not implemented yet");
    }
}