using FitnessApp.Shared.Models;
using MongoDB.Driver;
using MongoDB.Bson;
using Mongo2Go;

namespace UserService.Tests;

[TestClass]
public class UserRepositoryTests
{
    private UserRepository _userRepository = null!;
    private IMongoDatabase _database = null!;
    private MongoDbRunner _runner = null!;

    [TestInitialize]
    public void Setup()
    {
        _runner = MongoDbRunner.Start();
        var client = new MongoClient(_runner.ConnectionString);
        _database = client.GetDatabase("test_fitness_db");
        _userRepository = new UserRepository(_database);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _runner?.Dispose();
    }

    [TestMethod]
    public void CreateUser_ShouldAddUserToDatabase()
    {
        //Arrange
        var newUser = new User
        {
            Username = "testuser",
            Email = "testuser@example.com",
            HashedPassword = "password123"
        };
        
        //Act
        var result = _userRepository.CreateUser(newUser);
        
        //Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("testuser", result.Username);
        Assert.AreEqual("testuser@example.com", result.Email);
        Assert.IsNotNull(result.Id);
        Assert.IsNotNull(result.Salt);
        Assert.IsNotNull(result.HashedPassword);
    }

    [TestMethod]
    public void GetUserById_WithValidId_ShouldReturnUser()
    {
        // Arrange
        var newUser = new User
        {
            Username = "testuser",
            Email = "testuser@example.com",
            HashedPassword = "password123"
        };
        var createdUser = _userRepository.CreateUser(newUser);
        // Act
        var fetchedUser = _userRepository.GetUserById(createdUser.Id!);
        // Assert
        Assert.IsNotNull(fetchedUser);
        Assert.AreEqual(createdUser.Id, fetchedUser.Id);
        Assert.AreEqual(createdUser.Username, fetchedUser.Username);
        Assert.AreEqual(createdUser.Email, fetchedUser.Email);

    }
    [TestMethod]
    public void CreateDuplicateUser_ShouldThrowException()
    {
        // Arrange
        var newUser = new User
        {
            Username = "testuser",
            Email = "testuser@example.com",
            HashedPassword = "password123"
        };
        _userRepository.CreateUser(newUser);
        // Act & Assert
        Assert.ThrowsException<ArgumentException>(() => _userRepository.CreateUser(newUser));
    }
    [TestMethod]
    public void UpdateUser_ShouldModifyExistingUser()
    {
        // Arrange
        var newUser = new User
        {
            Username = "testuser",
            Email = "testuser@example.com",
            HashedPassword = "password123"
        };
        var createdUser = _userRepository.CreateUser(newUser);
        createdUser.Email = "updateduser@example.com";
        // Act
        var updatedUser = _userRepository.UpdateUser(createdUser);
        // Assert
        Assert.IsNotNull(updatedUser);
        Assert.AreEqual(createdUser.Id, updatedUser.Id);
        
    }

    [TestMethod]
    public void DeleteUser_ShouldRemoveUserFromDatabase()
    {
        // Arrange
        var newUser = new User
        {
            Username = "testuser",
            Email = "testuser@example.com",
            HashedPassword = "password123"
        };
        var createdUser = _userRepository.CreateUser(newUser);
        // Act
        _userRepository.DeleteUser(createdUser.Id!);
        var fetchedUser = _userRepository.GetUserById(createdUser.Id!);
        // Assert
        Assert.IsNull(fetchedUser);
    }
    [TestMethod]
    public void DeleteUser_WithNoUserWithId_ShouldThrowException()
    {
        // Act & Assert
        Assert.ThrowsException<ArgumentException>(() => _userRepository.DeleteUser("1234567890abcdef12345678"));
    }
    [TestMethod]
    public void GetAllUsersByRole_ShouldReturnUsersWithSpecifiedRole()
    {
        // Arrange
        var adminUser = new User
        {
            Username = "adminuser",
            Email = "adminuser@example.com",
            HashedPassword = "adminpassword",
            Role = Role.Admin
        };
        var regularUser = new User
        {
            Username = "regularuser",
            Email = "regularuser@example.com",
            HashedPassword = "userpassword",
            Role = Role.Member
        };
        _userRepository.CreateUser(adminUser);
        _userRepository.CreateUser(regularUser);
        // Act
        var adminUsers = _userRepository.GetUsersByRole(Role.Admin);
        var memberUsers = _userRepository.GetUsersByRole(Role.Member);
        // Assert
        Assert.AreEqual(1, adminUsers.Count);
        Assert.AreEqual("adminuser", adminUsers[0].Username);
        Assert.AreEqual(1, memberUsers.Count);
        Assert.AreEqual("regularuser", memberUsers[0].Username);
    }

    [TestMethod]
    public void GetAllUsersByRole_WithNoUsers_ShouldReturnEmptyList()
    {
        // Act
        var guestUsers = _userRepository.GetUsersByRole(Role.Admin);
        // Assert
        Assert.IsNotNull(guestUsers);
        Assert.AreEqual(0, guestUsers.Count);
    }

}