using FitnessApp.Shared.Models;

namespace ClassService.Tests;

[TestClass]
public class ClassRepositoryTests
{
    [TestInitialize]
    public void Setup()
    {
        // TBA: Setup repository with test database/context
        // var repository = new ClassRepository(testContext);
    }

    [TestMethod]
    public void CreateClass_ShouldAddClassToDatabase()
    {
        // TBA: Implement class creation test
        Assert.Inconclusive("Test not implemented yet");
    }

    [TestMethod]
    public void EnrollUserInClass_ShouldAddUserToClassList()
    {
        // TBA: Implement class enrollment test
        Assert.Inconclusive("Test not implemented yet");
    }

    [TestMethod]
    public void GetAvailableClasses_ShouldReturnNonFullClasses()
    {
        // TBA: Implement available classes test
        Assert.Inconclusive("Test not implemented yet");
    }
}