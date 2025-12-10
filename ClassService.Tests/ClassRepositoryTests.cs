using ClassService.Model;
using Mongo2Go;
using MongoDB.Driver;

namespace ClassService.Tests;

[TestClass]
public class ClassRepositoryTests
{
    private MongoDbRunner _runner = null!;
    private IMongoDatabase _database = null!;
    private ClassRepository _repository = null!;

    [TestInitialize]
    public void Setup()
    {
        _runner = MongoDbRunner.Start();
        var client = new MongoClient(_runner.ConnectionString);
        _database = client.GetDatabase("TestDatabase");
        _repository = new ClassRepository(_database);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _runner.Dispose();
    }

    [TestMethod]
    public async Task CreateClassAsync_AddsClassToDatabase()
    {
        // Arrange
        var fitnessClass = new FitnessClass
        {
            InstructorId = "instructor123",
            CenterId = "center456",
            Name = "Morning Yoga",
            Category = Category.Yoga,
            Intensity = Intensity.Easy,
            Description = "Gentle yoga for beginners.",
            StartTime = DateTime.UtcNow.AddDays(1),
            Duration = 60,
            MaxCapacity = 20
        };

        // Act
        var result = await _repository.CreateClassAsync(fitnessClass);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Id);
        Assert.AreEqual("instructor123", result.InstructorId);
        Assert.AreEqual("Morning Yoga", result.Name);
        Assert.IsTrue(result.IsActive);

        // Verify it's in the database
        var collection = _database.GetCollection<FitnessClass>("Classes");
        var foundClass = await collection.Find(c => c.Id == result.Id).FirstOrDefaultAsync();
        Assert.IsNotNull(foundClass);
        Assert.AreEqual("Morning Yoga", foundClass.Name);
    }

    [TestMethod]
    public async Task CreateClassAsync_SetsIsActiveToTrue()
    {
        // Arrange
        var fitnessClass = new FitnessClass
        {
            InstructorId = "instructor123",
            CenterId = "center456",
            Name = "Evening Pilates",
            Category = Category.Pilates,
            Intensity = Intensity.Medium,
            Description = "Core strength training.",
            StartTime = DateTime.UtcNow.AddDays(2),
            Duration = 45,
            MaxCapacity = 15,
            IsActive = false // Should be overridden to true
        };

        // Act
        var result = await _repository.CreateClassAsync(fitnessClass);

        // Assert
        Assert.IsTrue(result.IsActive);
    }
}