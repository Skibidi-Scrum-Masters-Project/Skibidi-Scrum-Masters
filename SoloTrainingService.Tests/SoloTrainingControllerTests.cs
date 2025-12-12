using Microsoft.AspNetCore.Mvc;
using FitnessApp.Shared.Models;
using SoloTrainingService.Controllers;
using Moq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
    public async Task GetSoloTrainings_ShouldReturnAllSoloTrainings()
    {
        // Implemented as a simple sanity check using existing tests below
        var userId = "user123";
        var sessions = new List<SoloTrainingSession>
        {
            new SoloTrainingSession { UserId = userId, Date = DateTime.UtcNow },
            new SoloTrainingSession { UserId = userId, Date = DateTime.UtcNow.AddDays(-1) }
        };
        _mockRepository.Setup(r => r.GetAllSoloTrainingsForUser(userId)).ReturnsAsync(sessions);

        var result = await _controller.GetAllSoloTrainingsForUser(userId);

        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        var returnedSessions = okResult.Value as List<SoloTrainingSession>;
        Assert.IsNotNull(returnedSessions);
        Assert.AreEqual(2, returnedSessions.Count);
    }

    [TestMethod]
    public async Task GetSoloTrainings_WhenNoSoloTrainings_ShouldReturnEmptyList()
    {
        var userId = "user123";
        _mockRepository.Setup(r => r.GetAllSoloTrainingsForUser(userId)).ReturnsAsync(new List<SoloTrainingSession>());

        var result = await _controller.GetAllSoloTrainingsForUser(userId);

        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        var returnedSessions = okResult.Value as List<SoloTrainingSession>;
        Assert.IsNotNull(returnedSessions);
        Assert.AreEqual(0, returnedSessions.Count);
    }

    [TestMethod]
    public async Task GetSoloTrainings_ShouldReturnOkResult()
    {
        var userId = "user123";
        var sessions = new List<SoloTrainingSession>
        {
            new SoloTrainingSession { UserId = userId, Date = DateTime.UtcNow }
        };
        _mockRepository.Setup(r => r.GetAllSoloTrainingsForUser(userId)).ReturnsAsync(sessions);

        var result = await _controller.GetAllSoloTrainingsForUser(userId);

        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
    }

    [TestMethod]
    public async Task CreateSoloTraining_ShouldCreateSoloTraining()
    {
        var userId = "user123";
        var session = new SoloTrainingSession { UserId = userId, Date = DateTime.UtcNow };
        _mockRepository.Setup(r => r.CreateSoloTraining(userId, session)).ReturnsAsync(session);

        var result = await _controller.CreateSoloTraining(userId, session);

        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreEqual(session, okResult.Value);
    }

    [TestMethod]
    public async Task CreateSoloTraining_ReturnsOkResult_WithCreatedSession()
    {
        // Duplicate of above but kept to reflect your original tests
        var userId = "user123";
        var session = new SoloTrainingSession { UserId = userId, Date = DateTime.UtcNow };
        _mockRepository.Setup(r => r.CreateSoloTraining(userId, session)).ReturnsAsync(session);

        var result = await _controller.CreateSoloTraining(userId, session);

        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreEqual(session, okResult.Value);
    }

    [TestMethod]
    public async Task CreateSoloTraining_WhenRepositoryThrows_Returns500()
    {
        var userId = "user123";
        var session = new SoloTrainingSession { UserId = userId, Date = DateTime.UtcNow };
        _mockRepository.Setup(r => r.CreateSoloTraining(userId, session)).ThrowsAsync(new Exception("DB error"));

        var result = await _controller.CreateSoloTraining(userId, session);

        var objectResult = result.Result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual(500, objectResult.StatusCode);
        // objectResult.Value is an anonymous object; check that the message contains the exception text
        StringAssert.Contains(objectResult.Value!.ToString()!, "DB error");
    }

    [TestMethod]
    public async Task CreateSoloTraining_WhenUserIdIsMissing_ReturnsBadRequest()
    {
        string? userId = null;
        var session = new SoloTrainingSession { UserId = "", Date = DateTime.UtcNow };

        var result = await _controller.CreateSoloTraining(userId!, session);

        var badRequest = result.Result as ObjectResult;
        Assert.IsNotNull(badRequest);
        Assert.AreEqual(400, badRequest.StatusCode);
    }

    [TestMethod]
    public async Task CreateSoloTraining_WhenSessionIsMissing_ReturnsBadRequest()
    {
        var userId = "user123";
        SoloTrainingSession? session = null;

        var result = await _controller.CreateSoloTraining(userId, session!);

        var badRequest = result.Result as ObjectResult;
        Assert.IsNotNull(badRequest);
        Assert.AreEqual(400, badRequest.StatusCode);
    }

    [TestMethod]
    public async Task GetAllSoloTrainingsForUser_ReturnsOkResult_WithSessions()
    {
        var userId = "user123";
        var sessions = new List<SoloTrainingSession>
        {
            new SoloTrainingSession { UserId = userId, Date = DateTime.UtcNow },
            new SoloTrainingSession { UserId = userId, Date = DateTime.UtcNow.AddDays(-1) }
        };
        _mockRepository.Setup(r => r.GetAllSoloTrainingsForUser(userId)).ReturnsAsync(sessions);

        var result = await _controller.GetAllSoloTrainingsForUser(userId);

        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        var returnedSessions = okResult.Value as List<SoloTrainingSession>;
        Assert.IsNotNull(returnedSessions);
        Assert.AreEqual(2, returnedSessions.Count);
    }

    [TestMethod]
    public async Task GetAllSoloTrainingsForUser_WhenUserIdIsMissing_ReturnsBadRequest()
    {
        string? userId = null;

        var result = await _controller.GetAllSoloTrainingsForUser(userId!);

        var badRequest = result.Result as BadRequestObjectResult;
        Assert.IsNotNull(badRequest);
        Assert.AreEqual(400, badRequest.StatusCode);
    }

    [TestMethod]
    public async Task GetAllSoloTrainingsForUser_WhenNoSessions_ReturnsEmptyList()
    {
        var userId = "user123";
        _mockRepository.Setup(r => r.GetAllSoloTrainingsForUser(userId)).ReturnsAsync(new List<SoloTrainingSession>());

        var result = await _controller.GetAllSoloTrainingsForUser(userId);

        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        var returnedSessions = okResult.Value as List<SoloTrainingSession>;
        Assert.IsNotNull(returnedSessions);
        Assert.AreEqual(0, returnedSessions.Count);
    }
}
