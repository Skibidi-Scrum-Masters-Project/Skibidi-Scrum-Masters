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

    [TestMethod]
    public async Task BookClassForUserWithSeat_ReturnsOkResult_WithBookedClass()
    {
        // Arrange
        var classId = "class123";
        var userId = "user456";
        var seatNumber = 2;
        var bookedClass = new FitnessClass
        {
            Id = classId,
            InstructorId = "instructor123",
            CenterId = "center456",
            Name = "Seated Yoga",
            Category = Category.Yoga,
            Intensity = Intensity.Easy,
            Description = "Yoga with seat selection.",
            StartTime = DateTime.UtcNow.AddDays(1),
            Duration = 60,
            MaxCapacity = 5,
            IsActive = true,
            SeatBookingEnabled = true,
            SeatMap = new bool[5],
            BookingList = new List<Booking>
            {
                new Booking { UserId = userId, SeatNumber = seatNumber, CheckedInAt = DateTime.MinValue }
            }
        };
        bookedClass.SeatMap![seatNumber] = true;
        _mockRepository.Setup(r => r.BookClassForUserWithSeatAsync(classId, userId, seatNumber)).ReturnsAsync(bookedClass);

        // Act
        var result = await _controller.BookClassForUserWithSeat(classId, userId, seatNumber);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreEqual(bookedClass, okResult.Value);
    }

    [TestMethod]
    public async Task BookClassForUserWithSeat_WhenRepositoryThrows_ReturnsBadRequest()
    {
        // Arrange
        var classId = "class123";
        var userId = "user456";
        var seatNumber = 2;
        _mockRepository.Setup(r => r.BookClassForUserWithSeatAsync(classId, userId, seatNumber))
            .ThrowsAsync(new Exception("Seat already booked."));

        // Act
        var result = await _controller.BookClassForUserWithSeat(classId, userId, seatNumber);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);
    }

    [TestMethod]
    public async Task CancelClassBookingForUser_ReturnsOkResult_WithUpdatedClass()
    {
        // Arrange
        var classId = "class123";
        var userId = "user456";
        var updatedClass = new FitnessClass
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
            BookingList = new List<Booking>()
        };
        _mockRepository.Setup(r => r.CancelClassBookingForUserAsync(classId, userId)).ReturnsAsync(updatedClass);

        // Act
        var result = await _controller.CancelClassBookingForUser(classId, userId);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreEqual(updatedClass, okResult.Value);
    }

    [TestMethod]
    public async Task CancelClassBookingForUser_WhenRepositoryThrows_ReturnsBadRequest()
    {
        // Arrange
        var classId = "class123";
        var userId = "user456";
        _mockRepository.Setup(r => r.CancelClassBookingForUserAsync(classId, userId))
            .ThrowsAsync(new Exception("User does not have a booking or waitlist entry in this class."));

        // Act
        var result = await _controller.CancelClassBookingForUser(classId, userId);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);
    }

    [TestMethod]
    public async Task BookClassForUserWithFriendsNoSeats_ReturnsOkResult()
    {
        // Arrange
        var classId = "class123";
        var userId = "user456";
        var friends = new List<string> { "friend1", "friend2" };
        var classInfo = new FitnessClass
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
            MaxCapacity = 10,
            IsActive = true,
            SeatBookingEnabled = false,
            BookingList = new List<Booking>()
        };
        _mockRepository.Setup(r => r.GetClassByIdAsync(classId)).ReturnsAsync(classInfo);
        _mockRepository.Setup(r => r.BookClassForUserNoSeatAsync(classId, It.IsAny<string>())).ReturnsAsync(classInfo);

        // Act
        var result = await _controller.BookClassForUserWithFriendsNoSeats(classId, userId, friends);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
    }

    [TestMethod]
    public async Task BookClassForUserWithFriendsNoSeats_WhenNotEnoughSpots_ReturnsBadRequest()
    {
        // Arrange
        var classId = "class123";
        var userId = "user456";
        var friends = new List<string> { "friend1", "friend2", "friend3" };
        var classInfo = new FitnessClass
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
            MaxCapacity = 3,
            IsActive = true,
            SeatBookingEnabled = false,
            BookingList = new List<Booking>
            {
                new Booking { UserId = "existingUser", SeatNumber = 0, CheckedInAt = DateTime.MinValue }
            }
        };
        _mockRepository.Setup(r => r.GetClassByIdAsync(classId)).ReturnsAsync(classInfo);

        // Act
        var result = await _controller.BookClassForUserWithFriendsNoSeats(classId, userId, friends);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);
    }

    [TestMethod]
    public async Task BookClassForUserWithFriendsNoSeats_WhenSeatBookingEnabled_ReturnsBadRequest()
    {
        // Arrange
        var classId = "class123";
        var userId = "user456";
        var friends = new List<string> { "friend1" };
        var classInfo = new FitnessClass
        {
            Id = classId,
            InstructorId = "instructor123",
            CenterId = "center456",
            Name = "Seated Yoga",
            Category = Category.Yoga,
            Intensity = Intensity.Easy,
            Description = "Yoga with seat selection.",
            StartTime = DateTime.UtcNow.AddDays(1),
            Duration = 60,
            MaxCapacity = 10,
            IsActive = true,
            SeatBookingEnabled = true,
            BookingList = new List<Booking>()
        };
        _mockRepository.Setup(r => r.GetClassByIdAsync(classId)).ReturnsAsync(classInfo);

        // Act
        var result = await _controller.BookClassForUserWithFriendsNoSeats(classId, userId, friends);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);
    }

    [TestMethod]
    public async Task BookClassForUserWithFriendsWithSeats_ReturnsOkResult()
    {
        // Arrange
        var classId = "class123";
        var userId = "user456";
        var friends = new List<string> { "friend1", "friend2" };
        var seats = new List<int> { 1, 2, 3 };
        var seatMap = new bool[10];
        var classInfo = new FitnessClass
        {
            Id = classId,
            InstructorId = "instructor123",
            CenterId = "center456",
            Name = "Seated Yoga",
            Category = Category.Yoga,
            Intensity = Intensity.Easy,
            Description = "Yoga with seat selection.",
            StartTime = DateTime.UtcNow.AddDays(1),
            Duration = 60,
            MaxCapacity = 10,
            IsActive = true,
            SeatBookingEnabled = true,
            SeatMap = seatMap,
            BookingList = new List<Booking>()
        };
        _mockRepository.Setup(r => r.GetClassByIdAsync(classId)).ReturnsAsync(classInfo);
        _mockRepository.Setup(r => r.BookClassForUserWithSeatAsync(classId, It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(classInfo);

        // Act
        var result = await _controller.BookClassForUserWithFriendsWithSeats(classId, userId, friends, seats);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
    }

    [TestMethod]
    public async Task BookClassForUserWithFriendsWithSeats_WhenSeatCountMismatch_ReturnsBadRequest()
    {
        // Arrange
        var classId = "class123";
        var userId = "user456";
        var friends = new List<string> { "friend1", "friend2" };
        var seats = new List<int> { 1, 2 }; // Only 2 seats for 3 users
        var classInfo = new FitnessClass
        {
            Id = classId,
            InstructorId = "instructor123",
            CenterId = "center456",
            Name = "Seated Yoga",
            Category = Category.Yoga,
            Intensity = Intensity.Easy,
            Description = "Yoga with seat selection.",
            StartTime = DateTime.UtcNow.AddDays(1),
            Duration = 60,
            MaxCapacity = 10,
            IsActive = true,
            SeatBookingEnabled = true,
            SeatMap = new bool[10],
            BookingList = new List<Booking>()
        };
        _mockRepository.Setup(r => r.GetClassByIdAsync(classId)).ReturnsAsync(classInfo);

        // Act
        var result = await _controller.BookClassForUserWithFriendsWithSeats(classId, userId, friends, seats);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);
    }

    [TestMethod]
    public async Task BookClassForUserWithFriendsWithSeats_WhenSeatBookingNotEnabled_ReturnsBadRequest()
    {
        // Arrange
        var classId = "class123";
        var userId = "user456";
        var friends = new List<string> { "friend1" };
        var seats = new List<int> { 1, 2 };
        var classInfo = new FitnessClass
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
            MaxCapacity = 10,
            IsActive = true,
            SeatBookingEnabled = false,
            BookingList = new List<Booking>()
        };
        _mockRepository.Setup(r => r.GetClassByIdAsync(classId)).ReturnsAsync(classInfo);

        // Act
        var result = await _controller.BookClassForUserWithFriendsWithSeats(classId, userId, friends, seats);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);
    }

    [TestMethod]
    public async Task BookClassForUserWithFriendsWithSeats_WhenSeatAlreadyBooked_ReturnsBadRequest()
    {
        // Arrange
        var classId = "class123";
        var userId = "user456";
        var friends = new List<string> { "friend1" };
        var seats = new List<int> { 1, 2 };
        var seatMap = new bool[10];
        seatMap[2] = true; // Seat 2 is already booked
        var classInfo = new FitnessClass
        {
            Id = classId,
            InstructorId = "instructor123",
            CenterId = "center456",
            Name = "Seated Yoga",
            Category = Category.Yoga,
            Intensity = Intensity.Easy,
            Description = "Yoga with seat selection.",
            StartTime = DateTime.UtcNow.AddDays(1),
            Duration = 60,
            MaxCapacity = 10,
            IsActive = true,
            SeatBookingEnabled = true,
            SeatMap = seatMap,
            BookingList = new List<Booking>()
        };
        _mockRepository.Setup(r => r.GetClassByIdAsync(classId)).ReturnsAsync(classInfo);

        // Act
        var result = await _controller.BookClassForUserWithFriendsWithSeats(classId, userId, friends, seats);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        Assert.AreEqual(400, badRequestResult.StatusCode);
    }

    [TestMethod]
    public async Task DeleteClass_ReturnsNoContent()
    {
        // Arrange
        var classId = "class123";
        var classInfo = new FitnessClass
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
            MaxCapacity = 10,
            IsActive = true,
            BookingList = new List<Booking>()
        };
        _mockRepository.Setup(r => r.GetClassByIdAsync(classId)).ReturnsAsync(classInfo);
        _mockRepository.Setup(r => r.DeleteClassAsync(classId)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteClass(classId);

        // Assert
        var noContentResult = result as NoContentResult;
        Assert.IsNotNull(noContentResult);
        Assert.AreEqual(204, noContentResult.StatusCode);
    }

    [TestMethod]
    public async Task DeleteClass_WhenClassNotFound_ReturnsNotFound()
    {
        // Arrange
        var classId = "nonexistent123";
        _mockRepository.Setup(r => r.GetClassByIdAsync(classId)).ReturnsAsync((FitnessClass)null!);

        // Act
        var result = await _controller.DeleteClass(classId);

        // Assert
        var notFoundResult = result as NotFoundObjectResult;
        Assert.IsNotNull(notFoundResult);
        Assert.AreEqual(404, notFoundResult.StatusCode);
    }
}