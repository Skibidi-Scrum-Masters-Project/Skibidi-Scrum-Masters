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
    public void GetUserFriends_ShouldReturnUsersFriends()
    {
        // TBA: Implement test for getting user's friends
        Assert.Inconclusive("Test not implemented yet");
    }

    [TestMethod]
    public void GetUserFriends_WhenNoFriends_ShouldReturnEmptyList()
    {
        // TBA: Implement test for empty friends list
        Assert.Inconclusive("Test not implemented yet");
    }

    [TestMethod]
    public void GetUserFriends_WithValidUserId_ShouldReturnOkResult()
    {
        // TBA: Implement test for OK status code
        Assert.Inconclusive("Test not implemented yet");
    }
}