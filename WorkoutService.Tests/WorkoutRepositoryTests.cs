using FitnessApp.Shared.Models;

namespace WorkoutService.Tests;

[TestClass]
public class WorkoutRepositoryTests
{
    [TestInitialize]
    public void Setup()
    {
        // TBA: Setup repository with test database/context
        // var repository = new WorkoutRepository(testContext);
    }

    [TestMethod]
    public void CreateWorkout_ShouldAddWorkoutToDatabase()
    {
        // TBA: Implement workout creation test
        Assert.Inconclusive("Test not implemented yet");
    }

    [TestMethod]
    public void AddExerciseToWorkout_ShouldUpdateWorkout()
    {
        // TBA: Implement exercise addition test
        Assert.Inconclusive("Test not implemented yet");
    }

    [TestMethod]
    public void CalculateWorkoutVolume_ShouldReturnCorrectTotal()
    {
        // TBA: Implement volume calculation test
        Assert.Inconclusive("Test not implemented yet");
    }
}