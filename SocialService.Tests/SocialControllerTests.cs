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
    
    
    //Testing GetAllFriends()
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

    
    //Testing GetFriendById()
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

    
    //Testing CancelFriendRequest
    [TestMethod]
    public async Task CancelFriendRequest_ShouldSetStatusToNone_WhenFriendRequestIsCancelled()
    {
        // Arrange
        var senderId = 1;
        var receiverId = 2;

        var friendshipFromRepo = new Friendship
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Accepted
        };

        _mockRepository
            .Setup(r => r.CancelFriendRequest(senderId, receiverId))
            .ReturnsAsync(() =>
            {
                friendshipFromRepo.FriendShipStatus = FriendshipStatus.None;
                return friendshipFromRepo;
            });

        // Act
        var result = await _controller.CancelFriendRequest(senderId, receiverId);

        
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
             var senderId = 1;
     
             
             var friendshipFromRepo = new Friendship
             {
                 SenderId = senderId,
                 FriendShipStatus = FriendshipStatus.Pending
             };
             
             
             _mockRepository.Setup(r => r.GetAllFriendRequests(senderId))
                 .ReturnsAsync(new List<Friendship> { friendshipFromRepo });
             
             //Act
             var result = await _controller.GetAllFriendRequests(senderId);
             
             
             // Assert
             var okResult = result.Result as OkObjectResult;
             Assert.IsNotNull(okResult, "Expected OkObjectResult");
             Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
     
             
             // Verificer at repositoryet blev kaldt korrekt
             _mockRepository.Verify(r => r.GetAllFriendRequests(senderId), Times.Once);
         }
         
    [TestMethod]
    public async Task GetAllFriendRequests_WhenItsUnSuccesfull_ShouldReturnStatusCode400()
    {
        // Arrange
        var senderId = 1;

        // Simulerer at repo ikke kan hente venneanmodninger (fx fejl eller ugyldig request)
        _mockRepository.Setup(r => r.GetAllFriendRequests(senderId))
            .ReturnsAsync((List<Friendship>?)null);

        // Act
        var result = await _controller.GetAllFriendRequests(senderId);

        // Assert
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult, "Expected BadRequestObjectResult");
        Assert.AreEqual(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);

        // Verificer at repositoryet blev kaldt korrekt
        _mockRepository.Verify(r => r.GetAllFriendRequests(senderId), Times.Once);
    }
}