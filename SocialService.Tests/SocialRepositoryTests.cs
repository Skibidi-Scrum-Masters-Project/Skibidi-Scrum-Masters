using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MongoDB.Driver;
using SocialService.Models;
using SocialService;
using System;
using FitnessApp.Shared.Models;
using SocialService.Controllers;
using SocialService.Repositories;


namespace SocialService.Tests;

[TestClass]
public class SocialRepositoryTests
{
    private Mock<IMongoCollection<Friendship>> _mockCollection = null!;
    private Mock<IMongoDatabase> _mockDatabase = null!;
    private SocialRepository _repository = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockCollection = new Mock<IMongoCollection<Friendship>>();
        _mockDatabase = new Mock<IMongoDatabase>();

        _mockDatabase
            .Setup(db => db.GetCollection<Friendship>(
                "Friendships", 
                It.IsAny<MongoCollectionSettings>()))
            .Returns(_mockCollection.Object);

        _repository = new SocialRepository(_mockDatabase.Object);
    }
    
    
    [TestMethod]
    public void AcceptFriendRequest_ShouldCreateFriendship()
    {
        // TBA: Implement accept friend request test
        Assert.Inconclusive("Test not implemented yet");
    }

    [TestMethod]
    public void GetMutualFriends_ShouldReturnSharedFriends()
    {
        // TBA: Implement mutual friends test
        Assert.Inconclusive("Test not implemented yet");
    }
}