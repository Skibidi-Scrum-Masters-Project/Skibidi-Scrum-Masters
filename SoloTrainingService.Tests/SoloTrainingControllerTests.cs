using Microsoft.AspNetCore.Mvc;
using FitnessApp.Shared.Models;
using SoloTrainingService.Controllers;
using Moq;
using SoloTrainingService.Models;

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
    public async Task CreateSoloTraining_ReturnsOkResult_WithCreatedSession()
    {
        // Arrange
        var userId = "user123";
        var session = new SoloTrainingSession { UserId = userId, Date = DateTime.UtcNow };

            _mockRepository
                .Setup(r => r.CreateSoloTraining(userId, It.IsAny<SoloTrainingSession>(), It.IsAny<string>()))
                .ReturnsAsync(session);

        // Act
            var result = await _controller.CreateSoloTraining(userId, "program1", session);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreEqual(session, okResult.Value);
    }

    [TestMethod]
    public async Task CreateSoloTraining_WhenRepositoryThrows_Returns500()
    {
        // Arrange
        var userId = "user123";
        var session = new SoloTrainingSession { UserId = userId, Date = DateTime.UtcNow };

            _mockRepository
                .Setup(r => r.CreateSoloTraining(userId, It.IsAny<SoloTrainingSession>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("DB error"));

        // Act
            var result = await _controller.CreateSoloTraining(userId, "program1", session);

        // Assert
        var objectResult = result.Result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual(500, objectResult.StatusCode);
        Assert.IsTrue(objectResult.Value!.ToString()!.Contains("DB error"));
    }

    [TestMethod]
    public async Task CreateSoloTraining_WhenUserIdIsMissing_ReturnsBadRequest()
    {
        // Arrange
        string? userId = null;
        var session = new SoloTrainingSession { UserId = "", Date = DateTime.UtcNow };

        // Act
        var result = await _controller.CreateSoloTraining(userId!, "program1", session);

        // Assert
        var badRequest = result.Result as BadRequestObjectResult;
        Assert.IsNotNull(badRequest);
        Assert.AreEqual(400, badRequest.StatusCode);
    }

    [TestMethod]
    public async Task CreateSoloTraining_WhenSessionIsMissing_ReturnsBadRequest()
    {
        // Arrange
        var userId = "user123";
        SoloTrainingSession? session = null;

        // Act
            var result = await _controller.CreateSoloTraining(userId, "program1", session!);

        // Assert
        var badRequest = result.Result as BadRequestObjectResult;
        Assert.IsNotNull(badRequest);
        Assert.AreEqual(400, badRequest.StatusCode);
    }

    [TestMethod]
    public async Task GetAllSoloTrainingsForUser_ReturnsOkResult_WithSessions()
    {
        // Arrange
        var userId = "user123";
        var sessions = new List<SoloTrainingSession>
        {
            new SoloTrainingSession { UserId = userId, Date = DateTime.UtcNow },
            new SoloTrainingSession { UserId = userId, Date = DateTime.UtcNow.AddDays(-1) }
        };

        _mockRepository
            .Setup(r => r.GetAllSoloTrainingsForUser(userId))
            .ReturnsAsync(sessions);

        // Act
        var result = await _controller.GetAllSoloTrainingsForUser(userId);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);

        var returnedSessions = okResult.Value as List<SoloTrainingSession>;
        Assert.IsNotNull(returnedSessions);
        Assert.AreEqual(2, returnedSessions.Count);
    }

    [TestMethod]
    public async Task GetAllSoloTrainingsForUser_WhenNoSessions_ReturnsEmptyList()
    {
        // Arrange
        var userId = "user123";

        _mockRepository
            .Setup(r => r.GetAllSoloTrainingsForUser(userId))
            .ReturnsAsync(new List<SoloTrainingSession>());

        // Act
        var result = await _controller.GetAllSoloTrainingsForUser(userId);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);

        var returnedSessions = okResult.Value as List<SoloTrainingSession>;
        Assert.IsNotNull(returnedSessions);
        Assert.AreEqual(0, returnedSessions.Count);
    }

    [TestMethod]
    public async Task GetAllSoloTrainingsForUser_WhenUserIdIsMissing_ReturnsBadRequest()
    {
        // Arrange
        string? userId = null;

        // Act
        var result = await _controller.GetAllSoloTrainingsForUser(userId!);

        // Assert
        var badRequest = result.Result as BadRequestObjectResult;
        Assert.IsNotNull(badRequest);
        Assert.AreEqual(400, badRequest.StatusCode);
    }

    [TestMethod]
    public async Task GetMostRecentSoloTrainingForUser_ReturnsOkWithSession()
    {
        // Arrange
        var userId = "user123";
        var session = new SoloTrainingSession { UserId = userId, Date = DateTime.UtcNow };

        _mockRepository
            .Setup(r => r.GetMostRecentSoloTrainingForUser(userId))
            .ReturnsAsync(session);

        // Act
        var result = await _controller.GetMostRecentSoloTrainingForUser(userId);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreEqual(session, okResult.Value);
    }

    [TestMethod]
    public async Task GetMostRecentSoloTrainingForUser_WhenNoSession_ReturnsNotFound()
    {
        // Arrange
        var userId = "user999";

        _mockRepository
            .Setup(r => r.GetMostRecentSoloTrainingForUser(userId))
            .ReturnsAsync((SoloTrainingSession?)null);

        // Act
        var result = await _controller.GetMostRecentSoloTrainingForUser(userId);

        // Assert
        var notFound = result.Result as NotFoundObjectResult;
        Assert.IsNotNull(notFound);
        Assert.AreEqual(404, notFound.StatusCode);
    }

    [TestMethod]
    public async Task DeleteSoloTraining_ReturnsNoContent_WhenDeleted()
    {
        // Arrange
        var sessionId = "session123";

        _mockRepository
            .Setup(r => r.DeleteSoloTraining(sessionId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteSoloTraining(sessionId);

        // Assert
        Assert.IsInstanceOfType(result, typeof(NoContentResult));
        var noContentResult = result as NoContentResult;
        Assert.IsNotNull(noContentResult);
        Assert.AreEqual(204, noContentResult.StatusCode);
    }

    [TestMethod]
    public async Task DeleteSoloTraining_ReturnsInternalServerError_WhenExceptionThrown()
    {
        // Arrange
        var sessionId = "session123";

        _mockRepository
            .Setup(r => r.DeleteSoloTraining(sessionId))
            .ThrowsAsync(new Exception("not found"));

        // Act
        var result = await _controller.DeleteSoloTraining(sessionId);

        // Assert
        var objectResult = result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual(500, objectResult.StatusCode);
    }
    
    

        [TestMethod]
        public async Task CreateSoloTraining_ReturnsOk_WithSession()
        {
            var userId = "user123";
            var session = new SoloTrainingSession
            {
                Date = DateTime.UtcNow,
                DurationMinutes = 30,
                Exercises = new()
            };

                _mockRepository.Setup(r => r.CreateSoloTraining(userId, It.IsAny<SoloTrainingSession>(), It.IsAny<string>()))
                    .ReturnsAsync(session);

                var result = await _controller.CreateSoloTraining(userId, "program1", session);

            var ok = result.Result as OkObjectResult;
            Assert.IsNotNull(ok);
            Assert.AreEqual(200, ok.StatusCode);
            Assert.AreEqual(session, ok.Value);
        }

        [TestMethod]
        public async Task GetMostRecentSoloTrainingForUser_WhenNone_ReturnsNotFound()
        {
            var userId = "user123";

            _mockRepository.Setup(r => r.GetMostRecentSoloTrainingForUser(userId))
                .ReturnsAsync((SoloTrainingSession?)null);

            var result = await _controller.GetMostRecentSoloTrainingForUser(userId);

            var notFound = result.Result as NotFoundObjectResult;
            Assert.IsNotNull(notFound);
            Assert.AreEqual(404, notFound.StatusCode);
        }

        [TestMethod]
        public async Task DeleteSoloTraining_ReturnsNoContent()
        {
            var trainingId = "abc123";

            _mockRepository.Setup(r => r.DeleteSoloTraining(trainingId))
                .Returns(Task.CompletedTask);

            var result = await _controller.DeleteSoloTraining(trainingId);

            Assert.IsInstanceOfType(result, typeof(NoContentResult));
        }
}
