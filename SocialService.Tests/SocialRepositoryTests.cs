using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MongoDB.Driver;
using SocialService.Models;
using SocialService;
using System;
using FitnessApp.Shared.Models;
using SocialService.Controllers;
using SocialService.Repositories;
using Mongo2Go;

namespace SocialService.Tests;

[TestClass]
public class SocialRepositoryTests
{
    
    private static MongoDbRunner _runner = null!;
    private IMongoDatabase _database = null!;
    private SocialRepository _repository = null!;
    
    private Mock<IMongoCollection<Friendship>> _mockCollection = null!;
    private Mock<IMongoDatabase> _mockDatabase = null!;

    
    [ClassInitialize]
    public static void ClassInit(TestContext _)
    {
        // Starter en embedded MongoDB
        _runner = MongoDbRunner.Start();
    }
    
    [TestInitialize]
    public void TestInit()
    {
        var client = new MongoClient(_runner.ConnectionString);

        // Brug et fast navn til testdatabase
        _database = client.GetDatabase("SocialServiceTests");

        // Ryd collection før hver test, så vi starter clean
        _database.DropCollection("Friendships");

        _repository = new SocialRepository(_database);
    }
    
    [ClassCleanup]
    public static void ClassCleanup()
    {
        _runner.Dispose();
    }
    
    [TestMethod]
    [DoNotParallelize]
    public async Task GetAllFriendRequests_WhenThereIsMultipleStatus_ShouldNotReturnAcceptedOrDeclinedRequests()
    {
        // Arrange
        var userId = 1;
        var collection = _database.GetCollection<Friendship>("Friendships");

        var pending = new Friendship
        {
            SenderId = userId,
            ReceiverId = 2,
            FriendShipStatus = FriendshipStatus.Pending
        };

        var accepted = new Friendship
        {
            SenderId = userId,
            ReceiverId = 3,
            FriendShipStatus = FriendshipStatus.Accepted
        };

        var rejected = new Friendship
        {
            SenderId = userId,
            ReceiverId = 4,
            FriendShipStatus = FriendshipStatus.Declined
        };

        await collection.InsertManyAsync(new[] { pending, accepted, rejected });

        // Act
        var result = await _repository.GetAllFriendRequests(userId);
        var list = result.ToList();

        // Assert
        Assert.AreEqual(1, list.Count, "Kun pending requests skal returneres");
        Assert.AreEqual(FriendshipStatus.Pending, list.Single().FriendShipStatus);
    }
    
    
    //GetAllFriendRequests
    [TestMethod]
    [DoNotParallelize]
    public async Task GetAllFriendRequests_WhenItsSuccessfull_ShouldReturnAllFriendRequests()
    {
        // Arrange
        var userId = 1;
        var collection = _database.GetCollection<Friendship>("Friendships");

        var shouldBeReturned1 = new Friendship
        {
            SenderId = userId,
            ReceiverId = 2,
            FriendShipStatus = FriendshipStatus.Pending
        };

        var shouldBeReturned2 = new Friendship
        {
            SenderId = userId,
            ReceiverId = 3,
            FriendShipStatus = FriendshipStatus.Pending
        };

        var otherSender = new Friendship
        {
            SenderId = userId,
            ReceiverId = 4,
            FriendShipStatus = FriendshipStatus.Pending
        };

        var otherStatus = new Friendship
        {
            SenderId = userId,
            ReceiverId = 5,
            FriendShipStatus = FriendshipStatus.Pending
        };

        await collection.InsertManyAsync(new[]
        {
            shouldBeReturned1,
            shouldBeReturned2,
            otherSender,
            otherStatus
        });

        // Act
        var result = await _repository.GetAllFriendRequests(userId);
        var list = result.ToList();

        // Assert
        Assert.IsTrue(list.All(f => f.SenderId == userId),
            "Alle resultater skal have samme SenderId som i testen");

        Assert.IsTrue(list.All(f => f.FriendShipStatus == FriendshipStatus.Pending),
            "Alle resultater skal have status Pending");
    }
    
    [TestMethod]
    public async Task GetAllFriendRequests_WhenNoRequestsExist_ShouldReturnEmptyList()
    {
        // Arrange
        var userId = 1;
        var collection = _database.GetCollection<Friendship>("Friendships");
        

        // Act
        var result = await _repository.GetAllFriendRequests(userId);
        var list = result.ToList();

        // Assert
        Assert.AreEqual(0, list.Count, "Der skal ikke returneres nogen requests når der ingen findes");
    }
    
    
    [TestMethod]
    [DoNotParallelize]
    public async Task AcceptFriendRequest_WhenPendingExists_ShouldUpdateStatusToAccepted()
    {
        // Arrange
        var userId = 1;
        var receiverId = 2;

        var collection = _database.GetCollection<Friendship>("Friendships");

        var pending = new Friendship
        {
            SenderId = userId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Pending
        };

        await collection.InsertOneAsync(pending);

        // Act
        var result = await _repository.AcceptFriendRequest(userId, receiverId);

        // Assert
        Assert.IsNotNull(result, "Result må ikke være null");
        Assert.AreEqual(FriendshipStatus.Accepted, result.FriendShipStatus, "Status skal være Accepted efter accept");

        // Tjek også at dokumentet i databasen er opdateret
        var stored = await collection
            .Find(f => f.SenderId == userId && f.ReceiverId == receiverId)
            .SingleOrDefaultAsync();

        Assert.IsNotNull(stored, "Friendship skal stadig eksistere i databasen");
        Assert.AreEqual(FriendshipStatus.Accepted, stored.FriendShipStatus, "Status i databasen skal være Accepted");
    }
    
    [TestMethod]
    [DoNotParallelize]
    public async Task AcceptFriendRequest_WhenNoRequestExists_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var userId = 1;
        var receiverId = 2;

        // Act + Assert
        await Assert.ThrowsExceptionAsync<KeyNotFoundException>(
            async () => await _repository.AcceptFriendRequest(userId, receiverId),
            "Hvis der ikke findes en friendship skal der smides KeyNotFoundException"
        );
    }
    
    [TestMethod]
    [DoNotParallelize]
    public async Task AcceptFriendRequest_WhenStatusIsNotPending_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var userId = 1;
        var receiverId = 2;

        var collection = _database.GetCollection<Friendship>("Friendships");

        var alreadyAccepted = new Friendship
        {
            SenderId = userId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Accepted
        };

        await collection.InsertOneAsync(alreadyAccepted);

        // Act + Assert
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            async () => await _repository.AcceptFriendRequest(userId, receiverId),
            "Hvis status ikke er Pending skal der smides InvalidOperationException"
        );
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task AcceptFriendRequest_WhenStatusIsDeclined_ShouldAlsoThrowInvalidOperationException()
    {
        // Arrange
        var userId = 1;
        var receiverId = 2;

        var collection = _database.GetCollection<Friendship>("Friendships");

        var declined = new Friendship
        {
            SenderId = userId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Declined
        };

        await collection.InsertOneAsync(declined);

        // Act + Assert
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            async () => await _repository.AcceptFriendRequest(userId, receiverId),
            "Det skal ikke være muligt at acceptere en Declined request"
        );
    }
    

}