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

    [TestMethod]
    public async Task GetAllActiveClassesAsync_ReturnsOnlyActiveClasses()
    {
        // Arrange
        var activeClass = new FitnessClass
        {
            InstructorId = "instructor123",
            CenterId = "center456",
            Name = "Active Yoga",
            Category = Category.Yoga,
            Intensity = Intensity.Easy,
            Description = "Active class.",
            StartTime = DateTime.UtcNow.AddDays(1),
            Duration = 60,
            MaxCapacity = 20,
            IsActive = true
        };
        var inactiveClass = new FitnessClass
        {
            InstructorId = "instructor456",
            CenterId = "center789",
            Name = "Inactive Pilates",
            Category = Category.Pilates,
            Intensity = Intensity.Medium,
            Description = "Inactive class.",
            StartTime = DateTime.UtcNow.AddDays(2),
            Duration = 45,
            MaxCapacity = 15,
            IsActive = false
        };
        var collection = _database.GetCollection<FitnessClass>("Classes");
        await collection.InsertOneAsync(activeClass);
        await collection.InsertOneAsync(inactiveClass);

        // Act
        var result = await _repository.GetAllActiveClassesAsync();

        // Assert
        var resultList = result.ToList();
        Assert.AreEqual(1, resultList.Count);
        Assert.IsTrue(resultList.All(c => c.IsActive));
        Assert.AreEqual("Active Yoga", resultList[0].Name);
    }

    [TestMethod]
    public async Task GetAllActiveClassesAsync_WhenNoActiveClasses_ReturnsEmptyList()
    {
        // Arrange
        var inactiveClass = new FitnessClass
        {
            InstructorId = "instructor123",
            CenterId = "center456",
            Name = "Inactive Class",
            Category = Category.Yoga,
            Intensity = Intensity.Easy,
            Description = "Inactive.",
            StartTime = DateTime.UtcNow.AddDays(1),
            Duration = 60,
            MaxCapacity = 20,
            IsActive = false
        };
        var collection = _database.GetCollection<FitnessClass>("Classes");
        await collection.InsertOneAsync(inactiveClass);

        // Act
        var result = await _repository.GetAllActiveClassesAsync();

        // Assert
        var resultList = result.ToList();
        Assert.AreEqual(0, resultList.Count);
    }

    [TestMethod]
    public async Task BookClassForUserNoSeatAsync_AddsBookingToClass()
    {
        // Arrange
        var fitnessClass = new FitnessClass
        {
            InstructorId = "instructor123",
            CenterId = "center456",
            Name = "Morning Yoga",
            Category = Category.Yoga,
            Intensity = Intensity.Easy,
            Description = "Yoga class.",
            StartTime = DateTime.UtcNow.AddDays(1),
            Duration = 60,
            MaxCapacity = 5,
            IsActive = true,
            BookingList = new List<Booking>(),
            WaitlistUserIds = new List<string>()
        };
        var collection = _database.GetCollection<FitnessClass>("Classes");
        await collection.InsertOneAsync(fitnessClass);

        // Act
        var result = await _repository.BookClassForUserNoSeatAsync(fitnessClass.Id!, "user123");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.BookingList.Count);
        Assert.AreEqual("user123", result.BookingList[0].UserId);
        Assert.AreEqual(0, result.BookingList[0].SeatNumber);
    }

    [TestMethod]
    public async Task BookClassForUserNoSeatAsync_WhenFull_AddsToWaitlist()
    {
        // Arrange
        var fitnessClass = new FitnessClass
        {
            InstructorId = "instructor123",
            CenterId = "center456",
            Name = "Full Yoga",
            Category = Category.Yoga,
            Intensity = Intensity.Easy,
            Description = "Full class.",
            StartTime = DateTime.UtcNow.AddDays(1),
            Duration = 60,
            MaxCapacity = 2,
            IsActive = true,
            BookingList = new List<Booking>
            {
                new Booking { UserId = "user1", SeatNumber = 0, CheckedInAt = DateTime.MinValue },
                new Booking { UserId = "user2", SeatNumber = 0, CheckedInAt = DateTime.MinValue }
            },
            WaitlistUserIds = new List<string>()
        };
        var collection = _database.GetCollection<FitnessClass>("Classes");
        await collection.InsertOneAsync(fitnessClass);

        // Act
        var result = await _repository.BookClassForUserNoSeatAsync(fitnessClass.Id!, "user3");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.BookingList.Count);
        Assert.AreEqual(1, result.WaitlistUserIds.Count);
        Assert.IsTrue(result.WaitlistUserIds.Contains("user3"));
    }

    [TestMethod]
    public async Task BookClassForUserWithSeatAsync_AddsBookingWithSeat()
    {
        // Arrange
        var fitnessClass = new FitnessClass
        {
            InstructorId = "instructor123",
            CenterId = "center456",
            Name = "Seated Yoga",
            Category = Category.Yoga,
            Intensity = Intensity.Easy,
            Description = "Yoga with seat selection.",
            StartTime = DateTime.UtcNow.AddDays(1),
            Duration = 60,
            MaxCapacity = 5,
            IsActive = true,
            SeatBookingEnabled = true,
            SeatMap = new bool[5],
            BookingList = new List<Booking>(),
            WaitlistUserIds = new List<string>()
        };
        var collection = _database.GetCollection<FitnessClass>("Classes");
        await collection.InsertOneAsync(fitnessClass);

        // Act
        var result = await _repository.BookClassForUserWithSeatAsync(fitnessClass.Id!, "user123", 2);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.BookingList.Count);
        Assert.AreEqual("user123", result.BookingList[0].UserId);
        Assert.AreEqual(2, result.BookingList[0].SeatNumber);
        Assert.IsTrue(result.SeatMap![2]);
    }

    [TestMethod]
    public async Task BookClassForUserWithSeatAsync_WhenSeatTaken_ThrowsException()
    {
        // Arrange
        var seatMap = new bool[5];
        seatMap[2] = true; // Seat 2 is already taken
        var fitnessClass = new FitnessClass
        {
            InstructorId = "instructor123",
            CenterId = "center456",
            Name = "Seated Yoga",
            Category = Category.Yoga,
            Intensity = Intensity.Easy,
            Description = "Yoga with seat selection.",
            StartTime = DateTime.UtcNow.AddDays(1),
            Duration = 60,
            MaxCapacity = 5,
            IsActive = true,
            SeatBookingEnabled = true,
            SeatMap = seatMap,
            BookingList = new List<Booking>
            {
                new Booking { UserId = "user1", SeatNumber = 2, CheckedInAt = DateTime.MinValue }
            },
            WaitlistUserIds = new List<string>()
        };
        var collection = _database.GetCollection<FitnessClass>("Classes");
        await collection.InsertOneAsync(fitnessClass);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<Exception>(
            async () => await _repository.BookClassForUserWithSeatAsync(fitnessClass.Id!, "user123", 2)
        );
    }

    [TestMethod]
    public async Task CancelClassBookingForUserAsync_RemovesBooking()
    {
        // Arrange
        var fitnessClass = new FitnessClass
        {
            InstructorId = "instructor123",
            CenterId = "center456",
            Name = "Morning Yoga",
            Category = Category.Yoga,
            Intensity = Intensity.Easy,
            Description = "Yoga class.",
            StartTime = DateTime.UtcNow.AddDays(1),
            Duration = 60,
            MaxCapacity = 5,
            IsActive = true,
            SeatBookingEnabled = false,
            BookingList = new List<Booking>
            {
                new Booking { UserId = "user123", SeatNumber = 0, CheckedInAt = DateTime.MinValue },
                new Booking { UserId = "user456", SeatNumber = 0, CheckedInAt = DateTime.MinValue }
            },
            WaitlistUserIds = new List<string>()
        };
        var collection = _database.GetCollection<FitnessClass>("Classes");
        await collection.InsertOneAsync(fitnessClass);

        // Act
        var result = await _repository.CancelClassBookingForUserAsync(fitnessClass.Id!, "user123");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.BookingList.Count);
        Assert.IsFalse(result.BookingList.Any(b => b.UserId == "user123"));
        Assert.IsTrue(result.BookingList.Any(b => b.UserId == "user456"));
    }

    [TestMethod]
    public async Task CancelClassBookingForUserAsync_WithSeat_FreesUpSeat()
    {
        // Arrange
        var seatMap = new bool[5];
        seatMap[2] = true;
        var fitnessClass = new FitnessClass
        {
            InstructorId = "instructor123",
            CenterId = "center456",
            Name = "Seated Yoga",
            Category = Category.Yoga,
            Intensity = Intensity.Easy,
            Description = "Yoga with seat selection.",
            StartTime = DateTime.UtcNow.AddDays(1),
            Duration = 60,
            MaxCapacity = 5,
            IsActive = true,
            SeatBookingEnabled = true,
            SeatMap = seatMap,
            BookingList = new List<Booking>
            {
                new Booking { UserId = "user123", SeatNumber = 2, CheckedInAt = DateTime.MinValue }
            },
            WaitlistUserIds = new List<string>()
        };
        var collection = _database.GetCollection<FitnessClass>("Classes");
        await collection.InsertOneAsync(fitnessClass);

        // Act
        var result = await _repository.CancelClassBookingForUserAsync(fitnessClass.Id!, "user123");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.BookingList.Count);
        Assert.IsFalse(result.SeatMap![2]);
    }

    [TestMethod]
    public async Task CancelClassBookingForUserAsync_MovesWaitlistUserToBooking_NoSeat()
    {
        // Arrange
        var fitnessClass = new FitnessClass
        {
            InstructorId = "instructor123",
            CenterId = "center456",
            Name = "Morning Yoga",
            Category = Category.Yoga,
            Intensity = Intensity.Easy,
            Description = "Yoga class.",
            StartTime = DateTime.UtcNow.AddDays(1),
            Duration = 60,
            MaxCapacity = 5,
            IsActive = true,
            SeatBookingEnabled = false,
            BookingList = new List<Booking>
            {
                new Booking { UserId = "user123", SeatNumber = 0, CheckedInAt = DateTime.MinValue }
            },
            WaitlistUserIds = new List<string> { "waitlistUser1", "waitlistUser2" }
        };
        var collection = _database.GetCollection<FitnessClass>("Classes");
        await collection.InsertOneAsync(fitnessClass);

        // Act
        var result = await _repository.CancelClassBookingForUserAsync(fitnessClass.Id!, "user123");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.BookingList.Count);
        Assert.AreEqual("waitlistUser1", result.BookingList[0].UserId);
        Assert.AreEqual(1, result.WaitlistUserIds.Count);
        Assert.IsFalse(result.WaitlistUserIds.Contains("waitlistUser1"));
        Assert.IsTrue(result.WaitlistUserIds.Contains("waitlistUser2"));
    }

    [TestMethod]
    public async Task CancelClassBookingForUserAsync_MovesWaitlistUserToBooking_WithSeat()
    {
        // Arrange
        var seatMap = new bool[5];
        seatMap[2] = true;
        var fitnessClass = new FitnessClass
        {
            InstructorId = "instructor123",
            CenterId = "center456",
            Name = "Seated Yoga",
            Category = Category.Yoga,
            Intensity = Intensity.Easy,
            Description = "Yoga with seat selection.",
            StartTime = DateTime.UtcNow.AddDays(1),
            Duration = 60,
            MaxCapacity = 5,
            IsActive = true,
            SeatBookingEnabled = true,
            SeatMap = seatMap,
            BookingList = new List<Booking>
            {
                new Booking { UserId = "user123", SeatNumber = 2, CheckedInAt = DateTime.MinValue }
            },
            WaitlistUserIds = new List<string> { "waitlistUser1" }
        };
        var collection = _database.GetCollection<FitnessClass>("Classes");
        await collection.InsertOneAsync(fitnessClass);

        // Act
        var result = await _repository.CancelClassBookingForUserAsync(fitnessClass.Id!, "user123");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.BookingList.Count);
        Assert.AreEqual("waitlistUser1", result.BookingList[0].UserId);
        Assert.AreEqual(2, result.BookingList[0].SeatNumber);
        Assert.AreEqual(0, result.WaitlistUserIds.Count);
        Assert.IsFalse(result.SeatMap![2]);
    }

    [TestMethod]
    public async Task CancelClassBookingForUserAsync_RemovesFromWaitlistOnly()
    {
        // Arrange
        var fitnessClass = new FitnessClass
        {
            InstructorId = "instructor123",
            CenterId = "center456",
            Name = "Morning Yoga",
            Category = Category.Yoga,
            Intensity = Intensity.Easy,
            Description = "Yoga class.",
            StartTime = DateTime.UtcNow.AddDays(1),
            Duration = 60,
            MaxCapacity = 2,
            IsActive = true,
            SeatBookingEnabled = false,
            BookingList = new List<Booking>
            {
                new Booking { UserId = "user1", SeatNumber = 0, CheckedInAt = DateTime.MinValue },
                new Booking { UserId = "user2", SeatNumber = 0, CheckedInAt = DateTime.MinValue }
            },
            WaitlistUserIds = new List<string> { "waitlistUser1", "waitlistUser2" }
        };
        var collection = _database.GetCollection<FitnessClass>("Classes");
        await collection.InsertOneAsync(fitnessClass);

        // Act
        var result = await _repository.CancelClassBookingForUserAsync(fitnessClass.Id!, "waitlistUser1");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.BookingList.Count);
        Assert.AreEqual(1, result.WaitlistUserIds.Count);
        Assert.IsFalse(result.WaitlistUserIds.Contains("waitlistUser1"));
        Assert.IsTrue(result.WaitlistUserIds.Contains("waitlistUser2"));
    }

    [TestMethod]
    public async Task CancelClassBookingForUserAsync_WhenUserNotFound_ThrowsException()
    {
        // Arrange
        var fitnessClass = new FitnessClass
        {
            InstructorId = "instructor123",
            CenterId = "center456",
            Name = "Morning Yoga",
            Category = Category.Yoga,
            Intensity = Intensity.Easy,
            Description = "Yoga class.",
            StartTime = DateTime.UtcNow.AddDays(1),
            Duration = 60,
            MaxCapacity = 5,
            IsActive = true,
            BookingList = new List<Booking>(),
            WaitlistUserIds = new List<string>()
        };
        var collection = _database.GetCollection<FitnessClass>("Classes");
        await collection.InsertOneAsync(fitnessClass);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<Exception>(
            async () => await _repository.CancelClassBookingForUserAsync(fitnessClass.Id!, "nonExistentUser")
        );
    }

    [TestMethod]
    public async Task DeleteClassAsync_RemovesClassFromDatabase()
    {
        // Arrange
        var fitnessClass = new FitnessClass
        {
            InstructorId = "instructor123",
            CenterId = "center456",
            Name = "Morning Yoga",
            Category = Category.Yoga,
            Intensity = Intensity.Easy,
            Description = "Yoga class to be deleted.",
            StartTime = DateTime.UtcNow.AddDays(1),
            Duration = 60,
            MaxCapacity = 20,
            IsActive = true,
            BookingList = new List<Booking>()
        };
        var collection = _database.GetCollection<FitnessClass>("Classes");
        await collection.InsertOneAsync(fitnessClass);

        // Act
        await _repository.DeleteClassAsync(fitnessClass.Id!);

        // Assert
        var deletedClass = await _repository.GetClassByIdAsync(fitnessClass.Id!);
        Assert.IsNull(deletedClass);
    }

    [TestMethod]
    public async Task DeleteClassAsync_WhenClassNotFound_ThrowsException()
    {
        // Arrange
        var nonExistentClassId = "nonExistentClass123";

        // Act & Assert
        await Assert.ThrowsExceptionAsync<Exception>(
            async () => await _repository.DeleteClassAsync(nonExistentClassId)
        );
    }
}