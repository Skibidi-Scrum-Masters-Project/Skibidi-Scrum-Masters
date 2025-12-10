using Microsoft.AspNetCore.Mvc;
using ClassService.Model;
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

    [TestMethod]
    public async Task CreateClassAsync_ReturnsOkResult_WithCreatedClass()
    {
        // Arrange
        var fitnessClass = new FitnessClass
        {
            InstructorId = "instructor123",
            CenterId = "center456",
            Name = "Morning Yoga",
            Category = Category.Yoga,
            Intensity = Intensity.Easy,
            Description = "Gentle yoga for beginners.",
            StartTime = DateTime.UtcNow.AddDays(1),
            Duration = 60,
            MaxCapacity = 20,
            IsActive = true
        };
        _mockRepository.Setup(r => r.CreateClassAsync(fitnessClass)).ReturnsAsync(fitnessClass);

        // Act
        var result = await _controller.CreateClassAsync(fitnessClass);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreEqual(fitnessClass, okResult.Value);
    }

    [TestMethod]
    public async Task CreateClassAsync_WhenClassIsNull_ReturnsBadRequest()
    {
        // Arrange
        FitnessClass? fitnessClass = null;

        // Act
        var result = await _controller.CreateClassAsync(fitnessClass!);

        // Assert
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);
    }

    [TestMethod]
    public async Task CreateClassAsync_WhenInstructorIdIsEmpty_ReturnsBadRequest()
    {
        // Arrange
        var fitnessClass = new FitnessClass
        {
            InstructorId = "",
            CenterId = "center456",
            Name = "Morning Yoga",
            Category = Category.Yoga,
            Intensity = Intensity.Easy,
            Description = "Gentle yoga for beginners.",
            StartTime = DateTime.UtcNow.AddDays(1),
            Duration = 60,
            MaxCapacity = 20
        };

        // Act
        var result = await _controller.CreateClassAsync(fitnessClass);

        // Assert
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);
    }

    [TestMethod]
    public async Task CreateClassAsync_WhenDurationIsZero_ReturnsBadRequest()
    {
        // Arrange
        var fitnessClass = new FitnessClass
        {
            InstructorId = "instructor123",
            CenterId = "center456",
            Name = "Morning Yoga",
            Category = Category.Yoga,
            Intensity = Intensity.Easy,
            Description = "Gentle yoga for beginners.",
            StartTime = DateTime.UtcNow.AddDays(1),
            Duration = 0,
            MaxCapacity = 20
        };

        // Act
        var result = await _controller.CreateClassAsync(fitnessClass);

        // Assert
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);
    }

    [TestMethod]
    public async Task CreateClassAsync_WhenMaxCapacityIsZero_ReturnsBadRequest()
    {
        // Arrange
        var fitnessClass = new FitnessClass
        {
            InstructorId = "instructor123",
            CenterId = "center456",
            Name = "Morning Yoga",
            Category = Category.Yoga,
            Intensity = Intensity.Easy,
            Description = "Gentle yoga for beginners.",
            StartTime = DateTime.UtcNow.AddDays(1),
            Duration = 60,
            MaxCapacity = 0
        };

        // Act
        var result = await _controller.CreateClassAsync(fitnessClass);

        // Assert
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);
    }

    [TestMethod]
    public async Task GetAllActiveClassesAsync_ReturnsOkResult_WithClasses()
    {
        // Arrange
        var classes = new List<FitnessClass>
        {
            new FitnessClass
            {
                InstructorId = "instructor123",
                CenterId = "center456",
                Name = "Morning Yoga",
                Category = Category.Yoga,
                Intensity = Intensity.Easy,
                Description = "Gentle yoga.",
                StartTime = DateTime.UtcNow.AddDays(1),
                Duration = 60,
                MaxCapacity = 20,
                IsActive = true
            }
        };
        _mockRepository.Setup(r => r.GetAllActiveClassesAsync()).ReturnsAsync(classes);

        // Act
        var result = await _controller.GetAllActiveClassesAsync();

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
        var returnedClasses = okResult.Value as IEnumerable<FitnessClass>;
        Assert.IsNotNull(returnedClasses);
        Assert.AreEqual(1, returnedClasses.Count());
    }

    [TestMethod]
    public async Task GetAllActiveClassesAsync_ReturnsOkResult_WithEmptyList()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAllActiveClassesAsync()).ReturnsAsync(Enumerable.Empty<FitnessClass>());

        // Act
        var result = await _controller.GetAllActiveClassesAsync();

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
        var returnedClasses = okResult.Value as IEnumerable<FitnessClass>;
        Assert.IsNotNull(returnedClasses);
        Assert.AreEqual(0, returnedClasses.Count());
    }

    [TestMethod]
    public async Task BookClassForUser_ReturnsOkResult_WithBookedClass()
    {
        // Arrange
        var classId = "class123";
        var userId = "user456";
        var bookedClass = new FitnessClass
        {
            Id = classId,
            InstructorId = "instructor123",
            CenterId = "center456",
            Name = "Morning Yoga",
            Category = Category.Yoga,
            Intensity = Intensity.Easy,
            Description = "Yoga class.",
            StartTime = DateTime.UtcNow.AddDays(1),
            Duration = 60,
            MaxCapacity = 5,
            IsActive = true,
            BookingList = new List<Booking>
            {
                new Booking { UserId = userId, SeatNumber = 0, CheckedInAt = DateTime.MinValue }
            }
        };
        _mockRepository.Setup(r => r.BookClassForUserNoSeatAsync(classId, userId)).ReturnsAsync(bookedClass);

        // Act
        var result = await _controller.BookClassForUser(classId, userId);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreEqual(bookedClass, okResult.Value);
    }

    [TestMethod]
    public async Task BookClassForUser_WhenRepositoryThrows_ReturnsBadRequest()
    {
        // Arrange
        var classId = "class123";
        var userId = "user456";
        _mockRepository.Setup(r => r.BookClassForUserNoSeatAsync(classId, userId))
            .ThrowsAsync(new Exception("User already booked in this class."));

        // Act
        var result = await _controller.BookClassForUser(classId, userId);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);
    }
}