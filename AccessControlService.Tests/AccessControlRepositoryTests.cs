using AccessControlService.Models;
using AccessControlService.Repositories;
using Mongo2Go;
using MongoDB.Driver;
using Moq;
using Moq.Protected;
using System.Net;

namespace AccessControlService.Tests;

[TestClass]
public class AccessControlRepositoryTests
{
    private MongoDbRunner _runner = null!;
    private IMongoDatabase _database = null!;
    private AccessControlRepository _repository = null!;
    private IMongoCollection<LockerRoom> _lockerRooms = null!;
    private IMongoCollection<EntryPoint> _entryPoints = null!;
    private HttpClient _httpClient = null!;

    [TestInitialize]
    public void Setup()
    {
        _runner = MongoDbRunner.Start(singleNodeReplSet: true);
        var client = new MongoClient(_runner.ConnectionString);
        _database = client.GetDatabase("AccessControlTests");

        // Mock HttpClient (Analytics calls)
        var handler = new Mock<HttpMessageHandler>();
        handler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK
            });

        _httpClient = new HttpClient(handler.Object);

        _repository = new AccessControlRepository(_database, _httpClient);

        _lockerRooms = _database.GetCollection<LockerRoom>("LockerRooms");
        _entryPoints = _database.GetCollection<EntryPoint>("EntryPoints");
    }

    [TestCleanup]
    public void Cleanup()
    {
        _runner.Dispose();
    }

    #region CreateLockerRoom

    [TestMethod]
    public async Task CreateLockerRoom_InsertsDocument()
    {
        var room = new LockerRoom
        {
            Capacity = 10,
            Lockers = new()
            {
                new Locker { LockerId = "L1" }
            }
        };

        var result = await _repository.CreateLockerRoom(room);

        var dbRoom = await _lockerRooms.Find(r => r.Id == result.Id).FirstOrDefaultAsync();

        Assert.IsNotNull(dbRoom);
        Assert.AreEqual(10, dbRoom.Capacity);
        Assert.AreEqual(1, dbRoom.Lockers!.Count);
    }

    #endregion

    #region OpenDoor / CloseDoor

    [TestMethod]
    public async Task OpenDoor_CreatesEntryPoint()
    {
        var entry = await _repository.OpenDoor("user1");

        Assert.AreEqual("user1", entry.UserId);
        Assert.AreEqual(DateTime.MinValue, entry.ExitedAt);

        var dbEntry = await _entryPoints.Find(e => e.Id == entry.Id).FirstOrDefaultAsync();
        Assert.IsNotNull(dbEntry);
    }

    [TestMethod]
    public async Task CloseDoor_UpdatesExitedAt()
    {
        await _repository.OpenDoor("user1");

        var closed = await _repository.CloseDoor("user1");

        Assert.IsNotNull(closed);
        Assert.AreNotEqual(DateTime.MinValue, closed.ExitedAt);
    }

    [TestMethod]
    public async Task CloseDoor_NoActiveEntry_ReturnsNull()
    {
        var result = await _repository.CloseDoor("ghost");
        Assert.IsNull(result);
    }

    #endregion

    #region GetUserStatus

    [TestMethod]
    public async Task GetUserStatus_NeverCheckedIn_ReturnsNull()
    {
        var status = await _repository.GetUserStatus("user1");
        Assert.IsNull(status);
    }

    [TestMethod]
    public async Task GetUserStatus_CheckedIn_ReturnsMinValue()
    {
        await _repository.OpenDoor("user1");

        var status = await _repository.GetUserStatus("user1");

        Assert.AreEqual(DateTime.MinValue, status);
    }

    [TestMethod]
    public async Task GetUserStatus_CheckedOut_ReturnsExitTime()
    {
        await _repository.OpenDoor("user1");
        var closed = await _repository.CloseDoor("user1");

        var status = await _repository.GetUserStatus("user1");

        Assert.AreEqual(closed!.ExitedAt, status);
    }

    #endregion

    #region Lockers

    [TestMethod]
    public async Task GetAllAvailableLockers_ReturnsOnlyUnlocked()
    {
        var room = new LockerRoom
        {
            Lockers = new()
            {
                new Locker { LockerId = "L1", IsLocked = false },
                new Locker { LockerId = "L2", IsLocked = true }
            }
        };

        var created = await _repository.CreateLockerRoom(room);

        var lockers = await _repository.GetAllAvailableLockers(created.Id!);

        Assert.AreEqual(1, lockers.Count);
        Assert.AreEqual("L1", lockers[0].LockerId);
    }

    [TestMethod]
    public async Task LockLocker_AssignsUser()
    {
        var room = new LockerRoom
        {
            Lockers = new()
            {
                new Locker { LockerId = "L1" }
            }
        };

        var created = await _repository.CreateLockerRoom(room);

        var locker = await _repository.LockLocker(created.Id!, "L1", "user1");

        Assert.IsTrue(locker!.IsLocked);
        Assert.AreEqual("user1", locker.UserId);
    }

    [TestMethod]
    public async Task UnlockLocker_RemovesUser()
    {
        var room = new LockerRoom
        {
            Lockers = new()
            {
                new Locker { LockerId = "L1", IsLocked = true, UserId = "user1" }
            }
        };

        var created = await _repository.CreateLockerRoom(room);

        var locker = await _repository.UnlockLocker(created.Id!, "L1", "user1");

        Assert.IsFalse(locker!.IsLocked);
        Assert.IsNull(locker.UserId);
    }

    #endregion

    #region GetLocker (by userId)

    [TestMethod]
    public async Task GetLocker_UserHasLocker_ReturnsLocker()
    {
        var room = new LockerRoom
        {
            Lockers = new()
            {
                new Locker { LockerId = "L1", IsLocked = true, UserId = "user1" }
            }
        };

        var created = await _repository.CreateLockerRoom(room);

        var locker = await _repository.GetLocker(created.Id!, "user1");

        Assert.IsNotNull(locker);
        Assert.AreEqual("L1", locker!.LockerId);
    }

    [TestMethod]
    public async Task GetLocker_UserHasNoLocker_ReturnsNull()
    {
        var room = new LockerRoom
        {
            Lockers = new()
            {
                new Locker { LockerId = "L1" }
            }
        };

        var created = await _repository.CreateLockerRoom(room);

        var locker = await _repository.GetLocker(created.Id!, "ghost");

        Assert.IsNull(locker);
    }

    #endregion

    #region GetCrowd

    [TestMethod]
    public async Task GetCrowd_CountsOnlyActiveUsers()
    {
        await _repository.OpenDoor("u1");
        await _repository.OpenDoor("u2");
        await _repository.OpenDoor("u3");

        await _repository.CloseDoor("u1");

        var crowd = await _repository.GetCrowd();

        Assert.AreEqual(2, crowd);
    }

    [TestMethod]
    public async Task GetCrowd_NoUsers_ReturnsZero()
    {
        var crowd = await _repository.GetCrowd();
        Assert.AreEqual(0, crowd);
    }

    #endregion
}
