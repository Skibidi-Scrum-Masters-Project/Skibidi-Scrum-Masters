using ClassService.Model;
using Mongo2Go;
using MongoDB.Driver;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace ClassService.Tests;

[TestClass]
public class ClassRepositoryTests
{
    private MongoDbRunner _runner = null!;
    private IMongoDatabase _database = null!;
    private ClassRepository _repository = null!;
    private RecordingHandler _handler = null!;

    [TestInitialize]
    public void Setup()
    {
        _runner = MongoDbRunner.Start();
        var client = new MongoClient(_runner.ConnectionString);
        _database = client.GetDatabase("TestDatabase");

        _handler = new RecordingHandler();
        var httpClient = new HttpClient(_handler)
        {
            BaseAddress = new Uri("http://fake-socialservice")
        };

        var factory = new FakeHttpClientFactory(httpClient);

        _repository = new ClassRepository(_database, factory);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _runner.Dispose();
    }

    // --------- HTTP test doubles ---------

    public sealed class RecordingHandler : HttpMessageHandler
    {
        public List<HttpRequestMessage> Requests { get; } = new();
        public HttpStatusCode StatusCodeToReturn { get; set; } = HttpStatusCode.OK;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);

            return Task.FromResult(new HttpResponseMessage(StatusCodeToReturn)
            {
                Content = new StringContent("ok")
            });
        }
    }

    public sealed class FakeHttpClientFactory : IHttpClientFactory
    {
        private readonly HttpClient _client;

        public FakeHttpClientFactory(HttpClient client)
        {
            _client = client;
        }

        public HttpClient CreateClient(string name) => _client;
    }

    // --------- Existing tests ---------

    [TestMethod]
    public async Task CreateClassAsync_AddsClassToDatabase()
    {
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

        var result = await _repository.CreateClassAsync(fitnessClass);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Id);
        Assert.AreEqual("instructor123", result.InstructorId);
        Assert.AreEqual("Morning Yoga", result.Name);
        Assert.IsTrue(result.IsActive);

        var collection = _database.GetCollection<FitnessClass>("Classes");
        var foundClass = await collection.Find(c => c.Id == result.Id).FirstOrDefaultAsync();
        Assert.IsNotNull(foundClass);
        Assert.AreEqual("Morning Yoga", foundClass.Name);
    }

    [TestMethod]
    public async Task CreateClassAsync_SetsIsActiveToTrue()
    {
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
            IsActive = false
        };

        var result = await _repository.CreateClassAsync(fitnessClass);

        Assert.IsTrue(result.IsActive);
    }

    [TestMethod]
    public async Task GetAllActiveClassesAsync_ReturnsOnlyActiveClasses()
    {
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

        var result = await _repository.GetAllActiveClassesAsync();

        var resultList = result.ToList();
        Assert.AreEqual(1, resultList.Count);
        Assert.IsTrue(resultList.All(c => c.IsActive));
        Assert.AreEqual("Active Yoga", resultList[0].Name);
    }

    [TestMethod]
    public async Task GetAllActiveClassesAsync_WhenNoActiveClasses_ReturnsEmptyList()
    {
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

        var result = await _repository.GetAllActiveClassesAsync();

        var resultList = result.ToList();
        Assert.AreEqual(0, resultList.Count);
    }

    [TestMethod]
    public async Task BookClassForUserNoSeatAsync_AddsBookingToClass()
    {
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

        var result = await _repository.BookClassForUserNoSeatAsync(fitnessClass.Id!, "user123");

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.BookingList.Count);
        Assert.AreEqual("user123", result.BookingList[0].UserId);
        Assert.AreEqual(0, result.BookingList[0].SeatNumber);
    }

    [TestMethod]
    public async Task BookClassForUserNoSeatAsync_WhenFull_AddsToWaitlist()
    {
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

        var result = await _repository.BookClassForUserNoSeatAsync(fitnessClass.Id!, "user3");

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.BookingList.Count);
        Assert.AreEqual(1, result.WaitlistUserIds.Count);
        Assert.IsTrue(result.WaitlistUserIds.Contains("user3"));
    }

    [TestMethod]
    public async Task BookClassForUserWithSeatAsync_AddsBookingWithSeat()
    {
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

        var result = await _repository.BookClassForUserWithSeatAsync(fitnessClass.Id!, "user123", 2);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.BookingList.Count);
        Assert.AreEqual("user123", result.BookingList[0].UserId);
        Assert.AreEqual(2, result.BookingList[0].SeatNumber);
        Assert.IsTrue(result.SeatMap![2]);
    }

    [TestMethod]
    public async Task BookClassForUserWithSeatAsync_WhenSeatTaken_ThrowsException()
    {
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
                new Booking { UserId = "user1", SeatNumber = 2, CheckedInAt = DateTime.MinValue }
            },
            WaitlistUserIds = new List<string>()
        };
        var collection = _database.GetCollection<FitnessClass>("Classes");
        await collection.InsertOneAsync(fitnessClass);

        await Assert.ThrowsExceptionAsync<Exception>(
            async () => await _repository.BookClassForUserWithSeatAsync(fitnessClass.Id!, "user123", 2)
        );
    }

    [TestMethod]
    public async Task CancelClassBookingForUserAsync_RemovesBooking()
    {
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

        var result = await _repository.CancelClassBookingForUserAsync(fitnessClass.Id!, "user123");

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.BookingList.Count);
        Assert.IsFalse(result.BookingList.Any(b => b.UserId == "user123"));
        Assert.IsTrue(result.BookingList.Any(b => b.UserId == "user456"));
    }

    [TestMethod]
    public async Task CancelClassBookingForUserAsync_WithSeat_FreesUpSeat()
    {
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

        var result = await _repository.CancelClassBookingForUserAsync(fitnessClass.Id!, "user123");

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.BookingList.Count);
        Assert.IsFalse(result.SeatMap![2]);
    }

    [TestMethod]
    public async Task CancelClassBookingForUserAsync_MovesWaitlistUserToBooking_NoSeat()
    {
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

        var result = await _repository.CancelClassBookingForUserAsync(fitnessClass.Id!, "user123");

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

        var result = await _repository.CancelClassBookingForUserAsync(fitnessClass.Id!, "user123");

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

        var result = await _repository.CancelClassBookingForUserAsync(fitnessClass.Id!, "waitlistUser1");

        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.BookingList.Count);
        Assert.AreEqual(1, result.WaitlistUserIds.Count);
        Assert.IsFalse(result.WaitlistUserIds.Contains("waitlistUser1"));
        Assert.IsTrue(result.WaitlistUserIds.Contains("waitlistUser2"));
    }

    [TestMethod]
    public async Task CancelClassBookingForUserAsync_WhenUserNotFound_ThrowsException()
    {
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

        await Assert.ThrowsExceptionAsync<Exception>(
            async () => await _repository.CancelClassBookingForUserAsync(fitnessClass.Id!, "nonExistentUser")
        );
    }

    [TestMethod]
    public async Task DeleteClassAsync_RemovesClassFromDatabase()
    {
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

        await _repository.DeleteClassAsync(fitnessClass.Id!);

        var deletedClass = await _repository.GetClassByIdAsync(fitnessClass.Id!);
        Assert.IsNull(deletedClass);
    }

    [TestMethod]
    public async Task DeleteClassAsync_WhenClassNotFound_ThrowsException()
    {
        var nonExistentClassId = "nonExistentClass123";

        await Assert.ThrowsExceptionAsync<Exception>(
            async () => await _repository.DeleteClassAsync(nonExistentClassId)
        );
    }

    [TestMethod]
    public async Task FinishClass_CreatesClassResultsForAllAttendees()
    {
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
            MaxCapacity = 10,
            IsActive = true,
            BookingList = new List<Booking>
            {
                new Booking { UserId = "user1", SeatNumber = 0 },
                new Booking { UserId = "user2", SeatNumber = 0 },
                new Booking { UserId = "user3", SeatNumber = 0 }
            }
        };
        var collection = _database.GetCollection<FitnessClass>("Classes");
        await collection.InsertOneAsync(fitnessClass);

        await _repository.FinishClass(fitnessClass.Id!);

        var resultsCollection = _database.GetCollection<ClassResult>("ClassResults");
        var results = resultsCollection.Find(r => r.ClassId == fitnessClass.Id).ToList();
        Assert.AreEqual(3, results.Count);
        Assert.IsTrue(results.Any(r => r.UserId == "user1"));
        Assert.IsTrue(results.Any(r => r.UserId == "user2"));
        Assert.IsTrue(results.Any(r => r.UserId == "user3"));
        Assert.IsTrue(results.All(r => r.CaloriesBurned > 0));
        Assert.IsTrue(results.All(r => r.DurationMin == 60));
    }

    [TestMethod]
    public async Task FinishClass_SetsIsActiveToFalse()
    {
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
            MaxCapacity = 10,
            IsActive = true,
            BookingList = new List<Booking>
            {
                new Booking { UserId = "user1", SeatNumber = 0 }
            }
        };
        var collection = _database.GetCollection<FitnessClass>("Classes");
        await collection.InsertOneAsync(fitnessClass);

        await _repository.FinishClass(fitnessClass.Id!);

        var updatedClass = await _repository.GetClassByIdAsync(fitnessClass.Id!);
        Assert.IsFalse(updatedClass.IsActive);
    }

    [TestMethod]
    public async Task FinishClass_WithEmptyBookingList_CreatesNoResults()
    {
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
            MaxCapacity = 10,
            IsActive = true,
            BookingList = new List<Booking>()
        };
        var collection = _database.GetCollection<FitnessClass>("Classes");
        await collection.InsertOneAsync(fitnessClass);

        await _repository.FinishClass(fitnessClass.Id!);

        var resultsCollection = _database.GetCollection<ClassResult>("ClassResults");
        var results = resultsCollection.Find(r => r.ClassId == fitnessClass.Id).ToList();
        Assert.AreEqual(0, results.Count);

        var updatedClass = await _repository.GetClassByIdAsync(fitnessClass.Id!);
        Assert.IsFalse(updatedClass.IsActive);
    }

    // --------- NEW TEST: verifies event POST to SocialService ---------

    [TestMethod]
    public async Task FinishClass_SendsEventToSocialService_ForEachAttendant()
    {
        var fitnessClass = new FitnessClass
        {
            InstructorId = "instructor123",
            CenterId = "center456",
            Name = "Event Test Class",
            Category = Category.Yoga,
            Intensity = Intensity.Easy,
            Description = "Test class.",
            StartTime = DateTime.UtcNow.AddDays(1),
            Duration = 60,
            MaxCapacity = 10,
            IsActive = true,
            BookingList = new List<Booking>
            {
                new Booking { UserId = "user1", SeatNumber = 0 },
                new Booking { UserId = "user2", SeatNumber = 0 }
            }
        };

        await _database.GetCollection<FitnessClass>("Classes").InsertOneAsync(fitnessClass);

        await _repository.FinishClass(fitnessClass.Id!);

        Assert.AreEqual(2, _handler.Requests.Count);

        foreach (var req in _handler.Requests)
        {
            Assert.AreEqual(HttpMethod.Post, req.Method);
            Assert.IsNotNull(req.RequestUri);
            Assert.AreEqual("/internal/events/class-workout-completed", req.RequestUri!.AbsolutePath);

            var body = await req.Content!.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(body);

            var hasEventId =
                (doc.RootElement.TryGetProperty("eventId", out var e1) && !string.IsNullOrWhiteSpace(e1.GetString())) ||
                (doc.RootElement.TryGetProperty("EventId", out var e2) && !string.IsNullOrWhiteSpace(e2.GetString()));

            Assert.IsTrue(hasEventId, $"Expected EventId in payload but got: {body}");
        }
    }
}
