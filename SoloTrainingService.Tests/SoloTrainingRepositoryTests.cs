using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FitnessApp.Shared.Models;
using Mongo2Go;
using MongoDB.Driver;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SoloTrainingService.Tests;

[TestClass]
public class SoloTrainingRepositoryTests
{
    private MongoDbRunner _runner = null!;
    private IMongoDatabase _database = null!;
    private SoloTrainingRepository _repository = null!;
    private HttpClient _httpClient = null!;

    [TestInitialize]
    public void Setup()
    {
        _runner = MongoDbRunner.Start();
        var client = new MongoClient(_runner.ConnectionString);
        _database = client.GetDatabase("TestSoloTrainingDb");

        // If your SoloTrainingRepository actually requires an HttpClient in the constructor,
        // replace the line below with: _repository = new SoloTrainingRepository(_database, new HttpClient());
        _repository = new SoloTrainingRepository(_database, _httpClient);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _runner.Dispose();
    }

    [TestMethod]
    public async Task CreateSoloTraining_ShouldAddSessionToDatabase()
    {
        // Arrange
        var userId = "user123";
        var session = new SoloTrainingSession
        {
            UserId = userId,
            Date = DateTime.UtcNow,
            TrainingType = TrainingType.UpperBody,
            DurationMinutes = 45,
            Exercises = new List<Exercise>()
        };

        // Act
        var result = await _repository.CreateSoloTraining(userId, session);

        // Assert
        var collection = _database.GetCollection<SoloTrainingSession>("SoloTrainingSessions");
        var found = collection.Find(s => s.UserId == userId).FirstOrDefault();
        Assert.IsNotNull(found);
        Assert.AreEqual(userId, found.UserId);
        Assert.IsTrue(Math.Abs((session.Date - found.Date).TotalSeconds) < 1);
    }

    [TestMethod]
    public async Task CreateSoloTraining_ShouldSetUserId()
    {
        // Arrange
        var userId = "user456";
        var session = new SoloTrainingSession { Date = DateTime.UtcNow };

        // Act
        var result = await _repository.CreateSoloTraining(userId, session);

        // Assert
        Assert.AreEqual(userId, result.UserId);
    }

    [TestMethod]
    public void AddExerciseToSoloTraining_ShouldUpdateSoloTraining()
    {
        // TBA: Implement exercise addition test
        Assert.Inconclusive("Test not implemented yet");
    }

    [TestMethod]
    public void CalculateSoloTrainingVolume_ShouldReturnCorrectTotal()
    {
        // TBA: Implement volume calculation test
        Assert.Inconclusive("Test not implemented yet");
    }

    [TestMethod]
    public async Task GetAllSoloTrainingsForUser_ReturnsSessionsForUser()
    {
        // Arrange
        var userId = "user123";
        var session1 = new SoloTrainingSession { UserId = userId, Date = DateTime.UtcNow };
        var session2 = new SoloTrainingSession { UserId = userId, Date = DateTime.UtcNow.AddDays(-1) };
        var otherSession = new SoloTrainingSession { UserId = "otherUser", Date = DateTime.UtcNow };
        var collection = _database.GetCollection<SoloTrainingSession>("SoloTrainingSessions");
        collection.InsertOne(session1);
        collection.InsertOne(session2);
        collection.InsertOne(otherSession);

        // Act
        var result = await _repository.GetAllSoloTrainingsForUser(userId);

        // Assert
        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.All(s => s.UserId == userId));
    }

    [TestMethod]
    public async Task GetAllSoloTrainingsForUser_WhenNoSessions_ReturnsEmptyList()
    {
        // Arrange
        var userId = "user999";
        // No sessions inserted for this user

        // Act
        var result = await _repository.GetAllSoloTrainingsForUser(userId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public async Task GetMostRecentSoloTrainingForUser_ReturnsLatestSession()
    {
        // Arrange
        var userId = "user123";
        var oldSession = new SoloTrainingSession { UserId = userId, Date = DateTime.UtcNow.AddDays(-2) };
        var recentSession = new SoloTrainingSession { UserId = userId, Date = DateTime.UtcNow };
        var midSession = new SoloTrainingSession { UserId = userId, Date = DateTime.UtcNow.AddDays(-1) };
        var otherUserSession = new SoloTrainingSession { UserId = "otherUser", Date = DateTime.UtcNow };
        var collection = _database.GetCollection<SoloTrainingSession>("SoloTrainingSessions");
        collection.InsertOne(oldSession);
        collection.InsertOne(recentSession);
        collection.InsertOne(midSession);
        collection.InsertOne(otherUserSession);

        // Act
        var result = await _repository.GetMostRecentSoloTrainingForUser(userId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(userId, result.UserId);
        Assert.IsTrue(Math.Abs((result.Date - recentSession.Date).TotalSeconds) < 2);
    }

    [TestMethod]
    public async Task GetMostRecentSoloTrainingForUser_WhenNoSessions_ReturnsNull()
    {
        // Arrange
        var userId = "user999";
        // No sessions inserted for this user

        // Act
        var result = await _repository.GetMostRecentSoloTrainingForUser(userId);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task DeleteSoloTraining_RemovesSession()
    {
        // Arrange
        var userId = "user123";
        var session = new SoloTrainingSession { UserId = userId, Date = DateTime.UtcNow };
        var collection = _database.GetCollection<SoloTrainingSession>("SoloTrainingSessions");
        collection.InsertOne(session);

        // Act
        await _repository.DeleteSoloTraining(session.Id!);

        // Assert
        var found = collection.Find(s => s.Id == session.Id).FirstOrDefault();
        Assert.IsNull(found);
    }

    [TestMethod]
    public async Task DeleteSoloTraining_WhenSessionDoesNotExist_ThrowsException()
    {
        // Arrange
        string nonExistentId = "607f1f77bcf86cd799439011";

        // Act & Assert
        await Assert.ThrowsExceptionAsync<Exception>(async () =>
        {
            await _repository.DeleteSoloTraining(nonExistentId);
        });
    }
}
