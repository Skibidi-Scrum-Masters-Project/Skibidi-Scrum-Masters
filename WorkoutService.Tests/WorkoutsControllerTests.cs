using Microsoft.AspNetCore.Mvc;
using FitnessApp.Shared.Models;
using WorkoutService.Controllers;
using Moq;

namespace WorkoutService.Tests;

[TestClass]
public class WorkoutsControllerTests
{
    private WorkoutsController _controller = null!;
    private Mock<IWorkoutRepository> _mockRepository = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new Mock<IWorkoutRepository>();
        _controller = new WorkoutsController(_mockRepository.Object);
    }

    [TestMethod]
    public void GetWorkouts_ShouldReturnAllWorkouts()
    {
        // TBA: Implement test for getting all workouts
        Assert.Inconclusive("Test not implemented yet");
    }

    [TestMethod]
    public void GetWorkouts_WhenNoWorkouts_ShouldReturnEmptyList()
    {
        // TBA: Implement test for empty workout list
        Assert.Inconclusive("Test not implemented yet");
    }

    [TestMethod]
    public void GetWorkouts_ShouldReturnOkResult()
    {
        // TBA: Implement test for OK status code
        Assert.Inconclusive("Test not implemented yet");
    }
}