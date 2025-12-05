using FitnessApp.Shared.Models;

namespace AccessControlService.Tests;

[TestClass]
public class LockerRepositoryTests
{
    [TestInitialize]
    public void Setup()
    {
        // TBA: Setup repository with test database/context
        // var repository = new LockerRepository(testContext);
    }

    [TestMethod]
    public void RentLocker_ShouldAssignLockerToUser()
    {
        // TBA: Implement locker rental test
        Assert.Inconclusive("Test not implemented yet");
    }

    [TestMethod]
    public void GetAvailableLockers_ShouldReturnUnrentedLockers()
    {
        // TBA: Implement available lockers test
        Assert.Inconclusive("Test not implemented yet");
    }

    [TestMethod]
    public void ReleaseLocker_ShouldMakeLockerAvailable()
    {
        // TBA: Implement locker release test
        Assert.Inconclusive("Test not implemented yet");
    }
}