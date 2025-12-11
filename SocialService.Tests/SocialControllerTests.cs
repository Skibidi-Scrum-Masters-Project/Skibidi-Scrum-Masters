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
public class SocialControllerTests
{
    private SocialController _controller = null!;
    private Mock<ISocialRepository> _mockRepository = null!;
    
    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new Mock<ISocialRepository>();
        _controller = new SocialController(_mockRepository.Object);
    }
    
    //Testing SendFriendRequest()
    [TestMethod]
    public async Task SendFriendRequestAsync_ShouldMakeStatusPending_WhenFriendRequestSent()
    {
        //Arrange
        var userId = 1;
        var receiverId = 2;
        
        var friendshipInput = new Friendship
        {
            SenderId = userId,
            ReceiverId = receiverId
        };
        
        var friendshipFromRepo = new Friendship
        {
            SenderId = userId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Pending
        };
        
        _mockRepository
            .Setup(r => r.SendFriendRequestAsync(userId, receiverId))
            .ReturnsAsync(friendshipFromRepo);
        
        //Act
        var actionResult = await _controller.SendFriendRequestAsync(friendshipInput);
        
        // Assert
        var okResult = actionResult.Result as OkObjectResult;
        Assert.IsNotNull(okResult, "Expected OkObjectResult");
        Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);

      
        var friendship = okResult.Value as Friendship;
        Assert.IsNotNull(friendship, "Expected Value to be Friendship");
        
        // Tjek at status er Pending
        Assert.AreEqual(FriendshipStatus.Pending, friendship.FriendShipStatus);
    }
    
    //Testing DeclineFriendRequest()
    [TestMethod]
    public async Task DeclineFriendRequestAsync_ShouldReturnPositiveIfStatusIsDeclined_WhenFriendRequestDeclined()
    {
        // Arrange
        var userId = 1;
        var receiverId = 2;

        var friendshipFromRepo = new Friendship
        {
            SenderId =  userId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Declined
        };

        _mockRepository
            .Setup(r => r.DeclineFriendRequestAsync(userId, receiverId))
            .ReturnsAsync(friendshipFromRepo);
        
        //Act
        var result = await _controller.DeclineFriendRequestAsync(userId, receiverId);
        
        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult, "Expected OkObjectResult");
        Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
        
        var status = (FriendshipStatus)okResult.Value!;
        Assert.AreEqual(FriendshipStatus.Declined, status);
    }
    
    
    //Testing GetAllFriends()
    [TestMethod]
    public async Task GetAllFriends_ShouldReturnListOfAllFriends_WhenSearchedForFriends()
    {
        //Arrange
        var userId = 1;
        var receiverId = userId;

        var friendshipFromRepo = new Friendship
        {
            SenderId = userId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Accepted
        };
        
        _mockRepository
            .Setup(f => f.GetAllFriends(userId))
            .ReturnsAsync(new List<Friendship> { friendshipFromRepo });
        
        //Act
        var result = await _controller.GetAllFriends(userId);
        
        
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
        Assert.AreEqual(userId, friend.SenderId, "SenderId should match");
        Assert.AreEqual(FriendshipStatus.Accepted, friend.FriendShipStatus, "FriendshipStatus should be Accepted");
        

        // Assert at repository metoden blev kaldt korrekt
        _mockRepository.Verify(f => f.GetAllFriends(userId), Times.Once);
    }

    
    //Testing GetFriendById()
    [TestMethod]
    public async Task GetFriendById_ShouldReturnFriend_WhenFriendIsFound()
    {
        //Arrange
        var userId = 1;
        var receiverId = 2;
        
        var friendshipFromRepo = new Friendship
        {
            SenderId = userId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Accepted
        };
        
        _mockRepository
            .Setup(f => f.GetFriendById(userId, receiverId))
            .ReturnsAsync(friendshipFromRepo);
        
        
        //Act
        var result =  await _controller.GetFriendById(userId, receiverId);
        
        //Assert
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult, "Expected OkObjectResult");
        Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
        
        
        var friend = okResult.Value as Friendship;
        Assert.IsNotNull(friend, "Expected a Friendship");
        Assert.AreEqual(userId, friend.SenderId, "SenderId should match");
        Assert.AreEqual(receiverId, friend.ReceiverId, "ReceiverId should match");
        Assert.AreEqual(FriendshipStatus.Accepted, friend.FriendShipStatus, "FriendshipStatus should be Accepted");
        
        Assert.IsNotNull(result, "Result should not be null");
        
        // Verificer at repoet blev kaldt korrekt
        _mockRepository.Verify(
            f => f.GetFriendById(userId, receiverId), Times.Once, "GetFriendById should be called exactly once");
    }
    
    [TestMethod]
    public async Task GetFriendById_ShouldReturnNotFound_WhenFriendDoesNotExist()
    {
        // Arrange
        var userId = 1;
        var receiverId = 2;

        // Repo returnerer null for denne kombination
        _mockRepository
            .Setup(f => f.GetFriendById(userId, receiverId))
            .ReturnsAsync((Friendship?)null);

        // Act
        var result = await _controller.GetFriendById(userId, receiverId);

        // Assert
        Assert.IsNotNull(result, "Result should not be null");

        var notFoundResult = result.Result as NotFoundResult;
        Assert.IsNotNull(notFoundResult, "Expected NotFoundResult");
        
        
        Assert.AreEqual(StatusCodes.Status404NotFound, notFoundResult.StatusCode);

        // Verificer at repoet blev kaldt korrekt
        _mockRepository.Verify(
            f => f.GetFriendById(userId, receiverId), Times.Once, "GetFriendById should be called exactly once");
    }

    
    //Testing CancelFriendRequest
    [TestMethod]
    public async Task CancelFriendRequest_ShouldSetStatusToNone_WhenFriendRequestIsCancelled()
    {
        // Arrange
        var userId = 1;
        var receiverId = 2;

        var friendshipFromRepo = new Friendship
        {
            SenderId = userId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Accepted
        };

        _mockRepository
            .Setup(r => r.CancelFriendRequest(userId, receiverId))
            .ReturnsAsync(() =>
            {
                friendshipFromRepo.FriendShipStatus = FriendshipStatus.None;
                return friendshipFromRepo;
            });

        // Act
        var result = await _controller.CancelFriendRequest(userId, receiverId);

        
        // Assert
        Assert.IsNotNull(result, "Result should not be null");

        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult, "Result should be OkObjectResult");

        var friend = okResult.Value as Friendship;
        Assert.IsNotNull(friend, "Returned value should be a Friendship");

        Assert.AreEqual(
            FriendshipStatus.None,
            friend.FriendShipStatus,
            "FriendshipStatus should be None after cancelling the request"
        );
    }
    
    
    //Testing GetAllFriendRequests
    [TestMethod]
         public async Task GetAllFriendRequests_WhenItsSuccefull_ShouldReturnStatusCode200()
         {
             //Arrange
             var userId = 1;
     
             
             var friendshipFromRepo = new Friendship
             {
                 SenderId = userId,
                 FriendShipStatus = FriendshipStatus.Pending
             };
             
             
             _mockRepository.Setup(r => r.GetAllFriendRequests(userId))
                 .ReturnsAsync(new List<Friendship> { friendshipFromRepo });
             
             //Act
             var result = await _controller.GetAllFriendRequests(userId);
             
             
             // Assert
             var okResult = result.Result as OkObjectResult;
             Assert.IsNotNull(okResult, "Expected OkObjectResult");
             Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
     
             
             // Verificer at repositoryet blev kaldt korrekt
             _mockRepository.Verify(r => r.GetAllFriendRequests(userId), Times.Once);
         }
         

    [TestMethod]
    public async Task GetAllFriendRequests_WhenItsUnSuccesfull_ShouldReturnStatusCode400()
    {
        // Arrange
        var userId = 123; // eller en hvilken som helst int

        _mockRepository
            .Setup(r => r.GetAllFriendRequests(userId))
            .ReturnsAsync((List<Friendship>?)null);

        // Act
        var result = await _controller.GetAllFriendRequests(userId);

        // Assert
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult, "Expected BadRequestObjectResult");
        Assert.AreEqual(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);

        _mockRepository.Verify(r => r.GetAllFriendRequests(userId), Times.Once);
    }

    [TestMethod]
    public async Task AcceptFriendRequestAsync_ReturnsBadRequest_WhenUserIdEqualsReceiverId()
    {
        // Arrange
        int userId = 1;
        int receiverId = 1;

        // Act
        var result = await _controller.AcceptFriendRequestAsync(userId, receiverId);

        // Assert
        var badRequestResult = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult, "Expected BadRequestObjectResult");
    }
    
    [TestMethod]
    public async Task AcceptFriendRequestAsync_ReturnsOk_WithFriendShipStatus_OnSuccess()
    {
        // Arrange
        int userId = 1;
        int receiverId = 2;
        var expectedStatus = "Accepted";

        _mockRepository
            .Setup(r => r.AcceptFriendRequest(userId, receiverId))
            .ReturnsAsync(new Friendship
            {
                FriendShipStatus = FriendshipStatus.Accepted
            });

        // Act
        var result = await _controller.AcceptFriendRequestAsync(userId, receiverId);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult, "Result should be OkObjectResult");
        Assert.AreEqual(200, okResult.StatusCode ?? 200);
    }

    
}