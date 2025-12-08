using Microsoft.AspNetCore.Mvc;
using FitnessApp.Shared.Models;
using UserService.Controllers;
using Moq;

namespace UserService.Tests;

[TestClass]
public class UsersControllerTests
{
    private UsersController _controller = null!;
    private Mock<IUserRepository> _mockRepository = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new Mock<IUserRepository>();
        _controller = new UsersController(_mockRepository.Object);
    }

    [TestMethod]
    public void GetUsers_ShouldReturnAllUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new User
            {
                Id = "1",
                Username = "user1",
                Email = "user1@example.com",
                Role = 0
            },
            new User
            {
                Id = "2",
                Username = "user2",
                Email = "user2@example.com",
                Role = 0
            },
            new User
            {
                Id = "3",
                Username = "coach1",
                Email = "coach@example.com",
                Role = 0
            }
        };

        _mockRepository.Setup(repo => repo.GetAllUsers()).Returns(users);

        // Act
        var result = _controller.GetUsers();

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult, "Expected OkObjectResult");
        Assert.AreEqual(200, okResult.StatusCode);

        var returnedUsers = okResult.Value as IEnumerable<User>;
        Assert.IsNotNull(returnedUsers, "Expected list of users");
        Assert.AreEqual(3, returnedUsers.Count(), "Expected 3 users");

        _mockRepository.Verify(repo => repo.GetAllUsers(), Times.Once);
    }

    [TestMethod]
    public void GetUsers_WhenNoUsers_ShouldReturnEmptyList()
    {
        // Arrange
        var emptyUsers = new List<User>();
        _mockRepository.Setup(repo => repo.GetAllUsers()).Returns(emptyUsers);

        // Act
        var result = _controller.GetUsers();

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult, "Expected OkObjectResult");
        Assert.AreEqual(200, okResult.StatusCode);

        var returnedUsers = okResult.Value as IEnumerable<User>;
        Assert.IsNotNull(returnedUsers, "Expected empty list of users");
        Assert.AreEqual(0, returnedUsers.Count(), "Expected 0 users");

        _mockRepository.Verify(repo => repo.GetAllUsers(), Times.Once);
    }

    [TestMethod]
    public void GetUsers_ShouldReturnOkResult()
    {
        // Arrange
        var users = new List<User>
        {
            new User
            {
                Id = "1",
                Username = "testuser",
                Email = "test@example.com",
                Role = 0
            }
        };

        _mockRepository.Setup(repo => repo.GetAllUsers()).Returns(users);

        // Act
        var result = _controller.GetUsers();

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult, "Expected OkObjectResult");
        Assert.AreEqual(200, okResult.StatusCode, "Expected status code 200");

        _mockRepository.Verify(repo => repo.GetAllUsers(), Times.Once);
    }
    [TestMethod]
    public void GetUsersByUsername_ShouldReturnUser()
    {
        // Arrange
        var username = "testuser";
        var user = new User
        {
            Id = "1",
            Username = username,
            Email = "test@example.com",
            Role = 0
        };
        _mockRepository.Setup(repo => repo.GetUserByUsername(username)).Returns(user);
        // Act
        var result = _controller.GetUserByUsername(username);
        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult, "Expected OkObjectResult");
        Assert.AreEqual(200, okResult.StatusCode, "Expected status code 200");
        var returnedUser = okResult.Value as User;
        Assert.IsNotNull(returnedUser, "Expected a user object");
        Assert.AreEqual(username, returnedUser.Username, "Usernames should match");
        _mockRepository.Verify(repo => repo.GetUserByUsername(username), Times.Once);
    }
    [TestMethod]
    public void GetUsersByUsername_WhenUserNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var username = "nonexistentuser";
        _mockRepository.Setup(repo => repo.GetUserByUsername(username)).Returns((User?)null);
        // Act
        var result = _controller.GetUserByUsername(username);
        // Assert
        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.IsNotNull(notFoundResult, "Expected NotFoundObjectResult");
        Assert.AreEqual(404, notFoundResult.StatusCode, "Expected status code 404");
        _mockRepository.Verify(repo => repo.GetUserByUsername(username), Times.Once);
    }
    [TestMethod]
    public void CreateUser_ShouldReturnCreatedUser()
    {
        // Arrange
        var newUser = new User
        {
            Username = "newuser",
            Email = "newuser@example.com",
            HashedPassword = "hashedpassword",
            Role = 0
        };

        var createdUser = new User
        {
            Id = "123",
            Username = "newuser",
            Email = "newuser@example.com",
            HashedPassword = "hashedpassword",
            Role = 0
        };

        _mockRepository.Setup(repo => repo.CreateUser(It.IsAny<User>())).Returns(createdUser);

        // Act
        var result = _controller.CreateUser(newUser);

        // Assert
        var createdResult = result.Result as CreatedAtActionResult;
        Assert.IsNotNull(createdResult, "Expected CreatedAtActionResult");
        Assert.AreEqual(201, createdResult.StatusCode);

        var returnedUser = createdResult.Value as User;
        Assert.IsNotNull(returnedUser, "Expected a user object");
        Assert.AreEqual(createdUser.Id, returnedUser.Id);
        Assert.AreEqual(createdUser.Username, returnedUser.Username);

        _mockRepository.Verify(repo => repo.CreateUser(It.IsAny<User>()), Times.Once);
    }

    [TestMethod]
    public void CreateUser_WithInvalidInput_ShouldReturnBadRequest()
    {
        // Arrange
        var invalidUser = new User
        {
            Username = "",
            Email = "invalid",
            Role = 0
        };

        _mockRepository
            .Setup(repo => repo.CreateUser(It.IsAny<User>()))
            .Throws(new ArgumentNullException("user", "User data is required"));

        // Act
        var result = _controller.CreateUser(invalidUser);

        // Assert
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult, "Expected BadRequestObjectResult");
        Assert.AreEqual(400, badRequestResult.StatusCode);

        _mockRepository.Verify(repo => repo.CreateUser(It.IsAny<User>()), Times.Once);
    }

    [TestMethod]
    public void CreateUser_WhenDatabaseError_ShouldReturnInternalServerError()
    {
        // Arrange
        var newUser = new User
        {
            Username = "testuser",
            Email = "test@example.com",
            HashedPassword = "hashedpassword",
            Role = 0
        };

        _mockRepository
            .Setup(repo => repo.CreateUser(It.IsAny<User>()))
            .Throws(new InvalidOperationException("Database connection failed"));

        // Act
        var result = _controller.CreateUser(newUser);

        // Assert
        var statusCodeResult = result.Result as ObjectResult;
        Assert.IsNotNull(statusCodeResult, "Expected ObjectResult");
        Assert.AreEqual(500, statusCodeResult.StatusCode);

        _mockRepository.Verify(repo => repo.CreateUser(It.IsAny<User>()), Times.Once);
    }

    [TestMethod]
    public void CreateUser_WhenUsernameAlreadyExists_ShouldReturnBadRequest()
    {
        // Arrange
        var duplicateUser = new User
        {
            Username = "existinguser",
            Email = "new@example.com",
            HashedPassword = "hashedpassword",
            Role = 0
        };

        _mockRepository
            .Setup(repo => repo.CreateUser(It.IsAny<User>()))
            .Throws(new ArgumentException("Username already exists"));

        // Act
        var result = _controller.CreateUser(duplicateUser);

        // Assert
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult, "Expected BadRequestObjectResult");
        Assert.AreEqual(400, badRequestResult.StatusCode);

        _mockRepository.Verify(repo => repo.CreateUser(It.IsAny<User>()), Times.Once);
    }
}