using FitnessApp.Shared.Models;
using Mongo2Go;
using MongoDB.Driver;


namespace SoloTrainingService.Tests;

[TestClass]
public class SoloTrainingRepositoryTests
{
    private MongoDbRunner _runner = null!;
    private IMongoDatabase _database = null!;
    private SoloTrainingRepository _repository = null!;

    [TestInitialize]
    public void Setup()
    {
        _runner = MongoDbRunner.Start();
        var client = new MongoClient(_runner.ConnectionString);
        _database = client.GetDatabase("TestSoloTrainingDb");
        _repository = new SoloTrainingRepository(_database);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _runner.Dispose();
    }

    [TestMethod]
    public void CreateSoloTraining_ShouldAddSessionToDatabase()
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
        var result = _repository.CreateSoloTraining(userId, session);

        // Assert
        var collection = _database.GetCollection<SoloTrainingSession>("SoloTrainingSessions");
        var found = collection.Find(s => s.UserId == userId).FirstOrDefault();
        Assert.IsNotNull(found);
        Assert.AreEqual(userId, found.UserId);
        Assert.IsTrue(Math.Abs((session.Date - found.Date).TotalSeconds) < 1);
    }

    [TestMethod]
    public void CreateSoloTraining_ShouldSetUserId()
    {
        // Arrange
        var userId = "user456";
        var session = new SoloTrainingSession { Date = DateTime.UtcNow };

        // Act
        var result = _repository.CreateSoloTraining(userId, session);

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
    public void GetAllSoloTrainingsForUser_ReturnsSessionsForUser()
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
        var result = _repository.GetAllSoloTrainingsForUser(userId);

        // Assert
        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.All(s => s.UserId == userId));
    }

    [TestMethod]
    public void GetAllSoloTrainingsForUser_WhenNoSessions_ReturnsEmptyList()
    {
        // Arrange
        var userId = "user999";
        // No sessions inserted for this user

        // Act
        var result = _repository.GetAllSoloTrainingsForUser(userId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }
}