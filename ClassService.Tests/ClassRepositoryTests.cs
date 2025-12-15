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
            BaseAddress = new Uri("http://fake-service")
        };

        var factory = new FakeHttpClientFactory(httpClient);
        _repository = new ClassRepository(_database, factory);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _runner.Dispose();
    }

    // ---------------- HTTP test doubles ----------------

    public sealed class RecordingHandler : HttpMessageHandler
    {
        public List<HttpRequestMessage> Requests { get; } = new();
        public HttpStatusCode StatusCodeToReturn { get; set; } = HttpStatusCode.OK;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
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

    // ---------------- Tests ----------------

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
            Duration = 60,
            MaxCapacity = 10,
            IsActive = true,
            BookingList = new List<Booking>
            {
                new() { UserId = "user1", SeatNumber = 0 },
                new() { UserId = "user2", SeatNumber = 0 },
                new() { UserId = "user3", SeatNumber = 0 }
            }
        };

        await _database.GetCollection<FitnessClass>("Classes")
            .InsertOneAsync(fitnessClass);

        await _repository.FinishClass(fitnessClass.Id!);

        var results = _database
            .GetCollection<ClassResult>("ClassResults")
            .Find(r => r.ClassId == fitnessClass.Id)
            .ToList();

        Assert.AreEqual(3, results.Count);
    }

    [TestMethod]
    public async Task FinishClass_SetsIsActiveToFalse()
    {
        var fitnessClass = new FitnessClass
        {
            InstructorId = "instructor123",
            CenterId = "center456",
            Name = "Yoga",
            Category = Category.Yoga,
            Intensity = Intensity.Easy,
            Duration = 60,
            MaxCapacity = 5,
            IsActive = true,
            BookingList = new List<Booking>
            {
                new() { UserId = "user1", SeatNumber = 0 }
            }
        };

        await _database.GetCollection<FitnessClass>("Classes")
            .InsertOneAsync(fitnessClass);

        await _repository.FinishClass(fitnessClass.Id!);

        var updated = await _repository.GetClassByIdAsync(fitnessClass.Id!);
        Assert.IsFalse(updated!.IsActive);
    }

    [TestMethod]
    public async Task FinishClass_WithEmptyBookingList_CreatesNoResults()
    {
        var fitnessClass = new FitnessClass
        {
            InstructorId = "instructor123",
            CenterId = "center456",
            Name = "Yoga",
            Category = Category.Yoga,
            Intensity = Intensity.Easy,
            Duration = 60,
            MaxCapacity = 5,
            IsActive = true,
            BookingList = new List<Booking>()
        };

        await _database.GetCollection<FitnessClass>("Classes")
            .InsertOneAsync(fitnessClass);

        await _repository.FinishClass(fitnessClass.Id!);

        var results = _database
            .GetCollection<ClassResult>("ClassResults")
            .Find(r => r.ClassId == fitnessClass.Id)
            .ToList();

        Assert.AreEqual(0, results.Count);
    }

    // ⭐ FIXET: tester KUN SocialService-events (ignorerer Analytics)

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
            Duration = 60,
            MaxCapacity = 10,
            IsActive = true,
            BookingList = new List<Booking>
            {
                new() { UserId = "user1", SeatNumber = 0 },
                new() { UserId = "user2", SeatNumber = 0 }
            }
        };

        await _database
            .GetCollection<FitnessClass>("Classes")
            .InsertOneAsync(fitnessClass);

        await _repository.FinishClass(fitnessClass.Id!);

        // 🔍 Kun SocialService-calls
        var socialRequests = _handler.Requests
            .Where(r => r.RequestUri!.AbsolutePath ==
                        "/internal/events/class-workout-completed")
            .ToList();

        Assert.AreEqual(2, socialRequests.Count);

        foreach (var req in socialRequests)
        {
            Assert.AreEqual(HttpMethod.Post, req.Method);

            var body = await req.Content!.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(body);

            // ✅ ACCEPTERER BÅDE eventId OG EventId
            var hasEventId =
                (doc.RootElement.TryGetProperty("eventId", out var e1) &&
                 !string.IsNullOrWhiteSpace(e1.GetString())) ||
                (doc.RootElement.TryGetProperty("EventId", out var e2) &&
                 !string.IsNullOrWhiteSpace(e2.GetString()));

            Assert.IsTrue(hasEventId, $"EventId missing in payload: {body}");
        }
    }

}
