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

    [TestMethod]
    public async Task CloseDoor_ValidUser_ShouldReturnOkWithDoor()
    {
        // Arrange
        string userId = "user123";

        var door = new EntryPoint
        {
            EnteredAt = DateTime.UtcNow.AddMinutes(-15),
            ExitedAt = DateTime.MinValue,
            UserId = userId
        };

        _mockRepository
            .Setup(r => r.CloseDoor(userId))
            .ReturnsAsync(door);

        // Act
        var result = await _controller.CloseDoor(userId);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult, "Expected OkObjectResult");

        var returnedDoor = okResult.Value as EntryPoint;
        Assert.IsNotNull(returnedDoor, "Expected EntryPoint in response");
        Assert.AreEqual(userId, returnedDoor.UserId);

        _mockRepository.Verify(r => r.CloseDoor(userId), Times.Once);
    }

    [TestMethod]
    public async Task CloseDoor_UserNotFound_ShouldReturnNotFound()
    {
        // Arrange
        string userId = "ghost-user";

        _mockRepository
            .Setup(r => r.CloseDoor(userId))
            .ReturnsAsync((EntryPoint?)null);

        // Act
        var result = await _controller.CloseDoor(userId);

        // Assert
        var notFoundResult = result as NotFoundObjectResult;
        Assert.IsNotNull(notFoundResult, "Expected NotFoundObjectResult");

        var message = notFoundResult.Value?.ToString();
        StringAssert.Contains(message, userId);

        _mockRepository.Verify(r => r.CloseDoor(userId), Times.Once);
    }
    
    [TestMethod]
    public async Task Door_ValidUser_ShouldReturnOkWithDoor()
    {
        // Arrange
        string userId = "user123";

        var door = new EntryPoint
        {
            EnteredAt = DateTime.Now,
            ExitedAt = DateTime.Now.AddHours(1),
            UserId = userId
        };

        _mockRepository
            .Setup(r => r.OpenDoor(userId))
            .ReturnsAsync(door);

        // Act
        var result = await _controller.Door(userId);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult, "Expected OkObjectResult");

        var returnedDoor = okResult.Value as EntryPoint;
        Assert.IsNotNull(returnedDoor, "Expected Door object in response");
        Assert.AreEqual(door.UserId, returnedDoor.UserId);

        _mockRepository.Verify(r => r.OpenDoor(userId), Times.Once);
    }

    [TestMethod]
    public async Task Door_UserNotFound_ShouldReturnNotFound()
    {
        // Arrange
        string userId = "ghost-user";

        _mockRepository
            .Setup(r => r.OpenDoor(userId))
            .ReturnsAsync((EntryPoint?)null);

        // Act
        var result = await _controller.Door(userId);

        // Assert
        var notFoundResult = result as NotFoundObjectResult;
        Assert.IsNotNull(notFoundResult, "Expected NotFoundObjectResult");

        var message = notFoundResult.Value?.ToString();
        StringAssert.Contains(message, userId);

        _mockRepository.Verify(r => r.OpenDoor(userId), Times.Once);
    }

    [TestMethod]
    public async Task LockLocker_ValidIds_ShouldLockLockerAndReturnOk()
    {
        // Arrange
        int lockerRoomId = 1;
        int lockerId = 5;
        int userId = 42;

        var locker = new Locker
        {
            LockerId = lockerId,
            UserId = 0,
            IsLocked = false
        };

        var lockerRoom = new LockerRoom
        {
            LockerRoomId = lockerRoomId,
            CenterId = 1,
            Capacity = 10,
            Lockers = new List<Locker> { locker }
        };

        _mockRepository
            .Setup(r => r.GetByIdAsync(lockerRoomId))
            .ReturnsAsync(lockerRoom);

        _mockRepository
            .Setup(r => r.SaveAsync(It.IsAny<LockerRoom>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.LockLocker(lockerRoomId, lockerId, userId);

        // Assert

        // 1) Controller should return an OkObjectResult
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult, "Expected OkObjectResult");

        // 2) Locker should now be assigned to the correct user and locked
        Assert.AreEqual(userId, locker.UserId);
        Assert.IsTrue(locker.IsLocked);

        // 3) SaveAsync should only be called once with the same lockerRoom instance
        _mockRepository.Verify(
            r => r.SaveAsync(It.Is<LockerRoom>(lr => lr == lockerRoom)),
            Times.Once);
    }

    [TestMethod]
    public async Task LockLocker_LockerRoomNotFound_ShouldReturnNotFound()
    {
        // Arrange
        string lockerRoomId = "1";
        string lockerId = "5";
        string userId = "42";

        _mockRepository
            .Setup(r => r.GetByIdAsync(lockerRoomId))
            .ReturnsAsync((LockerRoom?)null);

        // Act
        var result = await _controller.LockLocker(lockerRoomId, lockerId, userId);

        // Assert
        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        _mockRepository.Verify(r => r.SaveAsync(It.IsAny<LockerRoom>()), Times.Never);
    }

    [TestMethod]
    public async Task LockLocker_LockerNotFound_ShouldReturnNotFound()
    {
        // Arrange
        int lockerRoomId = 1;
        int lockerId = 99;
        int userId = 42;

        var lockerRoom = new LockerRoom
        {
            LockerRoomId = lockerRoomId,
            CenterId = 1,
            Capacity = 10,
            Lockers = new List<Locker>()
        };

        _mockRepository
            .Setup(r => r.GetByIdAsync(lockerRoomId))
            .ReturnsAsync(lockerRoom);

        // Act
        var result = await _controller.LockLocker(lockerRoomId, lockerId, userId);

        // Assert
        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        _mockRepository.Verify(r => r.SaveAsync(It.IsAny<LockerRoom>()), Times.Never);
    }

    [TestMethod]
    public async Task GetAvailableLockers_ShouldReturnOnlyUnlockedLockers()
    {
        // Arrange
        int lockerRoomId = 1;

        var lockers = new List<Locker>
        {
            new Locker { LockerId = 1, UserId = 0,  IsLocked = false }, // available
            new Locker { LockerId = 2, UserId = 0,  IsLocked = true  }, // locked
            new Locker { LockerId = 3, UserId = 99, IsLocked = false }  // taken by another user
        };

        var lockerRoom = new LockerRoom
        {
            LockerRoomId = lockerRoomId,
            CenterId = 1,
            Capacity = 10,
            Lockers = lockers
        };

        _mockRepository
            .Setup(r => r.GetByIdAsync(lockerRoomId))
            .ReturnsAsync(lockerRoom);

        // Act
        var result = await _controller.GetAvailableLockers(lockerRoomId);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult, "Expected OkObjectResult");

        var returned = okResult.Value as IEnumerable<Locker>;
        Assert.IsNotNull(returned, "Expected list of lockers");

        var list = returned.ToList();

        // Only lockerId = 1 should be available (!IsLocked && UserId == 0)
        Assert.AreEqual(1, list.Count);
        Assert.AreEqual(1, list[0].LockerId);
    }

    [TestMethod]
    public async Task GetAvailableLockers_LockerRoomNotFound_ShouldReturnNotFound()
    {
        // Arrange
        int lockerRoomId = 1;

        _mockRepository
            .Setup(r => r.GetByIdAsync(lockerRoomId))
            .ReturnsAsync((LockerRoom?)null);

        // Act
        var result = await _controller.GetAvailableLockers(lockerRoomId);

        // Assert
        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }
    
    
    [TestMethod]
    public async Task OpenLocker_ValidIds_ShouldUnlockLockerAndReturnOk()
    {
        // Arrange
        int lockerRoomId = 1;
        int lockerId = 5;

        var locker = new Locker
        {
            LockerId = lockerId,
            UserId = 42,
            IsLocked = true
        };

        var lockerRoom = new LockerRoom
        {
            LockerRoomId = lockerRoomId,
            CenterId = 1,
            Capacity = 10,
            Lockers = new List<Locker> { locker }
        };

        _mockRepository
            .Setup(r => r.GetByIdAsync(lockerRoomId))
            .ReturnsAsync(lockerRoom);

        _mockRepository
            .Setup(r => r.SaveAsync(It.IsAny<LockerRoom>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.OpenLocker(lockerRoomId, lockerId);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult, "Expected OkObjectResult");

        // Locker should now be available again
        Assert.AreEqual(0, locker.UserId);
        Assert.IsFalse(locker.IsLocked);

        _mockRepository.Verify(
            r => r.SaveAsync(It.Is<LockerRoom>(lr => lr == lockerRoom)),
            Times.Once);
    }

    [TestMethod]
    public async Task OpenLocker_LockerRoomNotFound_ShouldReturnNotFound()
    {
        // Arrange
        int lockerRoomId = 1;
        int lockerId = 5;

        _mockRepository
            .Setup(r => r.GetByIdAsync(lockerRoomId))
            .ReturnsAsync((LockerRoom?)null);

        // Act
        var result = await _controller.OpenLocker(lockerRoomId, lockerId);

        // Assert
        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        _mockRepository.Verify(r => r.SaveAsync(It.IsAny<LockerRoom>()), Times.Never);
    }

    [TestMethod]
    public async Task OpenLocker_LockerNotFound_ShouldReturnNotFound()
    {
        // Arrange
        int lockerRoomId = 1;
        int lockerId = 99;

        var lockerRoom = new LockerRoom
        {
            LockerRoomId = lockerRoomId,
            CenterId = 1,
            Capacity = 10,
            Lockers = new List<Locker>()
        };

        _mockRepository
            .Setup(r => r.GetByIdAsync(lockerRoomId))
            .ReturnsAsync(lockerRoom);

        // Act
        var result = await _controller.OpenLocker(lockerRoomId, lockerId);

        // Assert
        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
        _mockRepository.Verify(r => r.SaveAsync(It.IsAny<LockerRoom>()), Times.Never);
    }
}
