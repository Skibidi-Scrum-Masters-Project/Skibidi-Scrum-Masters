using Microsoft.AspNetCore.Mvc;
using Moq;
using AccessControlService.Controllers;
using AccessControlService.Models;
using AccessControlService.Repositories;

namespace AccessControlService.Tests;

[TestClass]
public class LockersControllerTests
{
    private LockersController _controller = null!;
    private Mock<ILockerRepository> _mockRepository = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new Mock<ILockerRepository>();
        _controller = new LockersController(_mockRepository.Object);
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
}
