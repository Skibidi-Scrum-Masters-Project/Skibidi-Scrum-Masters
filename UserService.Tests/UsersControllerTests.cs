using Microsoft.AspNetCore.Mvc;
using FitnessApp.Shared.Models;
using UserService.Controllers;
using Moq;

namespace UserService.Tests;

[TestClass]
public class UsersControllerTests
{
    private UsersController _controller = null!;
    private Mock<IUserRepository> _mockRepository = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new Mock<IUserRepository>();
        _controller = new UsersController(_mockRepository.Object);
    }

    [TestMethod]
    public void GetUsers_ShouldReturnAllUsers()
    {
        // TBA: Implement test for getting all users
        Assert.Inconclusive("Test not implemented yet");
    }

    [TestMethod]
    public void GetUsers_WhenNoUsers_ShouldReturnEmptyList()
    {
        // TBA: Implement test for empty user list
        Assert.Inconclusive("Test not implemented yet");
    }

    [TestMethod]
    public void GetUsers_ShouldReturnOkResult()
    {
        // TBA: Implement test for OK status code
        Assert.Inconclusive("Test not implemented yet");
    }
}