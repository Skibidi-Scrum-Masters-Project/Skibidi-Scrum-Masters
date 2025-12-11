using Microsoft.AspNetCore.Mvc;
using Moq;
using AccessControlService.Controllers;
using AccessControlService.Models;
using AccessControlService.Repositories;

namespace AccessControlService.Tests;

[TestClass]
public class AccessControlControllerTests
{
    private AccessControlController _controller = null!;
    private Mock<IAccessControlRepository> _mockRepository = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new Mock<IAccessControlRepository>();
        _controller = new AccessControlController(_mockRepository.Object);
    }

    #region CreateLockerRoom Tests

    [TestMethod]
    public async Task CreateLockerRoom_ValidLockerRoom_ReturnsOkWithCreatedLockerRoom()
    {
        // Arrange
        var lockerRoom = new LockerRoom
        {
            Id = "507f1f77bcf86cd799439011",
            Capacity = 50,
            Lockers = new List<Locker>
            {
                new Locker { LockerId = "L1", IsLocked = false, UserId = null }
            }
        };

        _mockRepository.Setup(repo => repo.CreateLockerRoom(It.IsAny<LockerRoom>()))
            .ReturnsAsync(lockerRoom);

        // Act
        var result = await _controller.CreateLockerRoom(lockerRoom);

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        var returnedLockerRoom = okResult.Value as LockerRoom;
        Assert.IsNotNull(returnedLockerRoom);
        Assert.AreEqual(lockerRoom.Id, returnedLockerRoom.Id);
        _mockRepository.Verify(repo => repo.CreateLockerRoom(It.IsAny<LockerRoom>()), Times.Once);
    }

    #endregion

    #region OpenDoor Tests

    [TestMethod]
    public async Task OpenDoor_ValidUserId_ReturnsOkWithEntryPoint()
    {
        // Arrange
        var userId = "user123";
        var entryPoint = new EntryPoint
        {
            Id = "507f1f77bcf86cd799439013",
            UserId = userId,
            EnteredAt = DateTime.UtcNow,
            ExitedAt = DateTime.MinValue
        };

        _mockRepository.Setup(repo => repo.OpenDoor(userId))
            .ReturnsAsync(entryPoint);

        // Act
        var result = await _controller.OpenDoor(userId);

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        var returnedEntryPoint = okResult.Value as EntryPoint;
        Assert.IsNotNull(returnedEntryPoint);
        Assert.AreEqual(userId, returnedEntryPoint.UserId);
        _mockRepository.Verify(repo => repo.OpenDoor(userId), Times.Once);
    }

    [TestMethod]
    public async Task OpenDoor_UserNotFound_ReturnsNotFound()
    {
        // Arrange
        var userId = "nonexistent123";
        _mockRepository.Setup(repo => repo.OpenDoor(userId))
            .ReturnsAsync((EntryPoint?)null);

        // Act
        var result = await _controller.OpenDoor(userId);

        // Assert
        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        var notFoundResult = result as NotFoundObjectResult;
        Assert.IsNotNull(notFoundResult);
        Assert.AreEqual($"User {userId} not found", notFoundResult.Value);
    }

    #endregion

    #region CloseDoor Tests

    [TestMethod]
    public async Task CloseDoor_ValidUserId_ReturnsOkWithEntryPoint()
    {
        // Arrange
        var userId = "user123";
        var entryPoint = new EntryPoint
        {
            Id = "507f1f77bcf86cd799439014",
            UserId = userId,
            EnteredAt = DateTime.UtcNow.AddHours(-2),
            ExitedAt = DateTime.UtcNow
        };

        _mockRepository.Setup(repo => repo.CloseDoor(userId))
            .ReturnsAsync(entryPoint);

        // Act
        var result = await _controller.CloseDoor(userId);

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        var returnedEntryPoint = okResult.Value as EntryPoint;
        Assert.IsNotNull(returnedEntryPoint);
        Assert.AreEqual(userId, returnedEntryPoint.UserId);
        Assert.IsTrue(returnedEntryPoint.ExitedAt > returnedEntryPoint.EnteredAt);
    }

    [TestMethod]
    public async Task CloseDoor_UserNotFound_ReturnsNotFound()
    {
        // Arrange
        var userId = "nonexistent456";
        _mockRepository.Setup(repo => repo.CloseDoor(userId))
            .ReturnsAsync((EntryPoint?)null);

        // Act
        var result = await _controller.CloseDoor(userId);

        // Assert
        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        var notFoundResult = result as NotFoundObjectResult;
        Assert.IsNotNull(notFoundResult);
        Assert.AreEqual($"User {userId} not found", notFoundResult.Value);
    }

    #endregion

    #region GetAvailableLockersById Tests

    [TestMethod]
    public async Task GetAvailableLockersById_ValidLockerRoomId_ReturnsOkWithLockers()
    {
        // Arrange
        var lockerRoomId = "507f1f77bcf86cd799439015";
        var availableLockers = new List<Locker>
        {
            new Locker { LockerId = "L1", IsLocked = false, UserId = null },
            new Locker { LockerId = "L2", IsLocked = false, UserId = null }
        };

        _mockRepository.Setup(repo => repo.GetAllAvailableLockers(lockerRoomId))
            .ReturnsAsync(availableLockers);

        // Act
        var result = await _controller.GetAvailableLockersById(lockerRoomId);

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        var returnedLockers = okResult.Value as List<Locker>;
        Assert.IsNotNull(returnedLockers);
        Assert.AreEqual(2, returnedLockers.Count);
    }

    [TestMethod]
    public async Task GetAvailableLockersById_NoAvailableLockers_ReturnsOkWithEmptyList()
    {
        // Arrange
        var lockerRoomId = "507f1f77bcf86cd799439016";
        var availableLockers = new List<Locker>();

        _mockRepository.Setup(repo => repo.GetAllAvailableLockers(lockerRoomId))
            .ReturnsAsync(availableLockers);

        // Act
        var result = await _controller.GetAvailableLockersById(lockerRoomId);

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        var okResult = result as OkObjectResult;
        var returnedLockers = okResult!.Value as List<Locker>;
        Assert.IsNotNull(returnedLockers);
        Assert.AreEqual(0, returnedLockers.Count);
    }

    #endregion

    #region LockLocker Tests

    [TestMethod]
    public async Task LockLocker_ValidParameters_ReturnsOkWithLocker()
    {
        // Arrange
        var lockerRoomId = "507f1f77bcf86cd799439017";
        var lockerId = "L1";
        var userId = "user123";
        var locker = new Locker
        {
            LockerId = lockerId,
            UserId = userId,
            IsLocked = true
        };

        _mockRepository.Setup(repo => repo.LockLocker(lockerRoomId, lockerId, userId))
            .ReturnsAsync(locker);

        // Act
        var result = await _controller.LockLocker(lockerRoomId, lockerId, userId);

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        var returnedLocker = okResult.Value as Locker;
        Assert.IsNotNull(returnedLocker);
        Assert.AreEqual(lockerId, returnedLocker.LockerId);
        Assert.AreEqual(userId, returnedLocker.UserId);
        Assert.IsTrue(returnedLocker.IsLocked);
    }

    [TestMethod]
    public async Task LockLocker_NullUserId_ReturnsOk()
    {
        // Arrange
        var lockerRoomId = "507f1f77bcf86cd799439018";
        var lockerId = "L1";
        string userId = null!;
        var locker = new Locker { LockerId = lockerId, UserId = userId, IsLocked = true };

        _mockRepository.Setup(repo => repo.LockLocker(lockerRoomId, lockerId, userId))
            .ReturnsAsync(locker);

        // Act
        var result = await _controller.LockLocker(lockerRoomId, lockerId, userId);

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
    }

    #endregion

    #region OpenLocker Tests

    [TestMethod]
    public async Task OpenLocker_ValidParameters_ReturnsOkWithUnlockedLocker()
    {
        // Arrange
        var lockerRoomId = "507f1f77bcf86cd799439021";
        var lockerId = "L1";
        var userId = "user123";
        var locker = new Locker
        {
            LockerId = lockerId,
            UserId = null,
            IsLocked = false
        };

        _mockRepository.Setup(repo => repo.UnlockLocker(lockerRoomId, lockerId, userId))
            .ReturnsAsync(locker);

        // Act
        var result = await _controller.OpenLocker(lockerRoomId, lockerId, userId);

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        var returnedLocker = okResult.Value as Locker;
        Assert.IsNotNull(returnedLocker);
        Assert.AreEqual(lockerId, returnedLocker.LockerId);
        Assert.IsFalse(returnedLocker.IsLocked);
    }

    [TestMethod]
    public async Task OpenLocker_WrongUser_ReturnsOk()
    {
        // Arrange
        var lockerRoomId = "507f1f77bcf86cd799439025";
        var lockerId = "L1";
        var userId = "user999";
        var locker = new Locker
        {
            LockerId = lockerId,
            UserId = "user123",
            IsLocked = true
        };

        _mockRepository.Setup(repo => repo.UnlockLocker(lockerRoomId, lockerId, userId))
            .ReturnsAsync(locker);

        // Act
        var result = await _controller.OpenLocker(lockerRoomId, lockerId, userId);

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
    }

    #endregion

    #region GetCrowd Tests

    [TestMethod]
    public async Task GetCrowd_ValidRequest_ReturnsOkWithCrowdCount()
    {
        // Arrange
        int expectedCrowdCount = 15;
        _mockRepository.Setup(repo => repo.GetCrowd())
            .ReturnsAsync(expectedCrowdCount);

        // Act
        var result = await _controller.GetCrowd();

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(expectedCrowdCount, okResult.Value);
        _mockRepository.Verify(repo => repo.GetCrowd(), Times.Once);
    }

    [TestMethod]
    public async Task GetCrowd_NoCrowd_ReturnsOkWithZero()
    {
        // Arrange
        _mockRepository.Setup(repo => repo.GetCrowd())
            .ReturnsAsync(0);

        // Act
        var result = await _controller.GetCrowd();

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(0, okResult.Value);
    }

    [TestMethod]
    public async Task GetCrowd_LargeCrowd_ReturnsOkWithValue()
    {
        // Arrange
        int expectedCrowdCount = 1000;
        _mockRepository.Setup(repo => repo.GetCrowd())
            .ReturnsAsync(expectedCrowdCount);

        // Act
        var result = await _controller.GetCrowd();

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(expectedCrowdCount, okResult.Value);
    }

    #endregion
}

