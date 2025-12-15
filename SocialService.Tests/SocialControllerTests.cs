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

    // ClassWorkoutCompleted

    [TestMethod]
    public async Task ClassWorkoutCompleted_ShouldReturnBadRequest_WhenPayloadInvalid()
    {
        var dto = new ClassResultEventDto(
            EventId: "",
            ClassId: "class-1",
            UserName: "Kent", 
            UserId: "user-1",
            CaloriesBurned: 100,
            Watt: 200,
            DurationMin: 60,
            Date: DateTime.UtcNow
        );

        var result = await _controller.ClassWorkoutCompleted(dto);

        var bad = result as BadRequestObjectResult;
        Assert.IsNotNull(bad);
        Assert.AreEqual(StatusCodes.Status400BadRequest, bad.StatusCode);

        _mockRepository.Verify(
            r => r.CreateDraftFromClassWorkoutCompletedAsync(It.IsAny<ClassResultEventDto>()),
            Times.Never);
    }

    [TestMethod]
    public async Task ClassWorkoutCompleted_ShouldReturnOkObject_WithDraftId_WhenRepositoryCreatesDraft()
    {
        var dto = new ClassResultEventDto(
            EventId: Guid.NewGuid().ToString(),
            ClassId: "class-1",
            UserId: "user-1",
            UserName: "Kent",
            CaloriesBurned: 123.4,
            Watt: 250.0,
            DurationMin: 60,
            Date: DateTime.UtcNow
        );

        _mockRepository
            .Setup(r => r.CreateDraftFromClassWorkoutCompletedAsync(dto))
            .ReturnsAsync("draft-123");

        var result = await _controller.ClassWorkoutCompleted(dto);

        var ok = result as OkObjectResult;
        Assert.IsNotNull(ok);
        Assert.AreEqual(StatusCodes.Status200OK, ok.StatusCode);

        _mockRepository.Verify(
            r => r.CreateDraftFromClassWorkoutCompletedAsync(dto),
            Times.Once);
    }

    [TestMethod]
    public async Task ClassWorkoutCompleted_ShouldReturnOk_WhenRepositoryReturnsNull_DueToDedupe()
    {
        var dto = new ClassResultEventDto(
            EventId: Guid.NewGuid().ToString(),
            ClassId: "class-1",
            UserName: "Kent",
            UserId: "user-1",
            CaloriesBurned: 100,
            Watt: 200,
            DurationMin: 60,
            Date: DateTime.UtcNow
        );

        _mockRepository
            .Setup(r => r.CreateDraftFromClassWorkoutCompletedAsync(dto))
            .ReturnsAsync((string?)null);

        var result = await _controller.ClassWorkoutCompleted(dto);

        Assert.IsTrue(result is OkResult or OkObjectResult);

        _mockRepository.Verify(
            r => r.CreateDraftFromClassWorkoutCompletedAsync(dto),
            Times.Once);
    }

    // GetUserFriends

    [TestMethod]
    public async Task GetAllFriends_ShouldReturnOkWithFriends()
    {
        var userId = "user-123";

        _mockRepository
            .Setup(r => r.GetAllFriends(userId))
            .ReturnsAsync(new List<Friendship>());

        var result = await _controller.GetAllFriends(userId);

        Assert.IsNotNull(result.Result, "Expected an IActionResult");

        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult, "Expected OkObjectResult");
        Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);

        var friends = okResult.Value as IEnumerable<Friendship>;
        Assert.IsNotNull(friends, "Expected list payload");
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

