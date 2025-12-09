using Microsoft.AspNetCore.Mvc;
using FitnessApp.Shared.Models;
using SoloTrainingService.Controllers;
using Moq;

namespace SoloTrainingService.Tests;

[TestClass]
public class SoloTrainingControllerTests
{
    private SoloTrainingController _controller = null!;
    private Mock<ISoloTrainingRepository> _mockRepository = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new Mock<ISoloTrainingRepository>();
        _controller = new SoloTrainingController(_mockRepository.Object);
    }

    [TestMethod]
    public void GetSoloTrainings_ShouldReturnAllSoloTrainings()
    {
        // TBA: Implement test for getting all solo trainings
        Assert.Inconclusive("Test not implemented yet");
    }

    [TestMethod]
    public void GetSoloTrainings_WhenNoSoloTrainings_ShouldReturnEmptyList()
    {
        // TBA: Implement test for empty solo training list
        Assert.Inconclusive("Test not implemented yet");
    }

    [TestMethod]
    public void GetSoloTrainings_ShouldReturnOkResult()
    {
        // TBA: Implement test for OK status code
        Assert.Inconclusive("Test not implemented yet");
    }
}