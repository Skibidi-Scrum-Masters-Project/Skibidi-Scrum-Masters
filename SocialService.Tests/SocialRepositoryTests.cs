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
    public async Task GetAllFriendRequests_WhenThereIsMultipleStatus_ShouldNotReturnAcceptedOrDeclinedRequests()
    {
        // Arrange
        var senderId = 1;
        var collection = _database.GetCollection<Friendship>("Friendships");

        var pending = new Friendship
        {
            SenderId = senderId,
            ReceiverId = 2,
            FriendShipStatus = FriendshipStatus.Pending
        };

        var accepted = new Friendship
        {
            SenderId = senderId,
            ReceiverId = 3,
            FriendShipStatus = FriendshipStatus.Accepted
        };

        var rejected = new Friendship
        {
            SenderId = senderId,
            ReceiverId = 4,
            FriendShipStatus = FriendshipStatus.Declined
        };

        await collection.InsertManyAsync(new[] { pending, accepted, rejected });

        // Act
        var result = await _repository.GetAllFriendRequests(senderId);
        var list = result.ToList();

        // Assert
        Assert.AreEqual(1, list.Count, "Kun pending requests skal returneres");
        Assert.AreEqual(FriendshipStatus.Pending, list.Single().FriendShipStatus);
    }
    
    
    //GetAllFriendRequests
    [TestMethod]
    public async Task GetAllFriendRequests_WhenItsSuccessfull_ShouldReturnAllFriendRequests()
    {
        // Arrange
        var senderId = 1;
        var collection = _database.GetCollection<Friendship>("Friendships");

        var shouldBeReturned1 = new Friendship
        {
            SenderId = senderId,
            ReceiverId = 2,
            FriendShipStatus = FriendshipStatus.Pending
        };

        var shouldBeReturned2 = new Friendship
        {
            SenderId = senderId,
            ReceiverId = 3,
            FriendShipStatus = FriendshipStatus.Pending
        };

        var otherSender = new Friendship
        {
            SenderId = senderId,
            ReceiverId = 4,
            FriendShipStatus = FriendshipStatus.Pending
        };

        var otherStatus = new Friendship
        {
            SenderId = senderId,
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
        var result = await _repository.GetAllFriendRequests(senderId);
        var list = result.ToList();

        // Assert
        Assert.IsTrue(list.All(f => f.SenderId == senderId),
            "Alle resultater skal have samme SenderId som i testen");

        Assert.IsTrue(list.All(f => f.FriendShipStatus == FriendshipStatus.Pending),
            "Alle resultater skal have status Pending");
    }
    
    [TestMethod]
    public async Task GetAllFriendRequests_WhenNoRequestsExist_ShouldReturnEmptyList()
    {
        // Arrange
        var senderId = 1;
        var collection = _database.GetCollection<Friendship>("Friendships");
        

        // Act
        var result = await _repository.GetAllFriendRequests(senderId);
        var list = result.ToList();

        // Assert
        Assert.AreEqual(0, list.Count, "Der skal ikke returneres nogen requests når der ingen findes");
    }
    
    

}