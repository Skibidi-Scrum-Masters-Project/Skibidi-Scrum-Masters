using FitnessApp.Shared.Models;
using Mongo2Go;
using MongoDB.Driver;
using System.Net;

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

        var httpClient = new HttpClient(new FakeHttpMessageHandler())
        {
            BaseAddress = new Uri("http://localhost")
        };

        _repository = new SoloTrainingRepository(_database, httpClient);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _runner.Dispose();
    }

    [TestMethod]
    public async Task CreateSoloTraining_ShouldAddSessionToDatabase_AndSetUserId()
    {
        // Arrange
        var userId = "user123";
        var session = new SoloTrainingSession
        {
            Date = DateTime.UtcNow,

            DurationMinutes = 45,
            Exercises = new List<Exercise>()
        };

        // Act
        var result = await _repository.CreateSoloTraining(userId, session);

        // Assert (return value)
        Assert.AreEqual(userId, result.UserId);

        // Assert (db)
        var collection = _database.GetCollection<SoloTrainingSession>("SoloTrainingSessions");
        var found = await collection.Find(s => s.UserId == userId).FirstOrDefaultAsync();

        Assert.IsNotNull(found);
        Assert.AreEqual(userId, found!.UserId);
    }

    [TestMethod]
    public async Task GetAllSoloTrainingsForUser_ReturnsOnlyUsersSessions()
    {
        // Arrange
        var userId = "user123";
        var collection = _database.GetCollection<SoloTrainingSession>("SoloTrainingSessions");

        await collection.InsertOneAsync(new SoloTrainingSession { UserId = userId, Date = DateTime.UtcNow });
        await collection.InsertOneAsync(new SoloTrainingSession { UserId = userId, Date = DateTime.UtcNow.AddDays(-1) });
        await collection.InsertOneAsync(new SoloTrainingSession { UserId = "otherUser", Date = DateTime.UtcNow });

        // Act
        var result = await _repository.GetAllSoloTrainingsForUser(userId);

        // Assert
        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.All(s => s.UserId == userId));
    }

    [TestMethod]
    public async Task GetMostRecentSoloTrainingForUser_ReturnsLatestSession()
    {
        // Arrange
        var userId = "user123";
        var collection = _database.GetCollection<SoloTrainingSession>("SoloTrainingSessions");

        await collection.InsertOneAsync(new SoloTrainingSession { UserId = userId, Date = DateTime.UtcNow.AddDays(-2) });
        await collection.InsertOneAsync(new SoloTrainingSession { UserId = userId, Date = DateTime.UtcNow });

        // Act
        var result = await _repository.GetMostRecentSoloTrainingForUser(userId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(userId, result!.UserId);
    }

    [TestMethod]
    public async Task DeleteSoloTraining_WhenSessionDoesNotExist_ThrowsException()
    {
        // Arrange
        string nonExistentId = "607f1f77bcf86cd799439011";

        // Act + Assert
        await Assert.ThrowsExceptionAsync<Exception>(async () =>
            await _repository.DeleteSoloTraining(nonExistentId)
        );
    }

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
