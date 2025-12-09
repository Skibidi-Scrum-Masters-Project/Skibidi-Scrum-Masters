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

        // 1) Det skal være OkObjectResult
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult, "Expected OkObjectResult");

        // 2) Locker blev opdateret korrekt
        Assert.AreEqual(userId, locker.UserId);
        Assert.IsTrue(locker.IsLocked);

        // 3) SaveAsync blev kaldt præcis én gang med vores lockerRoom
        _mockRepository.Verify(
            r => r.SaveAsync(It.Is<LockerRoom>(lr => lr == lockerRoom)),
            Times.Once);
    }
}
