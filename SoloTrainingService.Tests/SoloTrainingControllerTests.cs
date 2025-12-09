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
    [TestMethod]
    public void CreateSoloTraining_ShouldCreateSoloTraining()
    {
        // TBA: Implement test for creating solo training
        Assert.Inconclusive("Test not implemented yet");
    }

    [TestMethod]
    public void CreateSoloTraining_ReturnsOkResult_WithCreatedSession()
    {
        // Arrange
        var userId = "user123";
        var session = new SoloTrainingSession { UserId = userId, Date = DateTime.UtcNow };
        _mockRepository.Setup(r => r.CreateSoloTraining(userId, session)).Returns(session);

        // Act
        var result = _controller.CreateSoloTraining(userId, session);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreEqual(session, okResult.Value);
    }

    [TestMethod]
    public void CreateSoloTraining_WhenRepositoryThrows_Returns500()
    {
        // Arrange
        var userId = "user123";
        var session = new SoloTrainingSession { UserId = userId, Date = DateTime.UtcNow };
        _mockRepository.Setup(r => r.CreateSoloTraining(userId, session)).Throws(new Exception("DB error"));

        // Act
        var result = _controller.CreateSoloTraining(userId, session);

        // Assert
        var objectResult = result.Result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual(500, objectResult.StatusCode);
        Assert.IsTrue(objectResult.Value!.ToString()!.Contains("DB error"));
    }

    [TestMethod]
    public void CreateSoloTraining_WhenUserIdIsMissing_ReturnsBadRequest()
    {
        // Arrange
        string? userId = null;
        var session = new SoloTrainingSession { UserId = "", Date = DateTime.UtcNow };

        // Act
        var result = _controller.CreateSoloTraining(userId!, session);

        // Assert
        var objectResult = result.Result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual(400, objectResult.StatusCode);
    }

    [TestMethod]
    public void CreateSoloTraining_WhenSessionIsMissing_ReturnsBadRequest()
    {
        // Arrange
        var userId = "user123";
        SoloTrainingSession? session = null;

        // Act
        var result = _controller.CreateSoloTraining(userId, session!);

        // Assert
        var objectResult = result.Result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual(400, objectResult.StatusCode);
    }
}