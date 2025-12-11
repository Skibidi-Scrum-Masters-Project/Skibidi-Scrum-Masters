using AccessControlService.Models;
using AccessControlService.Repositories;
using Mongo2Go;
using MongoDB.Driver;

namespace AccessControlService.Tests;

[TestClass]
public class AccessControlRepositoryTests
{
    private MongoDbRunner _runner = null!;
    private IMongoDatabase _database = null!;
    private AccessControlRepository _repository = null!;
    private IMongoCollection<LockerRoom> _lockerRoomsCollection = null!;
    private IMongoCollection<EntryPoint> _entryPointsCollection = null!;

    [TestInitialize]
    public void Initialize()
    {
        _runner = MongoDbRunner.Start(singleNodeReplSet: true);
        var client = new MongoClient(_runner.ConnectionString);
        _database = client.GetDatabase("AccessControlTests");
        _repository = new AccessControlRepository(_database);
        _lockerRoomsCollection = _database.GetCollection<LockerRoom>("LockerRooms");
        _entryPointsCollection = _database.GetCollection<EntryPoint>("EntryPoints");
    }

    [TestCleanup]
    public void Cleanup()
    {
        _runner.Dispose();
    }

    #region CreateLockerRoom Tests

    [TestMethod]
    public async Task CreateLockerRoom_ValidLockerRoom_InsertsIntoDatabase()
    {
        // Arrange
        var lockerRoom = new LockerRoom
        {
            Capacity = 50,
            Lockers = new List<Locker>
            {
                new Locker { LockerId = "L1", IsLocked = false, UserId = null },
                new Locker { LockerId = "L2", IsLocked = false, UserId = null }
            }
        };

        // Act
        var result = await _repository.CreateLockerRoom(lockerRoom);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Id);
        var dbLockerRoom = await _lockerRoomsCollection.Find(lr => lr.Id == result.Id).FirstOrDefaultAsync();
        Assert.IsNotNull(dbLockerRoom);
        Assert.AreEqual(50, dbLockerRoom.Capacity);
        Assert.AreEqual(2, dbLockerRoom.Lockers!.Count);
    }

    [TestMethod]
    public async Task CreateLockerRoom_NullLockers_InsertsSuccessfully()
    {
        // Arrange
        var lockerRoom = new LockerRoom
        {
            Capacity = 0,
            Lockers = null
        };

        // Act
        var result = await _repository.CreateLockerRoom(lockerRoom);

        // Assert
        Assert.IsNotNull(result);
        var dbLockerRoom = await _lockerRoomsCollection.Find(lr => lr.Id == result.Id).FirstOrDefaultAsync();
        Assert.IsNotNull(dbLockerRoom);
        Assert.IsNull(dbLockerRoom.Lockers);
    }

    #endregion

    #region OpenDoor Tests

    [TestMethod]
    public async Task OpenDoor_ValidUserId_CreatesEntryPoint()
    {
        // Arrange
        var userId = "user123";

        // Act
        var result = await _repository.OpenDoor(userId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(userId, result.UserId);
        Assert.IsNotNull(result.Id);
        Assert.AreNotEqual(DateTime.MinValue, result.EnteredAt);
        Assert.AreEqual(DateTime.MinValue, result.ExitedAt);

        var dbEntryPoint = await _entryPointsCollection.Find(ep => ep.Id == result.Id).FirstOrDefaultAsync();
        Assert.IsNotNull(dbEntryPoint);
        Assert.AreEqual(userId, dbEntryPoint.UserId);
    }

    [TestMethod]
    public async Task OpenDoor_MultipleEntries_CreatesMultipleEntryPoints()
    {
        // Arrange
        var userId1 = "user123";
        var userId2 = "user456";

        // Act
        var result1 = await _repository.OpenDoor(userId1);
        var result2 = await _repository.OpenDoor(userId2);

        // Assert
        Assert.IsNotNull(result1);
        Assert.IsNotNull(result2);
        Assert.AreNotEqual(result1.Id, result2.Id);

        var count = await _entryPointsCollection.CountDocumentsAsync(ep => true);
        Assert.AreEqual(2, count);
    }

    #endregion

    #region CloseDoor Tests

    [TestMethod]
    public async Task CloseDoor_ValidUserId_UpdatesExitTime()
    {
        // Arrange
        var userId = "user123";
        await _repository.OpenDoor(userId);

        // Act
        var result = await _repository.CloseDoor(userId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(userId, result.UserId);
        Assert.AreNotEqual(DateTime.MinValue, result.ExitedAt);
        Assert.IsTrue(result.ExitedAt > result.EnteredAt);
    }

    [TestMethod]
    public async Task CloseDoor_UserNotEntered_ReturnsNull()
    {
        // Arrange
        var userId = "nonexistent";

        // Act
        var result = await _repository.CloseDoor(userId);

        // Assert
        Assert.IsNull(result);
    }

    #endregion

    #region GetAllAvailableLockers Tests

    [TestMethod]
    public async Task GetAllAvailableLockers_LockerRoomExists_ReturnsAvailableLockers()
    {
        // Arrange
        var lockerRoom = new LockerRoom
        {
            Capacity = 5,
            Lockers = new List<Locker>
            {
                new Locker { LockerId = "L1", IsLocked = false, UserId = null },
                new Locker { LockerId = "L2", IsLocked = true, UserId = "user123" },
                new Locker { LockerId = "L3", IsLocked = false, UserId = null }
            }
        };
        var created = await _repository.CreateLockerRoom(lockerRoom);

        // Act
        var result = await _repository.GetAllAvailableLockers(created.Id!);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.All(l => !l.IsLocked));
    }

    [TestMethod]
    public async Task GetAllAvailableLockers_LockerRoomNotFound_ReturnsEmptyList()
    {
        // Arrange
        var nonExistentId = "507f1f77bcf86cd799439011";

        // Act
        var result = await _repository.GetAllAvailableLockers(nonExistentId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Count);
    }

    #endregion

    #region LockLocker Tests

    [TestMethod]
    public async Task LockLocker_ValidParameters_LocksLocker()
    {
        // Arrange
        var lockerRoom = new LockerRoom
        {
            Capacity = 3,
            Lockers = new List<Locker>
            {
                new Locker { LockerId = "L1", IsLocked = false, UserId = null }
            }
        };
        var created = await _repository.CreateLockerRoom(lockerRoom);
        var userId = "user123";

        // Act
        var result = await _repository.LockLocker(created.Id!, "L1", userId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("L1", result.LockerId);
        Assert.AreEqual(userId, result.UserId);
        Assert.IsTrue(result.IsLocked);

        var dbLockerRoom = await _lockerRoomsCollection.Find(lr => lr.Id == created.Id).FirstOrDefaultAsync();
        var locker = dbLockerRoom.Lockers!.First(l => l.LockerId == "L1");
        Assert.IsTrue(locker.IsLocked);
        Assert.AreEqual(userId, locker.UserId);
    }

    [TestMethod]
    public async Task LockLocker_LockerNotFound_ReturnsNull()
    {
        // Arrange
        var lockerRoom = new LockerRoom
        {
            Capacity = 1,
            Lockers = new List<Locker>
            {
                new Locker { LockerId = "L1", IsLocked = false, UserId = null }
            }
        };
        var created = await _repository.CreateLockerRoom(lockerRoom);

        // Act
        var result = await _repository.LockLocker(created.Id!, "NonExistent", "user123");

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task LockLocker_NullUserId_ReturnsNull()
    {
        // Arrange
        var lockerRoom = new LockerRoom
        {
            Capacity = 1,
            Lockers = new List<Locker>
            {
                new Locker { LockerId = "L1", IsLocked = false, UserId = null }
            }
        };
        var created = await _repository.CreateLockerRoom(lockerRoom);

        // Act
        var result = await _repository.LockLocker(created.Id!, "L1", null!);

        // Assert
        Assert.IsNull(result);
    }

    #endregion

    #region UnlockLocker Tests

    [TestMethod]
    public async Task UnlockLocker_ValidParameters_UnlocksLocker()
    {
        // Arrange
        var userId = "user123";
        var lockerRoom = new LockerRoom
        {
            Capacity = 1,
            Lockers = new List<Locker>
            {
                new Locker { LockerId = "L1", IsLocked = true, UserId = userId }
            }
        };
        var created = await _repository.CreateLockerRoom(lockerRoom);

        // Act
        var result = await _repository.UnlockLocker(created.Id!, "L1", userId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("L1", result.LockerId);
        Assert.IsNull(result.UserId);
        Assert.IsFalse(result.IsLocked);

        var dbLockerRoom = await _lockerRoomsCollection.Find(lr => lr.Id == created.Id).FirstOrDefaultAsync();
        var locker = dbLockerRoom.Lockers!.First(l => l.LockerId == "L1");
        Assert.IsFalse(locker.IsLocked);
        Assert.IsNull(locker.UserId);
    }

    [TestMethod]
    public async Task UnlockLocker_WrongUser_ReturnsNull()
    {
        // Arrange
        var lockerRoom = new LockerRoom
        {
            Capacity = 1,
            Lockers = new List<Locker>
            {
                new Locker { LockerId = "L1", IsLocked = true, UserId = "user123" }
            }
        };
        var created = await _repository.CreateLockerRoom(lockerRoom);

        // Act
        var result = await _repository.UnlockLocker(created.Id!, "L1", "user456");

        // Assert
        Assert.IsNull(result);
    }

    #endregion

    #region GetByIdAsync Tests

    [TestMethod]
    public async Task GetByIdAsync_ValidId_ReturnsLockerRoom()
    {
        // Arrange
        var lockerRoom = new LockerRoom
        {
            Capacity = 10,
            Lockers = new List<Locker>
            {
                new Locker { LockerId = "L1", IsLocked = false, UserId = null }
            }
        };
        var created = await _repository.CreateLockerRoom(lockerRoom);

        // Act
        var result = await _repository.GetByIdAsync(created.Id!);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(created.Id, result.Id);
        Assert.AreEqual(10, result.Capacity);
    }

    [TestMethod]
    public async Task GetByIdAsync_InvalidId_ReturnsNull()
    {
        // Arrange
        var nonExistentId = "507f1f77bcf86cd799439011";

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        Assert.IsNull(result);
    }

    #endregion

    #region SaveAsync Tests

    [TestMethod]
    public async Task SaveAsync_UpdateExistingLockerRoom_UpdatesData()
    {
        // Arrange
        var lockerRoom = new LockerRoom
        {
            Capacity = 10,
            Lockers = new List<Locker>
            {
                new Locker { LockerId = "L1", IsLocked = false, UserId = null }
            }
        };
        var created = await _repository.CreateLockerRoom(lockerRoom);

        // Act
        created.Capacity = 50;
        created.Lockers!.Add(new Locker { LockerId = "L2", IsLocked = false, UserId = null });
        await _repository.SaveAsync(created);

        // Assert
        var dbLockerRoom = await _lockerRoomsCollection.Find(lr => lr.Id == created.Id).FirstOrDefaultAsync();
        Assert.IsNotNull(dbLockerRoom);
        Assert.AreEqual(50, dbLockerRoom.Capacity);
        Assert.AreEqual(2, dbLockerRoom.Lockers!.Count);
    }

    #endregion

    #region GetCrowd Tests

    [TestMethod]
    public async Task GetCrowd_WithActiveUsers_ReturnsCrowdCount()
    {
        // Arrange
        var userId1 = "user1";
        var userId2 = "user2";
        var userId3 = "user3";

        // Create entry points for users currently in the facility (ExitedAt = DateTime.MinValue)
        await _repository.OpenDoor(userId1);
        await _repository.OpenDoor(userId2);
        await _repository.OpenDoor(userId3);

        // Act
        var result = await _repository.GetCrowd();

        // Assert
        Assert.AreEqual(3, result);
    }

    [TestMethod]
    public async Task GetCrowd_WithMixedEntryExit_CountsOnlyActiveUsers()
    {
        // Arrange
        var userId1 = "user1";
        var userId2 = "user2";
        var userId3 = "user3";

        // User 1 and 2 enter
        await _repository.OpenDoor(userId1);
        await _repository.OpenDoor(userId2);

        // User 1 exits
        await _repository.CloseDoor(userId1);

        // User 3 enters
        await _repository.OpenDoor(userId3);

        // Act
        var result = await _repository.GetCrowd();

        // Assert
        Assert.AreEqual(2, result, "Should count only users still in facility (user2 and user3)");
    }

    [TestMethod]
    public async Task GetCrowd_WithNoActiveUsers_ReturnsZero()
    {
        // Arrange - No entry points created

        // Act
        var result = await _repository.GetCrowd();

        // Assert
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public async Task GetCrowd_AllUsersExited_ReturnsZero()
    {
        // Arrange
        var userId1 = "user1";
        var userId2 = "user2";

        // Users enter and then exit
        await _repository.OpenDoor(userId1);
        await _repository.OpenDoor(userId2);
        await _repository.CloseDoor(userId1);
        await _repository.CloseDoor(userId2);

        // Act
        var result = await _repository.GetCrowd();

        // Assert
        Assert.AreEqual(0, result);
    }

    #endregion
}

