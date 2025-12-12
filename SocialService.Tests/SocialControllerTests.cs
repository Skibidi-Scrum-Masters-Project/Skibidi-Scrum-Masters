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
using System.Security.Claims;
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

    private void SetUserOnController(string? userIdClaim)
    {
        var claims = new List<Claim>();

        if (userIdClaim != null)
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userIdClaim));
        }

        var identity = new ClaimsIdentity(claims, "TestAuthType");
        var user = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = user
            }
        };
    }

    // GetUserFriends

    [TestMethod]
    public void GetUserFriends_ShouldReturnOkWithMessage()
    {
        var userId = "user-123";

        var result = _controller.GetUserFriends(userId);

        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult, "Expected OkObjectResult");
        Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);
        Assert.IsNotNull(okResult.Value, "Expected a message payload");
    }

    // SendFriendRequestAsync

    [TestMethod]
    public async Task SendFriendRequestAsync_ShouldReturnOkWithFriendship_WhenSuccessful()
    {
        var userId = "user-1";
        var receiverId = "user-2";

        var friendshipFromRepo = new Friendship
        {
            SenderId = userId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Pending
        };

        _mockRepository
            .Setup(r => r.SendFriendRequestAsync(userId, receiverId))
            .ReturnsAsync(friendshipFromRepo);

        var result = await _controller.SendFriendRequestAsync(userId, receiverId);

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
        var userId = "user-1";
        var receiverId = "user-1";

        var result = await _controller.SendFriendRequestAsync(userId, receiverId);

        var badRequest = result.Result as BadRequestObjectResult;
        Assert.IsNotNull(badRequest, "Expected BadRequestObjectResult");
        Assert.AreEqual(StatusCodes.Status400BadRequest, badRequest.StatusCode);

        _mockRepository.Verify(r => r.SendFriendRequestAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task SendFriendRequestAsync_ShouldReturnBadRequest_WhenIdsAreInvalid()
    {
        var userId = "";
        var receiverId = "   ";

        var result = await _controller.SendFriendRequestAsync(userId, receiverId);

        var badRequest = result.Result as BadRequestObjectResult;
        Assert.IsNotNull(badRequest, "Expected BadRequestObjectResult");
        Assert.AreEqual(StatusCodes.Status400BadRequest, badRequest.StatusCode);

        _mockRepository.Verify(r => r.SendFriendRequestAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task SendFriendRequestAsync_ShouldReturnBadRequest_WhenRepositoryThrowsArgumentException()
    {
        var userId = "user-1";
        var receiverId = "user-2";

        _mockRepository
            .Setup(r => r.SendFriendRequestAsync(userId, receiverId))
            .ThrowsAsync(new ArgumentException("Invalid"));

        var result = await _controller.SendFriendRequestAsync(userId, receiverId);

        var badRequest = result.Result as BadRequestObjectResult;
        Assert.IsNotNull(badRequest, "Expected BadRequestObjectResult");
        Assert.AreEqual(StatusCodes.Status400BadRequest, badRequest.StatusCode);
    }

    [TestMethod]
    public async Task SendFriendRequestAsync_ShouldReturnConflict_WhenRepositoryThrowsInvalidOperationException()
    {
        var userId = "user-1";
        var receiverId = "user-2";

        _mockRepository
            .Setup(r => r.SendFriendRequestAsync(userId, receiverId))
            .ThrowsAsync(new InvalidOperationException("Friendship already exists"));

        var result = await _controller.SendFriendRequestAsync(userId, receiverId);

        var conflict = result.Result as ConflictObjectResult;
        Assert.IsNotNull(conflict, "Expected ConflictObjectResult");
        Assert.AreEqual(StatusCodes.Status409Conflict, conflict.StatusCode);
    }

    [TestMethod]
    public async Task SendFriendRequestAsync_ShouldReturnNotFound_WhenRepositoryThrowsKeyNotFoundException()
    {
        var userId = "user-1";
        var receiverId = "user-2";

        _mockRepository
            .Setup(r => r.SendFriendRequestAsync(userId, receiverId))
            .ThrowsAsync(new KeyNotFoundException("User not found"));

        var result = await _controller.SendFriendRequestAsync(userId, receiverId);

        var notFound = result.Result as NotFoundObjectResult;
        Assert.IsNotNull(notFound, "Expected NotFoundObjectResult");
        Assert.AreEqual(StatusCodes.Status404NotFound, notFound.StatusCode);
    }

    [TestMethod]
    public async Task SendFriendRequestAsync_ShouldReturn500_WhenRepositoryThrowsUnexpectedException()
    {
        var userId = "user-1";
        var receiverId = "user-2";

        _mockRepository
            .Setup(r => r.SendFriendRequestAsync(userId, receiverId))
            .ThrowsAsync(new Exception("Unexpected"));

        var result = await _controller.SendFriendRequestAsync(userId, receiverId);

        var statusResult = result.Result as ObjectResult;
        Assert.IsNotNull(statusResult, "Expected ObjectResult");
        Assert.AreEqual(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
    }

    // DeclineFriendRequestAsync

    [TestMethod]
    public async Task DeclineFriendRequestAsync_ShouldReturnOkWithStatusDeclined_WhenSuccessful()
    {
        var userId = "user-1";
        var receiverId = "user-2";

        var friendshipFromRepo = new Friendship
        {
            SenderId = userId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Declined
        };

        _mockRepository
            .Setup(r => r.DeclineFriendRequestAsync(userId, receiverId))
            .ReturnsAsync(friendshipFromRepo);

        var result = await _controller.DeclineFriendRequestAsync(userId, receiverId);

        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult, "Expected OkObjectResult");
        Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);

        var status = (FriendshipStatus)okResult.Value!;
        Assert.AreEqual(FriendshipStatus.Declined, status);
    }

    [TestMethod]
    public async Task DeclineFriendRequestAsync_ShouldReturnBadRequest_WhenUserIdEqualsReceiverId()
    {
        var userId = "user-1";
        var receiverId = "user-1";

        var result = await _controller.DeclineFriendRequestAsync(userId, receiverId);

        var badRequest = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequest, "Expected BadRequestObjectResult");
        Assert.AreEqual(StatusCodes.Status400BadRequest, badRequest.StatusCode);

        _mockRepository.Verify(r => r.DeclineFriendRequestAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task DeclineFriendRequestAsync_ShouldReturnBadRequest_WhenIdsAreInvalid()
    {
        var userId = "   ";
        var receiverId = "";

        var result = await _controller.DeclineFriendRequestAsync(userId, receiverId);

        var badRequest = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequest, "Expected BadRequestObjectResult");
        Assert.AreEqual(StatusCodes.Status400BadRequest, badRequest.StatusCode);

        _mockRepository.Verify(r => r.DeclineFriendRequestAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task DeclineFriendRequestAsync_ShouldReturnNotFound_WhenRepositoryThrowsKeyNotFoundException()
    {
        var userId = "user-1";
        var receiverId = "user-2";

        _mockRepository
            .Setup(r => r.DeclineFriendRequestAsync(userId, receiverId))
            .ThrowsAsync(new KeyNotFoundException("Not found"));

        var result = await _controller.DeclineFriendRequestAsync(userId, receiverId);

        var notFound = result as NotFoundObjectResult;
        Assert.IsNotNull(notFound, "Expected NotFoundObjectResult");
        Assert.AreEqual(StatusCodes.Status404NotFound, notFound.StatusCode);
    }

    [TestMethod]
    public async Task DeclineFriendRequestAsync_ShouldReturnConflict_WhenRepositoryThrowsInvalidOperationException()
    {
        var userId = "user-1";
        var receiverId = "user-2";

        _mockRepository
            .Setup(r => r.DeclineFriendRequestAsync(userId, receiverId))
            .ThrowsAsync(new InvalidOperationException("Cannot decline"));

        var result = await _controller.DeclineFriendRequestAsync(userId, receiverId);

        var conflict = result as ConflictObjectResult;
        Assert.IsNotNull(conflict, "Expected ConflictObjectResult");
        Assert.AreEqual(StatusCodes.Status409Conflict, conflict.StatusCode);
    }

    [TestMethod]
    public async Task DeclineFriendRequestAsync_ShouldReturnBadRequest_WhenRepositoryThrowsArgumentException()
    {
        var userId = "user-1";
        var receiverId = "user-2";

        _mockRepository
            .Setup(r => r.DeclineFriendRequestAsync(userId, receiverId))
            .ThrowsAsync(new ArgumentException("Invalid"));

        var result = await _controller.DeclineFriendRequestAsync(userId, receiverId);

        var badRequest = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequest, "Expected BadRequestObjectResult");
        Assert.AreEqual(StatusCodes.Status400BadRequest, badRequest.StatusCode);
    }

    [TestMethod]
    public async Task DeclineFriendRequestAsync_ShouldReturn500_WhenRepositoryThrowsUnexpectedException()
    {
        var userId = "user-1";
        var receiverId = "user-2";

        _mockRepository
            .Setup(r => r.DeclineFriendRequestAsync(userId, receiverId))
            .ThrowsAsync(new Exception("Unexpected"));

        var result = await _controller.DeclineFriendRequestAsync(userId, receiverId);

        var statusResult = result as ObjectResult;
        Assert.IsNotNull(statusResult, "Expected ObjectResult");
        Assert.AreEqual(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
    }

    // GetAllFriends

    [TestMethod]
    public async Task GetAllFriends_ShouldReturnOkWithFriends_WhenSuccessful()
    {
        var userId = "user-1";

        var friendship = new Friendship
        {
            SenderId = userId,
            ReceiverId = "user-2",
            FriendShipStatus = FriendshipStatus.Accepted
        };

        _mockRepository
            .Setup(r => r.GetAllFriends(userId))
            .ReturnsAsync(new List<Friendship> { friendship });

        var result = await _controller.GetAllFriends(userId);

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
        var userId = "user-1";

        _mockRepository
            .Setup(r => r.GetAllFriends(userId))
            .ThrowsAsync(new KeyNotFoundException("Not found"));

        var result = await _controller.GetAllFriends(userId);

        var notFound = result.Result as NotFoundObjectResult;
        Assert.IsNotNull(notFound, "Expected NotFoundObjectResult");
        Assert.AreEqual(StatusCodes.Status404NotFound, notFound.StatusCode);
    }

    [TestMethod]
    public async Task GetAllFriends_ShouldReturnConflict_WhenRepositoryThrowsInvalidOperationException()
    {
        var userId = "user-1";

        _mockRepository
            .Setup(r => r.GetAllFriends(userId))
            .ThrowsAsync(new InvalidOperationException("Conflict"));

        var result = await _controller.GetAllFriends(userId);

        var conflict = result.Result as ConflictObjectResult;
        Assert.IsNotNull(conflict, "Expected ConflictObjectResult");
        Assert.AreEqual(StatusCodes.Status409Conflict, conflict.StatusCode);
    }

    [TestMethod]
    public async Task GetAllFriends_ShouldReturnBadRequest_WhenRepositoryThrowsArgumentException()
    {
        var userId = "user-1";

        _mockRepository
            .Setup(r => r.GetAllFriends(userId))
            .ThrowsAsync(new ArgumentException("Invalid"));

        var result = await _controller.GetAllFriends(userId);

        var badRequest = result.Result as BadRequestObjectResult;
        Assert.IsNotNull(badRequest, "Expected BadRequestObjectResult");
        Assert.AreEqual(StatusCodes.Status400BadRequest, badRequest.StatusCode);
    }

    [TestMethod]
    public async Task GetAllFriends_ShouldReturn500_WhenRepositoryThrowsUnexpectedException()
    {
        var userId = "user-1";

        _mockRepository
            .Setup(r => r.GetAllFriends(userId))
            .ThrowsAsync(new Exception("Unexpected"));

        var result = await _controller.GetAllFriends(userId);

        var statusResult = result.Result as ObjectResult;
        Assert.IsNotNull(statusResult, "Expected ObjectResult");
        Assert.AreEqual(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
    }

    // GetFriendById

    [TestMethod]
    public async Task GetFriendById_ShouldReturnOkWithFriend_WhenFound()
    {
        var userId = "user-1";
        var receiverId = "user-2";

        var friendship = new Friendship
        {
            SenderId = userId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Accepted
        };

        _mockRepository
            .Setup(r => r.GetFriendById(userId, receiverId))
            .ReturnsAsync(friendship);

        var result = await _controller.GetFriendById(userId, receiverId);

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
        var userId = "user-1";
        var receiverId = "user-2";

        _mockRepository
            .Setup(r => r.GetFriendById(userId, receiverId))
            .ReturnsAsync((Friendship?)null);

        var result = await _controller.GetFriendById(userId, receiverId);

        var notFound = result.Result as NotFoundResult;
        Assert.IsNotNull(notFound, "Expected NotFoundResult");
        Assert.AreEqual(StatusCodes.Status404NotFound, notFound.StatusCode);
    }

    [TestMethod]
    public async Task GetFriendById_ShouldReturnNotFound_WhenRepositoryThrowsKeyNotFoundException()
    {
        var userId = "user-1";
        var receiverId = "user-2";

        _mockRepository
            .Setup(r => r.GetFriendById(userId, receiverId))
            .ThrowsAsync(new KeyNotFoundException("Not found"));

        var result = await _controller.GetFriendById(userId, receiverId);

        var notFound = result.Result as NotFoundObjectResult;
        Assert.IsNotNull(notFound, "Expected NotFoundObjectResult");
        Assert.AreEqual(StatusCodes.Status404NotFound, notFound.StatusCode);
    }

    [TestMethod]
    public async Task GetFriendById_ShouldReturnConflict_WhenRepositoryThrowsInvalidOperationException()
    {
        var userId = "user-1";
        var receiverId = "user-2";

        _mockRepository
            .Setup(r => r.GetFriendById(userId, receiverId))
            .ThrowsAsync(new InvalidOperationException("Duplicate"));

        var result = await _controller.GetFriendById(userId, receiverId);

        var conflict = result.Result as ConflictObjectResult;
        Assert.IsNotNull(conflict, "Expected ConflictObjectResult");
        Assert.AreEqual(StatusCodes.Status409Conflict, conflict.StatusCode);
    }

    [TestMethod]
    public async Task GetFriendById_ShouldReturnBadRequest_WhenRepositoryThrowsArgumentException()
    {
        var userId = "user-1";
        var receiverId = "user-2";

        _mockRepository
            .Setup(r => r.GetFriendById(userId, receiverId))
            .ThrowsAsync(new ArgumentException("Invalid"));

        var result = await _controller.GetFriendById(userId, receiverId);

        var badRequest = result.Result as BadRequestObjectResult;
        Assert.IsNotNull(badRequest, "Expected BadRequestObjectResult");
        Assert.AreEqual(StatusCodes.Status400BadRequest, badRequest.StatusCode);
    }

    [TestMethod]
    public async Task GetFriendById_ShouldReturn500_WhenRepositoryThrowsUnexpectedException()
    {
        var userId = "user-1";
        var receiverId = "user-2";

        _mockRepository
            .Setup(r => r.GetFriendById(userId, receiverId))
            .ThrowsAsync(new Exception("Unexpected"));

        var result = await _controller.GetFriendById(userId, receiverId);

        var statusResult = result.Result as ObjectResult;
        Assert.IsNotNull(statusResult, "Expected ObjectResult");
        Assert.AreEqual(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
    }

    // CancelFriendRequest

    [TestMethod]
    public async Task CancelFriendRequest_ShouldReturnOkWithFriendship_WhenSuccessful()
    {
        var userId = "user-1";
        var receiverId = "user-2";

        var friendship = new Friendship
        {
            SenderId = userId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.None
        };

        _mockRepository
            .Setup(r => r.CancelFriendRequest(userId, receiverId))
            .ReturnsAsync(friendship);

        var result = await _controller.CancelFriendRequest(userId, receiverId);

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
        var userId = "user-1";
        var receiverId = "user-2";

        _mockRepository
            .Setup(r => r.CancelFriendRequest(userId, receiverId))
            .ThrowsAsync(new InvalidOperationException("No pending request"));

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            async () => await _controller.CancelFriendRequest(userId, receiverId));
    }

    // GetOutgoingFriendRequests

    [TestMethod]
    public async Task GetAllFriendRequests_WhenItsSuccefull_ShouldReturnStatusCode200()
    {
        var userId = "user-1";

        var friendshipFromRepo = new Friendship
        {
            SenderId = userId,
            FriendShipStatus = FriendshipStatus.Pending
        };

        _mockRepository.Setup(r => r.GetOutgoingFriendRequestsAsync(userId))
            .ReturnsAsync(new List<Friendship> { friendshipFromRepo });

        var result = await _controller.GetOutgoingFriendRequests(userId);

        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult, "Expected OkObjectResult");
        Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);

        _mockRepository.Verify(r => r.GetOutgoingFriendRequestsAsync(userId), Times.Once);
    }

    [TestMethod]
    public async Task GetAllFriendRequests_WhenItsUnSuccesfull_ShouldReturnStatusCode400()
    {
        var userId = "user-123";

        _mockRepository
            .Setup(r => r.GetOutgoingFriendRequestsAsync(userId))
            .ReturnsAsync((List<Friendship>?)null);

        var result = await _controller.GetOutgoingFriendRequests(userId);

        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult, "Expected BadRequestObjectResult");
        Assert.AreEqual(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);

        _mockRepository.Verify(r => r.GetOutgoingFriendRequestsAsync(userId), Times.Once);
    }

    // GetAllIncomingFriendRequests

    [TestMethod]
    public async Task GetAllIncomingFriendRequests_ShouldReturnOkWithList_WhenNotNull()
    {
        var userId = "user-1";
        var friendship = new Friendship
        {
            SenderId = "user-2",
            ReceiverId = userId,
            FriendShipStatus = FriendshipStatus.Pending
        };

        _mockRepository
            .Setup(r => r.GetAllIncomingFriendRequests(userId))
            .ReturnsAsync(new List<Friendship> { friendship });

        var result = await _controller.GetAllIncomingFriendRequests(userId);

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
        var userId = "user-1";

        _mockRepository
            .Setup(r => r.GetAllIncomingFriendRequests(userId))
            .ReturnsAsync((IEnumerable<Friendship>?)null);

        var result = await _controller.GetAllIncomingFriendRequests(userId);

        var badRequest = result.Result as BadRequestObjectResult;
        Assert.IsNotNull(badRequest, "Expected BadRequestObjectResult");
        Assert.AreEqual(StatusCodes.Status400BadRequest, badRequest.StatusCode);
    }

    // AcceptFriendRequestAsync

    [TestMethod]
    public async Task AcceptFriendRequestAsync_ShouldReturnOkWithStatusAccepted_WhenSuccessful()
    {
        var userId = "user-1";
        var receiverId = "user-2";

        _mockRepository
            .Setup(r => r.AcceptFriendRequest(userId, receiverId))
            .ReturnsAsync(new Friendship
            {
                SenderId = userId,
                ReceiverId = receiverId,
                FriendShipStatus = FriendshipStatus.Accepted
            });

        var result = await _controller.AcceptFriendRequestAsync(userId, receiverId);

        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult, "Expected OkObjectResult");
        Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);

        var status = (FriendshipStatus)okResult.Value!;
        Assert.AreEqual(FriendshipStatus.Accepted, status);
    }

    [TestMethod]
    public async Task AcceptFriendRequestAsync_ShouldReturnBadRequest_WhenUserIdEqualsReceiverId()
    {
        var userId = "user-1";
        var receiverId = "user-1";

        var result = await _controller.AcceptFriendRequestAsync(userId, receiverId);

        var badRequest = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequest, "Expected BadRequestObjectResult");
        Assert.AreEqual(StatusCodes.Status400BadRequest, badRequest.StatusCode);

        _mockRepository.Verify(r => r.AcceptFriendRequest(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task AcceptFriendRequestAsync_ShouldReturnBadRequest_WhenIdsAreInvalid()
    {
        var userId = "";
        var receiverId = "   ";

        var result = await _controller.AcceptFriendRequestAsync(userId, receiverId);

        var badRequest = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequest, "Expected BadRequestObjectResult");
        Assert.AreEqual(StatusCodes.Status400BadRequest, badRequest.StatusCode);

        _mockRepository.Verify(r => r.AcceptFriendRequest(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task AcceptFriendRequestAsync_ShouldReturnNotFound_WhenRepositoryThrowsKeyNotFoundException()
    {
        var userId = "user-1";
        var receiverId = "user-2";

        _mockRepository
            .Setup(r => r.AcceptFriendRequest(userId, receiverId))
            .ThrowsAsync(new KeyNotFoundException("Not found"));

        var result = await _controller.AcceptFriendRequestAsync(userId, receiverId);

        var notFound = result as NotFoundObjectResult;
        Assert.IsNotNull(notFound, "Expected NotFoundObjectResult");
        Assert.AreEqual(StatusCodes.Status404NotFound, notFound.StatusCode);
    }

    [TestMethod]
    public async Task AcceptFriendRequestAsync_ShouldReturnConflict_WhenRepositoryThrowsInvalidOperationException()
    {
        var userId = "user-1";
        var receiverId = "user-2";

        _mockRepository
            .Setup(r => r.AcceptFriendRequest(userId, receiverId))
            .ThrowsAsync(new InvalidOperationException("Cannot accept"));

        var result = await _controller.AcceptFriendRequestAsync(userId, receiverId);

        var conflict = result as ConflictObjectResult;
        Assert.IsNotNull(conflict, "Expected ConflictObjectResult");
        Assert.AreEqual(StatusCodes.Status409Conflict, conflict.StatusCode);
    }

    [TestMethod]
    public async Task AcceptFriendRequestAsync_ShouldReturnBadRequest_WhenRepositoryThrowsArgumentException()
    {
        var userId = "user-1";
        var receiverId = "user-2";

        _mockRepository
            .Setup(r => r.AcceptFriendRequest(userId, receiverId))
            .ThrowsAsync(new ArgumentException("Invalid"));

        var result = await _controller.AcceptFriendRequestAsync(userId, receiverId);

        var badRequest = result as BadRequestObjectResult;
        Assert.IsNotNull(badRequest, "Expected BadRequestObjectResult");
        Assert.AreEqual(StatusCodes.Status400BadRequest, badRequest.StatusCode);
    }

    [TestMethod]
    public async Task AcceptFriendRequestAsync_ShouldReturn500_WhenRepositoryThrowsUnexpectedException()
    {
        var userId = "user-1";
        var receiverId = "user-2";

        _mockRepository
            .Setup(r => r.AcceptFriendRequest(userId, receiverId))
            .ThrowsAsync(new Exception("Unexpected"));

        var result = await _controller.AcceptFriendRequestAsync(userId, receiverId);

        var statusResult = result as ObjectResult;
        Assert.IsNotNull(statusResult, "Expected ObjectResult");
        Assert.AreEqual(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
    }

    // Post tests

    [TestMethod]
    public async Task PostAPost_calls_repository_and_returns_created_post()
    {
        var repoMock = new Mock<ISocialRepository>();

        var inputPost = new Post
        {
            UserId = "user-1",
            FitnessClassId = "10",
            WorkoutId = "100",
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

        var result = await controller.PostAPost(inputPost);

        Assert.AreSame(createdPost, result);
        repoMock.Verify(r => r.PostAPost(inputPost), Times.Once);
    }

    // RemoveAPost

    [TestMethod]
    public async Task RemoveAPost_calls_repository_and_returns_removed_post()
    {
        var postId = "some-mongo-id";

        var removedPost = new Post
        {
            Id = postId,
            UserId = "user-1",
            FitnessClassId = "10",
            WorkoutId = "100",
            PostTitle = "Removed title",
            PostContent = "Removed content"
        };

        _mockRepository
            .Setup(r => r.RemoveAPost(postId))
            .ReturnsAsync(removedPost);

        var result = await _controller.RemoveAPost(postId);

        Assert.IsNotNull(result, "Expected a Post to be returned");
        Assert.AreSame(removedPost, result, "Controller should return the Post from the repository");

        _mockRepository.Verify(r => r.RemoveAPost(postId), Times.Once);
    }

    // EditAPost

    [TestMethod]
    public async Task EditAPost_ShouldReturnUnauthorized_WhenNoUserIdClaim()
    {
        SetUserOnController(null);

        var post = new Post
        {
            Id = "post-1",
            PostTitle = "Title",
            PostContent = "Content"
        };

        var result = await _controller.EditAPost(post);

        var unauthorized = result.Result as UnauthorizedResult;
        Assert.IsNotNull(unauthorized, "Expected UnauthorizedResult");
        Assert.AreEqual(StatusCodes.Status401Unauthorized, unauthorized.StatusCode);

        _mockRepository.Verify(
            r => r.EditAPost(It.IsAny<Post>(), It.IsAny<string>()),
            Times.Never);
    }

    [TestMethod]
    public async Task EditAPost_ShouldReturnUnauthorized_WhenUserIdClaimIsWhitespace()
    {
        SetUserOnController("   ");

        var post = new Post
        {
            Id = "post-1",
            PostTitle = "Title",
            PostContent = "Content"
        };

        var result = await _controller.EditAPost(post);

        var unauthorized = result.Result as UnauthorizedResult;
        Assert.IsNotNull(unauthorized, "Expected UnauthorizedResult");
        Assert.AreEqual(StatusCodes.Status401Unauthorized, unauthorized.StatusCode);

        _mockRepository.Verify(
            r => r.EditAPost(It.IsAny<Post>(), It.IsAny<string>()),
            Times.Never);
    }

    [TestMethod]
    public async Task EditAPost_ShouldCallRepositoryWithCurrentUserId_AndReturnOkWithEditedPost()
    {
        var userId = "user-42";
        SetUserOnController(userId);

        var inputPost = new Post
        {
            Id = "post-1",
            UserId = "user-999",
            PostTitle = "Old title",
            PostContent = "Old content",
            FitnessClassId = "1",
            WorkoutId = "2"
        };

        var editedPostFromRepo = new Post
        {
            Id = "post-1",
            UserId = userId,
            PostTitle = "New title",
            PostContent = "New content",
            FitnessClassId = "3",
            WorkoutId = "4"
        };

        _mockRepository
            .Setup(r => r.EditAPost(It.IsAny<Post>(), userId))
            .ReturnsAsync(editedPostFromRepo);

        var result = await _controller.EditAPost(inputPost);

        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult, "Expected OkObjectResult");
        Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);

        var post = okResult.Value as Post;
        Assert.IsNotNull(post, "Expected Post as value");

        Assert.AreEqual(editedPostFromRepo.Id, post.Id);
        Assert.AreEqual(editedPostFromRepo.UserId, post.UserId);
        Assert.AreEqual(editedPostFromRepo.PostTitle, post.PostTitle);
        Assert.AreEqual(editedPostFromRepo.PostContent, post.PostContent);
        Assert.AreEqual(editedPostFromRepo.FitnessClassId, post.FitnessClassId);
        Assert.AreEqual(editedPostFromRepo.WorkoutId, post.WorkoutId);

        _mockRepository.Verify(
            r => r.EditAPost(
                It.Is<Post>(p =>
                    p.Id == inputPost.Id &&
                    p.UserId == userId &&
                    p.PostTitle == inputPost.PostTitle &&
                    p.PostContent == inputPost.PostContent),
                userId),
            Times.Once);
    }

    [TestMethod]
    public async Task EditAPost_ShouldReturnNotFound_WhenRepositoryThrowsKeyNotFoundException()
    {
        var userId = "user-42";
        SetUserOnController(userId);

        var inputPost = new Post
        {
            Id = "post-1",
            PostTitle = "Title",
            PostContent = "Content"
        };

        _mockRepository
            .Setup(r => r.EditAPost(It.IsAny<Post>(), userId))
            .ThrowsAsync(new KeyNotFoundException("Post not found or access denied"));

        var result = await _controller.EditAPost(inputPost);

        var notFound = result.Result as NotFoundResult;
        Assert.IsNotNull(notFound, "Expected NotFoundResult");
        Assert.AreEqual(StatusCodes.Status404NotFound, notFound.StatusCode);
    }

    // Comment tests

    [TestMethod]
    public async Task AddACommentToPost_calls_repository_and_returns_updated_post()
    {
        var postId = "some-mongo-id";

        var inputComment = new Comment
        {
            AuthorId = "user-1",
            CommentDate = DateTime.UtcNow,
            CommentText = "Nice workout!"
        };

        var updatedPostFromRepo = new Post
        {
            Id = postId,
            UserId = "user-1",
            FitnessClassId = "10",
            WorkoutId = "100",
            PostTitle = "Title",
            PostContent = "Content",
            Comments = new List<Comment> { inputComment }
        };

        _mockRepository
            .Setup(r => r.AddCommentToPost(postId, inputComment))
            .ReturnsAsync(updatedPostFromRepo);

        var result = await _controller.AddACommentToPost(postId, inputComment);

        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult, "Expected OkObjectResult");
        Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);

        var returnedPost = okResult.Value as Post;
        Assert.IsNotNull(returnedPost, "Expected Post as value");
        Assert.AreSame(updatedPostFromRepo, returnedPost, "Controller should return the Post from the repository");

        _mockRepository.Verify(
            r => r.AddCommentToPost(postId, inputComment),
            Times.Once);
    }

    [TestMethod]
    public async Task RemoveCommentFromPost_ShouldReturnOkAndCallRepository()
    {
        var postId = "post-123";
        var commentId = "comment-456";

        var updatedPost = new Post
        {
            Id = postId,
            UserId = "user-1",
            PostTitle = "Title",
            PostContent = "Content",
            Comments = new List<Comment>()
        };

        _mockRepository
            .Setup(r => r.RemoveCommentFromPost(postId, commentId))
            .ReturnsAsync(updatedPost);

        var result = await _controller.RemoveCommentFromPost(postId, commentId);

        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult, "Expected OkObjectResult");
        Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);

        var returnedPost = okResult.Value as Post;
        Assert.IsNotNull(returnedPost, "Expected Post as value");
        Assert.AreSame(updatedPost, returnedPost, "Controller should return the Post from repository");

        _mockRepository.Verify(
            r => r.RemoveCommentFromPost(postId, commentId),
            Times.Once);
    }

    [TestMethod]
    public async Task EditComment_ShouldReturnOkWithUpdatedPost_WhenSuccessful()
    {
        var postId = "post-123";

        var inputComment = new Comment
        {
            Id = "comment-1",
            AuthorId = "user-1",
            CommentDate = DateTime.UtcNow,
            CommentText = "Edited text"
        };

        var updatedPostFromRepo = new Post
        {
            Id = postId,
            UserId = "user-1",
            PostTitle = "Title",
            PostContent = "Content",
            Comments = new List<Comment>
            {
                new Comment
                {
                    Id = inputComment.Id,
                    AuthorId = inputComment.AuthorId,
                    CommentDate = inputComment.CommentDate,
                    CommentText = inputComment.CommentText
                }
            }
        };

        _mockRepository
            .Setup(r => r.EditComment(postId, inputComment))
            .ReturnsAsync(updatedPostFromRepo);

        var result = await _controller.EditComment(postId, inputComment);

        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult, "Expected OkObjectResult");
        Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);

        var returnedPost = okResult.Value as Post;
        Assert.IsNotNull(returnedPost, "Expected Post as value");
        Assert.AreSame(updatedPostFromRepo, returnedPost, "Controller should return the Post from the repository");

        _mockRepository.Verify(
            r => r.EditComment(postId, inputComment),
            Times.Once);
    }

    [TestMethod]
    public async Task EditComment_ShouldReturnNotFound_WhenRepositoryThrowsKeyNotFoundException()
    {
        var postId = "post-123";

        var inputComment = new Comment
        {
            Id = "comment-1",
            CommentText = "Edited text"
        };

        _mockRepository
            .Setup(r => r.EditComment(postId, inputComment))
            .ThrowsAsync(new KeyNotFoundException("Post not found or comment not found"));

        var result = await _controller.EditComment(postId, inputComment);

        var notFound = result.Result as NotFoundObjectResult;
        Assert.IsNotNull(notFound, "Expected NotFoundObjectResult");
        Assert.AreEqual(StatusCodes.Status404NotFound, notFound.StatusCode);
        Assert.AreEqual("Post not found or comment not found", notFound.Value);
    }

    [TestMethod]
    public async Task EditComment_ShouldReturn500_WhenRepositoryThrowsUnexpectedException()
    {
        var postId = "post-123";

        var inputComment = new Comment
        {
            Id = "comment-1",
            CommentText = "Edited text"
        };

        _mockRepository
            .Setup(r => r.EditComment(postId, inputComment))
            .ThrowsAsync(new Exception("Unexpected"));

        var result = await _controller.EditComment(postId, inputComment);

        var statusResult = result.Result as ObjectResult;
        Assert.IsNotNull(statusResult, "Expected ObjectResult");
        Assert.AreEqual(StatusCodes.Status500InternalServerError, statusResult.StatusCode);
        Assert.AreEqual("An unexpected error occurred", statusResult.Value);
    }

    [TestMethod]
    public async Task SeeAllCommentForPost_calls_repository_and_returns_comment_list()
    {
        var postId = "post-123";

        var commentsFromRepo = new List<Comment>
        {
            new Comment
            {
                Id = "comment-1",
                AuthorId = "user-1",
                CommentText = "First comment",
                CommentDate = DateTime.UtcNow
            },
            new Comment
            {
                Id = "comment-2",
                AuthorId = "user-2",
                CommentText = "Second comment",
                CommentDate = DateTime.UtcNow
            }
        };

        _mockRepository
            .Setup(r => r.SeeAllCommentForPostId(postId))
            .ReturnsAsync(commentsFromRepo);

        var result = await _controller.SeeAllCommentForPost(postId);

        Assert.IsNotNull(result, "Expected a list of comments to be returned");
        Assert.AreSame(commentsFromRepo, result, "Controller should return the list from the repository");

        _mockRepository.Verify(
            r => r.SeeAllCommentForPostId(postId),
            Times.Once);
    }

    [TestMethod]
    public async Task SeeAllPostsForUser_calls_repository_and_returns_post_list()
    {
        var userId = "user-1";

        var postsFromRepo = new List<Post>
        {
            new Post
            {
                Id = "post-1",
                UserId = userId,
                FitnessClassId = "10",
                WorkoutId = "100",
                PostTitle = "Title 1",
                PostContent = "Content 1"
            },
            new Post
            {
                Id = "post-2",
                UserId = userId,
                FitnessClassId = "11",
                WorkoutId = "101",
                PostTitle = "Title 2",
                PostContent = "Content 2"
            }
        };

        _mockRepository
            .Setup(r => r.SeeAllPostsForUser(userId))
            .ReturnsAsync(postsFromRepo);

        var result = await _controller.SeeAllPostsForUser(userId);

        Assert.IsNotNull(result, "Expected a list of posts to be returned");
        Assert.AreSame(postsFromRepo, result, "Controller should return the list from the repository");

        _mockRepository.Verify(
            r => r.SeeAllPostsForUser(userId),
            Times.Once);
    }

    [TestMethod]
    public async Task SeeAllPostsForUser_when_repo_returns_empty_list_returns_empty_list()
    {
        var userId = "user-1";

        var emptyList = new List<Post>();

        _mockRepository
            .Setup(r => r.SeeAllPostsForUser(userId))
            .ReturnsAsync(emptyList);

        var result = await _controller.SeeAllPostsForUser(userId);

        Assert.IsNotNull(result, "Expected an empty list, not null");
        Assert.AreEqual(0, result.Count(), "Expected empty list");

        _mockRepository.Verify(
            r => r.SeeAllPostsForUser(userId),
            Times.Once);
    }
}
