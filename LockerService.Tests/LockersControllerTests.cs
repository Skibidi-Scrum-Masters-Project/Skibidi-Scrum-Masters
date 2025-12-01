using Microsoft.AspNetCore.Mvc;
using FitnessApp.Shared.Models;
using LockerService.Controllers;
using Moq;

namespace LockerService.Tests;

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
    public void GetLockers_ShouldReturnAllLockers()
    {
        // TBA: Implement test for getting all lockers
        Assert.Inconclusive("Test not implemented yet");
    }

    [TestMethod]
    public void GetLockers_WhenNoLockers_ShouldReturnEmptyList()
    {
        // TBA: Implement test for empty locker list
        Assert.Inconclusive("Test not implemented yet");
    }

    [TestMethod]
    public void GetLockers_ShouldReturnOkResult()
    {
        // TBA: Implement test for OK status code
        Assert.Inconclusive("Test not implemented yet");
    }
}