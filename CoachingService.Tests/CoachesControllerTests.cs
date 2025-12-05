using Microsoft.AspNetCore.Mvc;
using FitnessApp.Shared.Models;
using CoachingService.Controllers;
using Moq;

namespace CoachingService.Tests;

[TestClass]
public class CoachesControllerTests
{
    private CoachesController _controller = null!;
    private Mock<ICoachRepository> _mockRepository = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new Mock<ICoachRepository>();
        _controller = new CoachesController(_mockRepository.Object);
    }

    [TestMethod]
    public void GetCoaches_ShouldReturnAllCoaches()
    {
        // TBA: Implement test for getting all coaches
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