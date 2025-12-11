using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SocialService.Controllers;
using SocialService.Models;
using SocialService.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

    // GetUserFriends

    [TestMethod]
    public void GetUserFriends_ShouldReturnOkWithMessage()
    {
        // Arrange
        var userId = "user-123";

        // Act
        var result = _controller.GetUserFriends(userId);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult, "Expected OkObjectResult");
        Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.IsNotNull(okResult.Value, "Expected a message payload");
    }

    // SendFriendRequestAsync

    [TestMethod]
    public async Task SendFriendRequestAsync_ShouldReturnOkWithFriendship_WhenSuccessful()
    {
        // Arrange
        var userId = 1;
        var receiverId = 2;

        var friendshipFromRepo = new Friendship
        {
            SenderId = userId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Pending
        };

        _mockRepository
            .Setup(r => r.SendFriendRequestAsync(userId, receiverId))
            .ReturnsAsync(friendshipFromRepo);

        // Act
        var result = await _controller.SendFriendRequestAsync(userId, receiverId);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult, "Expected OkObjectResult");
        Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);

        var friendship = okResult.Value as Friendship;
        Assert.IsNotNull(friendship, "Expected Friendship as value");
        Assert.AreEqual(FriendshipStatus.Pending, friendship.FriendShipStatus);
        Assert.AreEqual(userId, friendship.SenderId);
        Assert.AreEqual(receiverId, friendship.ReceiverId);
    }

    [TestMethod]
    public async Task SendFriendRequestAsync_ShouldReturnBadRequest_WhenUserIdEqualsReceiverId()
    {
        // Arrange
        var userId = 1;
        var receiverId = 1;

        // Act
        var result = await _controller.SendFriendRequestAsync(userId, receiverId);

        // Assert
        var badRequest = result.Result as BadRequestObjectResult;
        Assert.IsNotNull(badRequest, "Expected BadRequestObjectResult");
        Assert.AreEqual(StatusCodes.Status400BadRequest, badRequest.StatusCode);
        _mockRepository.Verify(r => r.SendFriendRequestAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [TestMethod]
    public async Task SendFriendRequestAsync_ShouldReturnBadRequest_WhenIdsAreInvalid()
    {
        // Arrange
        var userId = 0;
        var receiverId = -5;

        // Act
        var result = await _controller.SendFriendRequestAsync(userId, receiverId);

        // Assert
        var badRequest = result.Result as BadRequestObjectResult;
        Assert.IsNotNull(badRequest, "Expected BadRequestObjectResult");
        Assert.AreEqual(StatusCodes.Status400BadRequest, badRequest.StatusCode);
        _mockRepository.Verify(r => r.SendFriendRequestAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [TestMethod]
    public async Task SendFriendRequestAsync_ShouldReturnBadRequest_WhenRepositoryThrowsArgumentException()
    {
        // Arrange
        var userId = 1;
        var receiverId = 2;

        _mockRepository
            .Setup(r => r.SendFriendRequestAsync(userId, receiverId))
            .ThrowsAsync(new ArgumentException("Invalid"));

        // Act
        var result = await _controller.SendFriendRequestAsync(userId, receiverId);

        // Assert
        var badRequest = result.Result as BadRequestObjectResult;
        Assert.IsNotNull(badRequest, "Expected BadRequestObjectResult");
        Assert.AreEqual(StatusCodes.Status400BadRequest, badRequest.StatusCode);
    }

    [TestMethod]
    public async Task SendFriendRequestAsync_ShouldReturnConflict_WhenRepositoryThrowsInvalidOperationException()
    {
        // Arrange
        var userId = 1;
        var receiverId = 2;

        _mockRepository
            .Setup(r => r.SendFriendRequestAsync(userId, receiverId))
            .ThrowsAsync(new InvalidOperationException("Friendship already exists"));

        // Act
        var result = await _controller.SendFriendRequestAsync(userId, receiverId);

        // Assert
        var conflict = result.Result as ConflictObjectResult;
        Assert.IsNotNull(conflict, "Expected ConflictObjectResult");
        Assert.AreEqual(StatusCodes.Status409Conflict, conflict.StatusCode);
    }

    [TestMethod]
    public async Task SendFriendRequestAsync_ShouldReturnNotFound_WhenRepositoryThrowsKeyNotFoundException()
    {
        // Arrange
        var userId = 1;
        var receiverId = 2;

        _mockRepository
            .Setup(r => r.SendFriendRequestAsync(userId, receiverId))
            .ThrowsAsync(new KeyNotFoundException("User not found"));

        // Act
        var result = await _controller.SendFriendRequestAsync(userId, receiverId);

        // Assert
        var notFound = result.Result as NotFoundObjectResult;
        Assert.IsNotNull(notFound, "Expected NotFoundObjectResult");
        Assert.AreEqual(StatusCodes.Status404NotFound, notFound.StatusCode);
    }

    [TestMethod]
    public async Task SendFriendRequestAsync_ShouldReturn500_WhenRepositoryThrowsUnexpectedException()
    {
        // Arrange
        var userId = 1;
        var receiverId = 2;

        _mockRepository
            .Setup(r => r.SendFriendRequestAsync(userId, receiverId))
            .ThrowsAsync(new Exception("Unexpected"));

        // Act
        var result = await _controller.SendFriendRequestAsync(userId, receiverId);

        // Assert
        var statusResult = result.Result as ObjectResult;
        Assert.IsNotNull(statusResult, "Expected ObjectResult");
        Assert.AreEqual(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
    }

    // DeclineFriendRequestAsync

    [TestMethod]
    public async Task DeclineFriendRequestAsync_ShouldReturnOkWithStatusDeclined_WhenSuccessful()
    {
        // Arrange
        var userId = 1;
        var receiverId = 2;

        var friendshipFromRepo = new Friendship
        {
            SenderId = userId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Declined
        };

        _mockRepository
            .Setup(r => r.DeclineFriendRequestAsync(userId, receiverId))
            .ReturnsAsync(friendshipFromRepo);

        // Act
        var result = await _controller.DeclineFriendRequestAsync(userId, receiverId);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult, "Expected OkObjectResult");
        Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);

        var status = (FriendshipStatus)okResult.Value!;
        Assert.AreEqual(FriendshipStatus.Declined, status);
    }

    [TestMethod]
    public async Task DeclineFriendRequestAsync_ShouldReturnBadRequest_WhenUserIdEqualsReceiverId()
    {
        // Arrange
        var userId = 1;
        var receiverId = 1;

        // Act
        var result = await _controller.DeclineFriendRequestAsync(userId, receiverId);

        // Assert
        var badRequest = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequest, "Expected BadRequestObjectResult");
        Assert.AreEqual(StatusCodes.Status400BadRequest, badRequest.StatusCode);
        _mockRepository.Verify(r => r.DeclineFriendRequestAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [TestMethod]
    public async Task DeclineFriendRequestAsync_ShouldReturnBadRequest_WhenIdsAreInvalid()
    {
        // Arrange
        var userId = -1;
        var receiverId = 0;

        // Act
        var result = await _controller.DeclineFriendRequestAsync(userId, receiverId);

        // Assert
        var badRequest = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequest, "Expected BadRequestObjectResult");
        Assert.AreEqual(StatusCodes.Status400BadRequest, badRequest.StatusCode);
        _mockRepository.Verify(r => r.DeclineFriendRequestAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [TestMethod]
    public async Task DeclineFriendRequestAsync_ShouldReturnNotFound_WhenRepositoryThrowsKeyNotFoundException()
    {
        // Arrange
        var userId = 1;
        var receiverId = 2;

        _mockRepository
            .Setup(r => r.DeclineFriendRequestAsync(userId, receiverId))
            .ThrowsAsync(new KeyNotFoundException("Not found"));

        // Act
        var result = await _controller.DeclineFriendRequestAsync(userId, receiverId);

        // Assert
        var notFound = result as NotFoundObjectResult;
        Assert.IsNotNull(notFound, "Expected NotFoundObjectResult");
        Assert.AreEqual(StatusCodes.Status404NotFound, notFound.StatusCode);
    }

    [TestMethod]
    public async Task DeclineFriendRequestAsync_ShouldReturnConflict_WhenRepositoryThrowsInvalidOperationException()
    {
        // Arrange
        var userId = 1;
        var receiverId = 2;

        _mockRepository
            .Setup(r => r.DeclineFriendRequestAsync(userId, receiverId))
            .ThrowsAsync(new InvalidOperationException("Cannot decline"));

        // Act
        var result = await _controller.DeclineFriendRequestAsync(userId, receiverId);

        // Assert
        var conflict = result as ConflictObjectResult;
        Assert.IsNotNull(conflict, "Expected ConflictObjectResult");
        Assert.AreEqual(StatusCodes.Status409Conflict, conflict.StatusCode);
    }

    [TestMethod]
    public async Task DeclineFriendRequestAsync_ShouldReturnBadRequest_WhenRepositoryThrowsArgumentException()
    {
        // Arrange
        var userId = 1;
        var receiverId = 2;

        _mockRepository
            .Setup(r => r.DeclineFriendRequestAsync(userId, receiverId))
            .ThrowsAsync(new ArgumentException("Invalid"));

        // Act
        var result = await _controller.DeclineFriendRequestAsync(userId, receiverId);

        // Assert
        var badRequest = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequest, "Expected BadRequestObjectResult");
        Assert.AreEqual(StatusCodes.Status400BadRequest, badRequest.StatusCode);
    }

    [TestMethod]
    public async Task DeclineFriendRequestAsync_ShouldReturn500_WhenRepositoryThrowsUnexpectedException()
    {
        // Arrange
        var userId = 1;
        var receiverId = 2;

        _mockRepository
            .Setup(r => r.DeclineFriendRequestAsync(userId, receiverId))
            .ThrowsAsync(new Exception("Unexpected"));

        // Act
        var result = await _controller.DeclineFriendRequestAsync(userId, receiverId);

        // Assert
        var statusResult = result as ObjectResult;
        Assert.IsNotNull(statusResult, "Expected ObjectResult");
        Assert.AreEqual(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
    }

    // GetAllFriends

    [TestMethod]
    public async Task GetAllFriends_ShouldReturnOkWithFriends_WhenSuccessful()
    {
        // Arrange
        var userId = 1;

        var friendship = new Friendship
        {
            SenderId = userId,
            ReceiverId = 2,
            FriendShipStatus = FriendshipStatus.Accepted
        };

        _mockRepository
            .Setup(r => r.GetAllFriends(userId))
            .ReturnsAsync(new List<Friendship> { friendship });

        // Act
        var result = await _controller.GetAllFriends(userId);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult, "Expected OkObjectResult");
        Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);

        var list = okResult.Value as IEnumerable<Friendship>;
        Assert.IsNotNull(list, "Expected list of friendships");
        Assert.AreEqual(1, list.Count());
    }

    [TestMethod]
    public async Task GetAllFriends_ShouldReturnNotFound_WhenRepositoryThrowsKeyNotFoundException()
    {
        // Arrange
        var userId = 1;

        _mockRepository
            .Setup(r => r.GetAllFriends(userId))
            .ThrowsAsync(new KeyNotFoundException("Not found"));

        // Act
        var result = await _controller.GetAllFriends(userId);

        // Assert
        var notFound = result.Result as NotFoundObjectResult;
        Assert.IsNotNull(notFound, "Expected NotFoundObjectResult");
        Assert.AreEqual(StatusCodes.Status404NotFound, notFound.StatusCode);
    }

    [TestMethod]
    public async Task GetAllFriends_ShouldReturnConflict_WhenRepositoryThrowsInvalidOperationException()
    {
        // Arrange
        var userId = 1;

        _mockRepository
            .Setup(r => r.GetAllFriends(userId))
            .ThrowsAsync(new InvalidOperationException("Conflict"));

        // Act
        var result = await _controller.GetAllFriends(userId);

        // Assert
        var conflict = result.Result as ConflictObjectResult;
        Assert.IsNotNull(conflict, "Expected ConflictObjectResult");
        Assert.AreEqual(StatusCodes.Status409Conflict, conflict.StatusCode);
    }

    [TestMethod]
    public async Task GetAllFriends_ShouldReturnBadRequest_WhenRepositoryThrowsArgumentException()
    {
        // Arrange
        var userId = 1;

        _mockRepository
            .Setup(r => r.GetAllFriends(userId))
            .ThrowsAsync(new ArgumentException("Invalid"));

        // Act
        var result = await _controller.GetAllFriends(userId);

        // Assert
        var badRequest = result.Result as BadRequestObjectResult;
        Assert.IsNotNull(badRequest, "Expected BadRequestObjectResult");
        Assert.AreEqual(StatusCodes.Status400BadRequest, badRequest.StatusCode);
    }

    [TestMethod]
    public async Task GetAllFriends_ShouldReturn500_WhenRepositoryThrowsUnexpectedException()
    {
        // Arrange
        var userId = 1;

        _mockRepository
            .Setup(r => r.GetAllFriends(userId))
            .ThrowsAsync(new Exception("Unexpected"));

        // Act
        var result = await _controller.GetAllFriends(userId);

        // Assert
        var statusResult = result.Result as ObjectResult;
        Assert.IsNotNull(statusResult, "Expected ObjectResult");
        Assert.AreEqual(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
    }

    // GetFriendById

    [TestMethod]
    public async Task GetFriendById_ShouldReturnOkWithFriend_WhenFound()
    {
        // Arrange
        var userId = 1;
        var receiverId = 2;

        var friendship = new Friendship
        {
            SenderId = userId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Accepted
        };

        _mockRepository
            .Setup(r => r.GetFriendById(userId, receiverId))
            .ReturnsAsync(friendship);

        // Act
        var result = await _controller.GetFriendById(userId, receiverId);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult, "Expected OkObjectResult");
        Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);

        var friend = okResult.Value as Friendship;
        Assert.IsNotNull(friend);
        Assert.AreEqual(userId, friend.SenderId);
        Assert.AreEqual(receiverId, friend.ReceiverId);
    }

    [TestMethod]
    public async Task GetFriendById_ShouldReturnNotFound_WhenRepoReturnsNull()
    {
        // Arrange
        var userId = 1;
        var receiverId = 2;

        _mockRepository
            .Setup(r => r.GetFriendById(userId, receiverId))
            .ReturnsAsync((Friendship?)null);

        // Act
        var result = await _controller.GetFriendById(userId, receiverId);

        // Assert
        var notFound = result.Result as NotFoundResult;
        Assert.IsNotNull(notFound, "Expected NotFoundResult");
        Assert.AreEqual(StatusCodes.Status404NotFound, notFound.StatusCode);
    }

    [TestMethod]
    public async Task GetFriendById_ShouldReturnNotFound_WhenRepositoryThrowsKeyNotFoundException()
    {
        // Arrange
        var userId = 1;
        var receiverId = 2;

        _mockRepository
            .Setup(r => r.GetFriendById(userId, receiverId))
            .ThrowsAsync(new KeyNotFoundException("Not found"));

        // Act
        var result = await _controller.GetFriendById(userId, receiverId);

        // Assert
        var notFound = result.Result as NotFoundObjectResult;
        Assert.IsNotNull(notFound, "Expected NotFoundObjectResult");
        Assert.AreEqual(StatusCodes.Status404NotFound, notFound.StatusCode);
    }

    [TestMethod]
    public async Task GetFriendById_ShouldReturnConflict_WhenRepositoryThrowsInvalidOperationException()
    {
        // Arrange
        var userId = 1;
        var receiverId = 2;

        _mockRepository
            .Setup(r => r.GetFriendById(userId, receiverId))
            .ThrowsAsync(new InvalidOperationException("Duplicate"));

        // Act
        var result = await _controller.GetFriendById(userId, receiverId);

        // Assert
        var conflict = result.Result as ConflictObjectResult;
        Assert.IsNotNull(conflict, "Expected ConflictObjectResult");
        Assert.AreEqual(StatusCodes.Status409Conflict, conflict.StatusCode);
    }

    [TestMethod]
    public async Task GetFriendById_ShouldReturnBadRequest_WhenRepositoryThrowsArgumentException()
    {
        // Arrange
        var userId = 1;
        var receiverId = 2;

        _mockRepository
            .Setup(r => r.GetFriendById(userId, receiverId))
            .ThrowsAsync(new ArgumentException("Invalid"));

        // Act
        var result = await _controller.GetFriendById(userId, receiverId);

        // Assert
        var badRequest = result.Result as BadRequestObjectResult;
        Assert.IsNotNull(badRequest, "Expected BadRequestObjectResult");
        Assert.AreEqual(StatusCodes.Status400BadRequest, badRequest.StatusCode);
    }

    [TestMethod]
    public async Task GetFriendById_ShouldReturn500_WhenRepositoryThrowsUnexpectedException()
    {
        // Arrange
        var userId = 1;
        var receiverId = 2;

        _mockRepository
            .Setup(r => r.GetFriendById(userId, receiverId))
            .ThrowsAsync(new Exception("Unexpected"));

        // Act
        var result = await _controller.GetFriendById(userId, receiverId);

        // Assert
        var statusResult = result.Result as ObjectResult;
        Assert.IsNotNull(statusResult, "Expected ObjectResult");
        Assert.AreEqual(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
    }

    // CancelFriendRequest

    [TestMethod]
    public async Task CancelFriendRequest_ShouldReturnOkWithFriendship_WhenSuccessful()
    {
        // Arrange
        var userId = 1;
        var receiverId = 2;

        var friendship = new Friendship
        {
            SenderId = userId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.None
        };

        _mockRepository
            .Setup(r => r.CancelFriendRequest(userId, receiverId))
            .ReturnsAsync(friendship);

        // Act
        var result = await _controller.CancelFriendRequest(userId, receiverId);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult, "Expected OkObjectResult");
        Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);

        var resultFriend = okResult.Value as Friendship;
        Assert.IsNotNull(resultFriend);
        Assert.AreEqual(FriendshipStatus.None, resultFriend.FriendShipStatus);
    }

    [TestMethod]
    public async Task CancelFriendRequest_ShouldBubbleException_WhenRepositoryThrows()
    {
        // Arrange
        var userId = 1;
        var receiverId = 2;

        _mockRepository
            .Setup(r => r.CancelFriendRequest(userId, receiverId))
            .ThrowsAsync(new InvalidOperationException("No pending request"));

        // Act + Assert
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            async () => await _controller.CancelFriendRequest(userId, receiverId));
    }

    // Testing GetAllFriendRequests (OUTGOING)
    // BEHOLDER DINE TDD TESTS UDEN ÆNDRING

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
             
             
             _mockRepository.Setup(r => r.GetOutgoingFriendRequestsAsync(userId))
                 .ReturnsAsync(new List<Friendship> { friendshipFromRepo });
             
             //Act
             var result = await _controller.GetOutgoingFriendRequests(userId);
             
             
             // Assert
             var okResult = result.Result as OkObjectResult;
             Assert.IsNotNull(okResult, "Expected OkObjectResult");
             Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
     
             
             // Verificer at repositoryet blev kaldt korrekt
             _mockRepository.Verify(r => r.GetOutgoingFriendRequestsAsync(userId), Times.Once);
         }
         

    [TestMethod]
    public async Task GetAllFriendRequests_WhenItsUnSuccesfull_ShouldReturnStatusCode400()
    {
        // Arrange
        var userId = 123; // eller en hvilken som helst int

        _mockRepository
            .Setup(r => r.GetOutgoingFriendRequestsAsync(userId))
            .ReturnsAsync((List<Friendship>?)null);

        // Act
        var result = await _controller.GetOutgoingFriendRequests(userId);

        // Assert
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult, "Expected BadRequestObjectResult");
        Assert.AreEqual(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);

        _mockRepository.Verify(r => r.GetOutgoingFriendRequestsAsync(userId), Times.Once);
    }

    // GetAllIncomingFriendRequests

    [TestMethod]
    public async Task GetAllIncomingFriendRequests_ShouldReturnOkWithList_WhenNotNull()
    {
        // Arrange
        var userId = 1;
        var friendship = new Friendship
        {
            SenderId = 2,
            ReceiverId = userId,
            FriendShipStatus = FriendshipStatus.Pending
        };

        _mockRepository
            .Setup(r => r.GetAllIncomingFriendRequests(userId))
            .ReturnsAsync(new List<Friendship> { friendship });

        // Act
        var result = await _controller.GetAllIncomingFriendRequests(userId);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult, "Expected OkObjectResult");
        Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);

        var list = okResult.Value as IEnumerable<Friendship>;
        Assert.IsNotNull(list);
        Assert.AreEqual(1, list.Count());
    }

    [TestMethod]
    public async Task GetAllIncomingFriendRequests_ShouldReturnBadRequest_WhenRepositoryReturnsNull()
    {
        // Arrange
        var userId = 1;

        _mockRepository
            .Setup(r => r.GetAllIncomingFriendRequests(userId))
            .ReturnsAsync((IEnumerable<Friendship>?)null);

        // Act
        var result = await _controller.GetAllIncomingFriendRequests(userId);

        // Assert
        var badRequest = result.Result as BadRequestObjectResult;
        Assert.IsNotNull(badRequest, "Expected BadRequestObjectResult");
        Assert.AreEqual(StatusCodes.Status400BadRequest, badRequest.StatusCode);
    }

    // AcceptFriendRequestAsync

    [TestMethod]
    public async Task AcceptFriendRequestAsync_ShouldReturnOkWithStatusAccepted_WhenSuccessful()
    {
        // Arrange
        var userId = 1;
        var receiverId = 2;

        _mockRepository
            .Setup(r => r.AcceptFriendRequest(userId, receiverId))
            .ReturnsAsync(new Friendship
            {
                SenderId = userId,
                ReceiverId = receiverId,
                FriendShipStatus = FriendshipStatus.Accepted
            });

        // Act
        var result = await _controller.AcceptFriendRequestAsync(userId, receiverId);

        // Assert
        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult, "Expected OkObjectResult");
        Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);

        var status = (FriendshipStatus)okResult.Value!;
        Assert.AreEqual(FriendshipStatus.Accepted, status);
    }

    [TestMethod]
    public async Task AcceptFriendRequestAsync_ShouldReturnBadRequest_WhenUserIdEqualsReceiverId()
    {
        // Arrange
        var userId = 1;
        var receiverId = 1;

        // Act
        var result = await _controller.AcceptFriendRequestAsync(userId, receiverId);

        // Assert
        var badRequest = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequest, "Expected BadRequestObjectResult");
        Assert.AreEqual(StatusCodes.Status400BadRequest, badRequest.StatusCode);
        _mockRepository.Verify(r => r.AcceptFriendRequest(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [TestMethod]
    public async Task AcceptFriendRequestAsync_ShouldReturnBadRequest_WhenIdsAreInvalid()
    {
        // Arrange
        var userId = 0;
        var receiverId = -1;

        // Act
        var result = await _controller.AcceptFriendRequestAsync(userId, receiverId);

        // Assert
        var badRequest = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequest, "Expected BadRequestObjectResult");
        Assert.AreEqual(StatusCodes.Status400BadRequest, badRequest.StatusCode);
        _mockRepository.Verify(r => r.AcceptFriendRequest(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [TestMethod]
    public async Task AcceptFriendRequestAsync_ShouldReturnNotFound_WhenRepositoryThrowsKeyNotFoundException()
    {
        // Arrange
        var userId = 1;
        var receiverId = 2;

        _mockRepository
            .Setup(r => r.AcceptFriendRequest(userId, receiverId))
            .ThrowsAsync(new KeyNotFoundException("Not found"));

        // Act
        var result = await _controller.AcceptFriendRequestAsync(userId, receiverId);

        // Assert
        var notFound = result as NotFoundObjectResult;
        Assert.IsNotNull(notFound, "Expected NotFoundObjectResult");
        Assert.AreEqual(StatusCodes.Status404NotFound, notFound.StatusCode);
    }

    [TestMethod]
    public async Task AcceptFriendRequestAsync_ShouldReturnConflict_WhenRepositoryThrowsInvalidOperationException()
    {
        // Arrange
        var userId = 1;
        var receiverId = 2;

        _mockRepository
            .Setup(r => r.AcceptFriendRequest(userId, receiverId))
            .ThrowsAsync(new InvalidOperationException("Cannot accept"));

        // Act
        var result = await _controller.AcceptFriendRequestAsync(userId, receiverId);

        // Assert
        var conflict = result as ConflictObjectResult;
        Assert.IsNotNull(conflict, "Expected ConflictObjectResult");
        Assert.AreEqual(StatusCodes.Status409Conflict, conflict.StatusCode);
    }

    [TestMethod]
    public async Task AcceptFriendRequestAsync_ShouldReturnBadRequest_WhenRepositoryThrowsArgumentException()
    {
        // Arrange
        var userId = 1;
        var receiverId = 2;

        _mockRepository
            .Setup(r => r.AcceptFriendRequest(userId, receiverId))
            .ThrowsAsync(new ArgumentException("Invalid"));

        // Act
        var result = await _controller.AcceptFriendRequestAsync(userId, receiverId);

        // Assert
        var badRequest = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequest, "Expected BadRequestObjectResult");
        Assert.AreEqual(StatusCodes.Status400BadRequest, badRequest.StatusCode);
    }

    [TestMethod]
    public async Task AcceptFriendRequestAsync_ShouldReturn500_WhenRepositoryThrowsUnexpectedException()
    {
        // Arrange
        var userId = 1;
        var receiverId = 2;

        _mockRepository
            .Setup(r => r.AcceptFriendRequest(userId, receiverId))
            .ThrowsAsync(new Exception("Unexpected"));

        // Act
        var result = await _controller.AcceptFriendRequestAsync(userId, receiverId);

        // Assert
        var statusResult = result as ObjectResult;
        Assert.IsNotNull(statusResult, "Expected ObjectResult");
        Assert.AreEqual(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
    }
    
    
    
    
    
    //Post tests
    [TestMethod]
    public async Task PostAPost_calls_repository_and_returns_created_post()
    {
        // Arrange
        var repoMock = new Mock<ISocialRepository>();

        var inputPost = new Post
        {
            UserId = 1,
            FitnessClassId = 10,
            WorkoutId = 100,
            PostTitle = "Test title",
            PostContent = "Test content"
        };

        var createdPost = new Post
        {
            Id = "some-mongo-id",
            UserId = inputPost.UserId,
            FitnessClassId = inputPost.FitnessClassId,
            WorkoutId = inputPost.WorkoutId,
            PostTitle = inputPost.PostTitle,
            PostContent = inputPost.PostContent
        };

        repoMock
            .Setup(r => r.PostAPost(inputPost))
            .ReturnsAsync(createdPost);

        var controller = new SocialController(repoMock.Object);

        // Act
        var result = await controller.PostAPost(inputPost);

        // Assert
        Assert.AreSame(createdPost, result);
        repoMock.Verify(r => r.PostAPost(inputPost), Times.Once);
    }
    
    
    
    // RemoveAPost

    [TestMethod]
    public async Task RemoveAPost_calls_repository_and_returns_removed_post()
    {
        // Arrange
        var postId = "some-mongo-id";

        var removedPost = new Post
        {
            Id = postId,
            UserId = 1,
            FitnessClassId = 10,
            WorkoutId = 100,
            PostTitle = "Removed title",
            PostContent = "Removed content"
        };

        _mockRepository
            .Setup(r => r.RemoveAPost(postId))
            .ReturnsAsync(removedPost);

        // Act
        var result = await _controller.RemoveAPost(postId);

        // Assert
        Assert.IsNotNull(result, "Expected a Post to be returned");
        Assert.AreSame(removedPost, result, "Controller should return the Post from the repository");

        _mockRepository.Verify(r => r.RemoveAPost(postId), Times.Once);
    }

}