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

    #region CreateLockerRoom

    [TestMethod]
    public async Task CreateLockerRoom_ReturnsOk()
    {
        var lockerRoom = new LockerRoom { Id = "room1", Capacity = 10 };

        _mockRepository
            .Setup(r => r.CreateLockerRoom(It.IsAny<LockerRoom>()))
            .ReturnsAsync(lockerRoom);

        var result = await _controller.CreateLockerRoom(lockerRoom);

        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
    }

    #endregion

    #region OpenDoor

    [TestMethod]
    public async Task OpenDoor_ReturnsOk()
    {
        var entry = new EntryPoint
        {
            UserId = "user1",
            EnteredAt = DateTime.UtcNow,
            ExitedAt = DateTime.MinValue
        };

        _mockRepository
            .Setup(r => r.OpenDoor("user1"))
            .ReturnsAsync(entry);

        var result = await _controller.OpenDoor("user1");

        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
    }

    [TestMethod]
    public async Task OpenDoor_NotFound()
    {
        _mockRepository
            .Setup(r => r.OpenDoor("user1"))
            .ReturnsAsync((EntryPoint?)null);

        var result = await _controller.OpenDoor("user1");

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    #endregion

    #region CloseDoor

    [TestMethod]
    public async Task CloseDoor_ReturnsOk()
    {
        var entry = new EntryPoint
        {
            UserId = "user1",
            EnteredAt = DateTime.UtcNow.AddHours(-1),
            ExitedAt = DateTime.UtcNow
        };

        _mockRepository
            .Setup(r => r.CloseDoor("user1"))
            .ReturnsAsync(entry);

        var result = await _controller.CloseDoor("user1");

        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
    }

    [TestMethod]
    public async Task CloseDoor_NotFound()
    {
        _mockRepository
            .Setup(r => r.CloseDoor("user1"))
            .ReturnsAsync((EntryPoint?)null);

        var result = await _controller.CloseDoor("user1");

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    #endregion

    #region AvailableLockers

    [TestMethod]
    public async Task GetAvailableLockers_ReturnsList()
    {
        var lockers = new List<Locker>
        {
            new() { LockerId = "L1", IsLocked = false },
            new() { LockerId = "L2", IsLocked = false }
        };

        _mockRepository
            .Setup(r => r.GetAllAvailableLockers("room1"))
            .ReturnsAsync(lockers);

        var result = await _controller.GetAvailableLockersById("room1");

        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
    }

    #endregion

    #region LockLocker

    [TestMethod]
    public async Task LockLocker_ReturnsLocker()
    {
        var locker = new Locker
        {
            LockerId = "L1",
            UserId = "user1",
            IsLocked = true
        };

        _mockRepository
            .Setup(r => r.LockLocker("room1", "L1", "user1"))
            .ReturnsAsync(locker);

        var result = await _controller.LockLocker("room1", "L1", "user1");

        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
    }

    #endregion

    #region GetLockerForUser

    [TestMethod]
    public async Task GetLocker_UserHasLocker_ReturnsOk()
    {
        var locker = new Locker
        {
            LockerId = "L1",
            UserId = "user1",
            IsLocked = true
        };

        _mockRepository
            .Setup(r => r.GetLocker("room1", "user1"))
            .ReturnsAsync(locker);

        var result = await _controller.GetLocker("room1", "user1");

        Assert.IsInstanceOfType(result, typeof(OkObjectResult));

        var ok = result as OkObjectResult;
        Assert.IsNotNull(ok?.Value);
    }

    [TestMethod]
    public async Task GetLocker_UserHasNoLocker_ReturnsNull()
    {
        _mockRepository
            .Setup(r => r.GetLocker("room1", "user1"))
            .ReturnsAsync((Locker?)null);

        var result = await _controller.GetLocker("room1", "user1");

        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
    }

    #endregion

    #region OpenLocker

    [TestMethod]
    public async Task OpenLocker_ReturnsUnlockedLocker()
    {
        var locker = new Locker
        {
            LockerId = "L1",
            IsLocked = false
        };

        _mockRepository
            .Setup(r => r.UnlockLocker("room1", "L1", "user1"))
            .ReturnsAsync(locker);

        var result = await _controller.OpenLocker("room1", "L1", "user1");

        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
    }

    #endregion

    #region Crowd

    [TestMethod]
    public async Task GetCrowd_ReturnsValue()
    {
        _mockRepository
            .Setup(r => r.GetCrowd())
            .ReturnsAsync(42);

        var result = await _controller.GetCrowd();

        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
    }

    #endregion

    #region UserStatus

    [TestMethod]
    public async Task GetUserStatus_NeverCheckedIn_ReturnsNull()
    {
        _mockRepository
            .Setup(r => r.GetUserStatus("user1"))
            .ReturnsAsync((DateTime?)null);

        var result = await _controller.GetUserStatus("user1");

        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
    }

    [TestMethod]
    public async Task GetUserStatus_CheckedIn_ReturnsMinValue()
    {
        _mockRepository
            .Setup(r => r.GetUserStatus("user1"))
            .ReturnsAsync(DateTime.MinValue);

        var result = await _controller.GetUserStatus("user1");

        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
    }

    [TestMethod]
    public async Task GetUserStatus_CheckedOut_ReturnsDate()
    {
        var exit = DateTime.UtcNow;

        _mockRepository
            .Setup(r => r.GetUserStatus("user1"))
            .ReturnsAsync(exit);

        var result = await _controller.GetUserStatus("user1");

        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
    }

    #endregion
}
