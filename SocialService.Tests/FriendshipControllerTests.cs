using Microsoft.AspNetCore.Mvc;
using FitnessApp.Shared.Models;
using Microsoft.AspNetCore.Http;
using SocialService.Controllers;
using Moq;
using SocialService.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SocialService.Models;

namespace SocialService.Tests;

[TestClass]
public class FriendshipControllerTests
{
    private FriendshipController _controller = null!;
    private Mock<IFriendshipRepository> _mockRepository = null!;
    
    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new Mock<IFriendshipRepository>();
        _controller = new FriendshipController(_mockRepository.Object);
    }
    
    
    [TestMethod]
    public async Task SendFriendRequestAsync_ShouldMakeStatusPending_WhenFriendRequestSent()
    {
        //Arrange
        var senderId = 1;
        var receiverId = 2;
        
        var friendshipInput = new Friendship
        {
            SenderId = senderId,
            ReceiverId = receiverId
        };
        
        var friendshipFromRepo = new Friendship
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Pending
        };
        
        _mockRepository
            .Setup(r => r.SendFriendRequestAsync(senderId, receiverId))
            .ReturnsAsync(friendshipFromRepo);
        
        //Act
        var actionResult = await _controller.SendFriendRequestAsync(friendshipInput);
        
        // Assert
        var okResult = actionResult.Result as OkObjectResult;
        Assert.IsNotNull(okResult, "Expected OkObjectResult");
        Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);

        var status = (FriendshipStatus)okResult.Value!;
        Assert.AreEqual(FriendshipStatus.Pending, status);
    }


    [TestMethod]
    public async Task DeclineFriendRequestAsync_ShouldReturnPositiveIfStatusIsDeclined_WhenFriendRequestDeclined()
    {
        // Arrange
        var senderId = 1;
        var receiverId = 2;

        var friendshipFromRepo = new Friendship
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Declined
        };

        _mockRepository
            .Setup(r => r.DeclineFriendRequestAsync(senderId, receiverId))
            .ReturnsAsync(friendshipFromRepo);
        
        //Act
        var result = await _controller.DeclineFriendRequestAsync(senderId, receiverId);
        
        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult, "Expected OkObjectResult");
        Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
        
        var status = (FriendshipStatus)okResult.Value!;
        Assert.AreEqual(FriendshipStatus.Declined, status);
    }
    
    [TestMethod]
    public async Task GetAllFriends_ShouldReturnListOfAllFriends_WhenSearchedForFriends()
    {
        //Arrange
        var senderId = 1;
        var receiverId = senderId;

        var friendshipFromRepo = new Friendship
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Accepted
        };
        
        _mockRepository
            .Setup(f => f.GetAllFriends(senderId))
            .ReturnsAsync(new List<Friendship> { friendshipFromRepo });
        
        //Act
        var result = await _controller.GetAllFriends(senderId);
        
        
        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult, "Expected OkObjectResult");
        Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);

        // Assert at vi får en liste tilbage
        var friends = okResult.Value as IEnumerable<Friendship>;
        Assert.IsNotNull(friends, "Expected a list of friendships");

        var friendsList = friends.ToList();
        Assert.AreEqual(1, friendsList.Count, "Expected exactly one friendship");

        // Assert at den returnerede friendship har korrekt SenderId og status
        var friend = friendsList[0];
        Assert.AreEqual(senderId, friend.SenderId, "SenderId should match");
        Assert.AreEqual(FriendshipStatus.Accepted, friend.FriendShipStatus, "FriendshipStatus should be Accepted");
        

        // Assert at repository metoden blev kaldt korrekt
        _mockRepository.Verify(f => f.GetAllFriends(senderId), Times.Once);
    }

    [TestMethod]
    public async Task GetFriendById_ShouldReturnFriend_WhenFriendIsFound()
    {
        //Arrange
        var senderId = 1;
        var receiverId = 2;
        
        var friendshipFromRepo = new Friendship
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Accepted
        };
        
        _mockRepository
            .Setup(f => f.GetFriendById(senderId, receiverId))
            .ReturnsAsync(friendshipFromRepo);
        
        
        //Act
        var result =  await _controller.GetFriendById(senderId, receiverId);
        
        //Assert
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult, "Expected OkObjectResult");
        Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
        
        
        var friend = okResult.Value as Friendship;
        Assert.IsNotNull(friend, "Expected a Friendship");
        Assert.AreEqual(senderId, friend.SenderId, "SenderId should match");
        Assert.AreEqual(receiverId, friend.ReceiverId, "ReceiverId should match");
        Assert.AreEqual(FriendshipStatus.Accepted, friend.FriendShipStatus, "FriendshipStatus should be Accepted");
        
        Assert.IsNotNull(result, "Result should not be null");
        
        // Verificer at repoet blev kaldt korrekt
        _mockRepository.Verify(
            f => f.GetFriendById(senderId, receiverId), Times.Once, "GetFriendById should be called exactly once");
    }
    
    [TestMethod]
    public async Task GetFriendById_ShouldReturnNotFound_WhenFriendDoesNotExist()
    {
        // Arrange
        var senderId = 1;
        var receiverId = 2;

        // Repo returnerer null for denne kombination
        _mockRepository
            .Setup(f => f.GetFriendById(senderId, receiverId))
            .ReturnsAsync((Friendship?)null);

        // Act
        var result = await _controller.GetFriendById(senderId, receiverId);

        // Assert
        Assert.IsNotNull(result, "Result should not be null");

        var notFoundResult = result.Result as NotFoundResult;
        Assert.IsNotNull(notFoundResult, "Expected NotFoundResult");
        
        
        Assert.AreEqual(StatusCodes.Status404NotFound, notFoundResult.StatusCode);

        // Verificer at repoet blev kaldt korrekt
        _mockRepository.Verify(
            f => f.GetFriendById(senderId, receiverId), Times.Once, "GetFriendById should be called exactly once");
    }

    
    
    
    [TestMethod]
    public void GetUserFriends_WhenNoFriends_ShouldReturnEmptyList()
    {
        // TBA: Implement test for empty friends list
        Assert.Inconclusive("Test not implemented yet");
    }


}