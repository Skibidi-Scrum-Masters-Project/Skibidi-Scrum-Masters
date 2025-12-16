using Microsoft.AspNetCore.Mvc;
using FitnessApp.Shared.Models;
using ClassService.Controllers;
using Moq;

namespace ClassService.Tests;

[TestClass]
public class ClassesControllerTests
{
    private ClassesController _controller = null!;
    private Mock<IClassRepository> _mockRepository = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new Mock<IClassRepository>();
        _controller = new ClassesController(_mockRepository.Object);
    }

    [TestMethod]
    public void GetClasses_ShouldReturnAllClasses()
    {
        // TBA: Implement test for getting all fitness classes
        Assert.Inconclusive("Test not implemented yet");
    }

    [TestMethod]
    public void GetClasses_WhenNoClasses_ShouldReturnEmptyList()
    {
        // TBA: Implement test for empty class list
        Assert.Inconclusive("Test not implemented yet");
    }

    [TestMethod]
    public void GetClasses_ShouldReturnOkResult()
    {
        // TBA: Implement test for OK status code
        Assert.Inconclusive("Test not implemented yet");
    }
}