// CancelFriendRequest (delete)

    [TestMethod]
    public async Task CancelFriendRequest_ShouldReturnNoContent_WhenSuccessful()
    {
        var userId = "user-1";
        var receiverId = "user-2";

        _mockRepository
            .Setup(r => r.CancelFriendRequest(userId, receiverId))
            .ReturnsAsync(new Friendship
            {
                SenderId = userId,
                ReceiverId = receiverId,
                FriendShipStatus = FriendshipStatus.Pending
            });

        var result = await _controller.CancelFriendRequest(userId, receiverId);

        Assert.IsInstanceOfType(result, typeof(NoContentResult));
    }

    [TestMethod]
    public async Task CancelFriendRequest_ShouldReturnNotFound_WhenRequestDoesNotExist()
    {
        var userId = "user-1";
        var receiverId = "user-2";

        _mockRepository
            .Setup(r => r.CancelFriendRequest(userId, receiverId))
            .ThrowsAsync(new InvalidOperationException("Pending friend request not found"));

        var result = await _controller.CancelFriendRequest(userId, receiverId);

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    [TestMethod]
    public async Task CancelFriendRequest_ShouldBubbleException_WhenRepositoryThrowsUnexpected()
    {
        var userId = "user-1";
        var receiverId = "user-2";

        _mockRepository
            .Setup(r => r.CancelFriendRequest(userId, receiverId))
            .ThrowsAsync(new Exception("Unexpected failure"));

        await Assert.ThrowsExceptionAsync<Exception>(
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

    // PostAPost

    [TestMethod]
    public async Task PostAPost_calls_repository_and_returns_created_post()
    {
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

        _mockRepository
            .Setup(r => r.PostAPost(inputPost))
            .ReturnsAsync(createdPost);

        var result = await _controller.PostAPost(inputPost);

        Assert.AreSame(createdPost, result);
        _mockRepository.Verify(r => r.PostAPost(inputPost), Times.Once);
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
public async Task EditAPost_ShouldReturnOkWithEditedPost_WhenSuccessful()
{
    var inputPost = new Post
    {
        Id = "post-1",
        UserId = "user-42",
        PostTitle = "Old title",
        PostContent = "Old content",
        FitnessClassId = "1",
        WorkoutId = "2"
    };

    var editedPostFromRepo = new Post
    {
        Id = "post-1",
        UserId = "user-42",
        PostTitle = "New title",
        PostContent = "New content",
        FitnessClassId = "3",
        WorkoutId = "4"
    };

    _mockRepository
        .Setup(r => r.EditAPost(It.IsAny<Post>()))
        .ReturnsAsync(editedPostFromRepo);

    var result = await _controller.EditAPost(inputPost);

    var okResult = result.Result as OkObjectResult;
    Assert.IsNotNull(okResult, "Expected OkObjectResult");
    Assert.AreEqual(StatusCodes.Status200OK, okResult.StatusCode);

    var returnedPost = okResult.Value as Post;
    Assert.IsNotNull(returnedPost, "Expected Post as value");

    Assert.AreEqual(editedPostFromRepo.Id, returnedPost.Id);
    Assert.AreEqual(editedPostFromRepo.UserId, returnedPost.UserId);
    Assert.AreEqual(editedPostFromRepo.PostTitle, returnedPost.PostTitle);
    Assert.AreEqual(editedPostFromRepo.PostContent, returnedPost.PostContent);
    Assert.AreEqual(editedPostFromRepo.FitnessClassId, returnedPost.FitnessClassId);
    Assert.AreEqual(editedPostFromRepo.WorkoutId, returnedPost.WorkoutId);

    _mockRepository.Verify(
        r => r.EditAPost(It.Is<Post>(p =>
            p.Id == inputPost.Id &&
            p.UserId == inputPost.UserId &&
            p.PostTitle == inputPost.PostTitle &&
            p.PostContent == inputPost.PostContent &&
            p.FitnessClassId == inputPost.FitnessClassId &&
            p.WorkoutId == inputPost.WorkoutId
        )),
        Times.Once);
}

[TestMethod]
public async Task EditAPost_ShouldReturnNotFound_WhenRepositoryThrowsKeyNotFoundException()
{
    var inputPost = new Post
    {
        Id = "post-1",
        UserId = "user-42",
        PostTitle = "Title",
        PostContent = "Content"
    };

    _mockRepository
        .Setup(r => r.EditAPost(It.IsAny<Post>()))
        .ThrowsAsync(new KeyNotFoundException("Post not found or access denied"));

    var result = await _controller.EditAPost(inputPost);

    var notFound = result.Result as NotFoundResult;
    Assert.IsNotNull(notFound, "Expected NotFoundResult");
    Assert.AreEqual(StatusCodes.Status404NotFound, notFound.StatusCode);

    _mockRepository.Verify(r => r.EditAPost(It.IsAny<Post>()), Times.Once);
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
    
    [TestMethod]
    public async Task SeeAllFriendsPosts_calls_repository_and_returns_posts()
    {
        var userId = "user-1";

        var postsFromRepo = new List<Post>
        {
            new Post
            {
                Id = "post-1",
                UserId = "user-1",
                PostTitle = "Own post",
                PostContent = "Content",
                PostDate = new DateTime(2020, 1, 2),
                Comments = new List<Comment>()
            },
            new Post
            {
                Id = "post-2",
                UserId = "user-2",
                PostTitle = "Friend post",
                PostContent = "Content",
                PostDate = new DateTime(2020, 1, 3),
                Comments = new List<Comment>()
            }
        };

        _mockRepository
            .Setup(r => r.SeeAllFriendsPosts(userId))
            .ReturnsAsync(postsFromRepo);

        var result = await _controller.SeeAllFriendsPosts(userId);

        Assert.IsNotNull(result, "Expected a list of posts to be returned");
        Assert.AreSame(postsFromRepo, result, "Controller should return the list from the repository");
        Assert.AreEqual(2, result.Count(), "Expected exactly 2 posts");

        _mockRepository.Verify(r => r.SeeAllFriendsPosts(userId), Times.Once);
    }
    
    [TestMethod]
    public async Task SoloTrainingCompleted_InvalidPayload_ReturnsBadRequest()
    {
        var dto = new SoloTrainingCompletedEventDto
        {
            UserId = "",
            SoloTrainingSessionId = "",
            TrainingType = "",
            DurationMinutes = 0
        };

        var result = await _controller.SoloTrainingCompleted(dto);

        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task SoloTrainingCompleted_Valid_ReturnsOk()
    {
        var dto = new SoloTrainingCompletedEventDto
        {
            EventId = null,
            UserId = "user123",
            SoloTrainingSessionId = "session123",
            Date = DateTime.UtcNow,
            TrainingType = "UpperBody",
            DurationMinutes = 30,
            ExerciseCount = 5
        };

        _mockRepository.Setup(r => r.CreateDraftFromSoloTrainingCompletedAsync(It.IsAny<SoloTrainingCompletedEventDto>()))
            .ReturnsAsync("draft123");

        var result = await _controller.SoloTrainingCompleted(dto);

        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
    }

}
