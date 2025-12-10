using AccessControlService.Models;

namespace AccessControlService.Tests;

[TestClass]
public class AccessControlRepositoryTests
{

    [TestMethod]
    public void Locker_CanBeLockedForUser()
    {
        // Arrange
        var locker = new Locker
        {
            LockerId = 5,
            UserId = 0,
            IsLocked = false
        };

        var userId = 42;

        // Act
        locker.UserId = userId;
        locker.IsLocked = true;

        // Assert
        Assert.AreEqual(userId, locker.UserId);
        Assert.IsTrue(locker.IsLocked);
    }
}