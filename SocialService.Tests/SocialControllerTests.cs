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

    // ClassWorkoutCompleted

    [TestMethod]
    public async Task ClassWorkoutCompleted_InvalidPayload_ReturnsBadRequest_AndDoesNotCallRepo()
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

        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));

        _mockRepository.Verify(
            r => r.CreateDraftFromClassWorkoutCompletedAsync(It.IsAny<ClassResultEventDto>()),
            Times.Never);
    }

    [TestMethod]
    public async Task ClassWorkoutCompleted_Valid_ReturnsOk_AndCallsRepo()
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

        Assert.IsTrue(result is OkResult or OkObjectResult);

        _mockRepository.Verify(
            r => r.CreateDraftFromClassWorkoutCompletedAsync(dto),
            Times.Once);
    }

    // SoloTrainingCompleted

    [TestMethod]
    public async Task SoloTrainingCompleted_InvalidPayload_ReturnsBadRequest()
    {
        var dto = new SoloTrainingCompletedEventDto
        {
            UserId = "",
            SoloTrainingSessionId = "",
            WorkoutProgramName = "",
            DurationMinutes = 0
        };

        var result = await _controller.SoloTrainingCompleted(dto);

        Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
    }

    [TestMethod]
    public async Task SoloTrainingCompleted_Valid_ReturnsOk_AndCallsRepo()
    {
        var dto = new SoloTrainingCompletedEventDto
        {
            EventId = null,
            UserId = "user123",
            SoloTrainingSessionId = "session123",
            Date = DateTime.UtcNow,
            TrainingType = "UpperBody",
            DurationMinutes = 30,
            ExerciseCount = 5,
            WorkoutProgramName = "Strength Builder"
        };

        _mockRepository
            .Setup(r => r.CreateDraftFromSoloTrainingCompletedAsync(It.IsAny<SoloTrainingCompletedEventDto>()))
            .ReturnsAsync("draft123");

        var result = await _controller.SoloTrainingCompleted(dto);

        Assert.IsTrue(result is OkResult or OkObjectResult);

        _mockRepository.Verify(
            r => r.CreateDraftFromSoloTrainingCompletedAsync(It.IsAny<SoloTrainingCompletedEventDto>()),
            Times.Once);
    }

    // SendFriendRequestAsync

    [TestMethod]
    public async Task SendFriendRequestAsync_Success_ReturnsOkWithFriendship()
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

        var ok = result.Result as OkObjectResult;
        Assert.IsNotNull(ok);
        Assert.AreEqual(StatusCodes.Status200OK, ok.StatusCode);

        var friendship = ok.Value as Friendship;
        Assert.IsNotNull(friendship);
        Assert.AreEqual(FriendshipStatus.Pending, friendship.FriendShipStatus);

        _mockRepository.Verify(r => r.SendFriendRequestAsync(userId, receiverId), Times.Once);
    }

    [TestMethod]
    public async Task SendFriendRequestAsync_SameUser_ReturnsBadRequest_AndDoesNotCallRepo()
    {
        var userId = "user-1";
        var receiverId = "user-1";

        var result = await _controller.SendFriendRequestAsync(userId, receiverId);

        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));

        _mockRepository.Verify(r => r.SendFriendRequestAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task SendFriendRequestAsync_WhenRepoThrowsInvalidOperation_ReturnsConflict()
    {
        var userId = "user-1";
        var receiverId = "user-2";

        _mockRepository
            .Setup(r => r.SendFriendRequestAsync(userId, receiverId))
            .ThrowsAsync(new InvalidOperationException("Friendship already exists"));

        var result = await _controller.SendFriendRequestAsync(userId, receiverId);

        Assert.IsInstanceOfType(result.Result, typeof(ConflictObjectResult));
    }

    // GetAllFriends

    [TestMethod]
    public async Task GetAllFriends_Success_ReturnsOkWithList()
    {
        var userId = "user-1";

        _mockRepository
            .Setup(r => r.GetAllFriends(userId))
            .ReturnsAsync(new List<Friendship>());

        var result = await _controller.GetAllFriends(userId);

        var ok = result.Result as OkObjectResult;
        Assert.IsNotNull(ok);

        var list = ok.Value as IEnumerable<Friendship>;
        Assert.IsNotNull(list);

        _mockRepository.Verify(r => r.GetAllFriends(userId), Times.Once);
    }

    [TestMethod]
    public async Task GetAllFriends_WhenRepoThrowsKeyNotFound_ReturnsNotFound()
    {
        var userId = "user-1";

        _mockRepository
            .Setup(r => r.GetAllFriends(userId))
            .ThrowsAsync(new KeyNotFoundException("Not found"));

        var result = await _controller.GetAllFriends(userId);

        Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
    }
    
    
    
    
    //GetAllIncommingFriendRequests
    [TestMethod]
    public async Task GetAllFriendRequest_Found_ReturnsOk()
    {
        //Arrange
        var userId = "user-1";
        
        _mockRepository
            .Setup(r => r.GetAllIncomingFriendRequests(userId))
            .ReturnsAsync(new List<Friendship>());

        
        //Act
        var result = await _controller.GetAllIncomingFriendRequests(userId);
        
        //Assert
        
        
        var ok = result.Result as OkObjectResult;
        Assert.IsNotNull(ok, "Expected OkObjectResult");
        Assert.AreEqual(StatusCodes.Status200OK, ok.StatusCode);
        
        //Verficere, at vi ramte repositoriet 
        _mockRepository.Verify(r => r.GetAllIncomingFriendRequests(userId), Times.Once);
    }
    
    [TestMethod]
    public async Task GetAllFriendRequest_WhenNotFound_ShouldReturnStatusCode400()
    {
        //Arrange
        var userId = "user-1";
        
        _mockRepository
            .Setup(r => r.GetAllIncomingFriendRequests(userId))
            .ReturnsAsync((List<Friendship>?)null);

        
        //Act
        var result = await _controller.GetAllIncomingFriendRequests(userId);
        
        //Assert
        
        
        var badRequest = result.Result as BadRequestObjectResult;
        Assert.IsNotNull(badRequest, "Expected OkObjectResult");
        Assert.AreEqual(StatusCodes.Status400BadRequest, badRequest.StatusCode);
        
        //Verficere, at vi ramte repositoriet 
        _mockRepository.Verify(r => r.GetAllIncomingFriendRequests(userId), Times.Once);
    }
    
    // GetFriendById

    [TestMethod]
    public async Task GetFriendById_Found_ReturnsOk()
    {
        var userId = "user-1";
        var receiverId = "user-2";

        _mockRepository
            .Setup(r => r.GetFriendById(userId, receiverId))
            .ReturnsAsync(new Friendship
            {
                SenderId = userId,
                ReceiverId = receiverId,
                FriendShipStatus = FriendshipStatus.Accepted
            });

        var result = await _controller.GetFriendById(userId, receiverId);

        var ok = result.Result as OkObjectResult;
        Assert.IsNotNull(ok);

        var friend = ok.Value as Friendship;
        Assert.IsNotNull(friend);
        Assert.AreEqual(userId, friend.SenderId);
        Assert.AreEqual(receiverId, friend.ReceiverId);
    }

    [TestMethod]
    public async Task GetFriendById_WhenRepoReturnsNull_ReturnsNotFound()
    {
        var userId = "user-1";
        var receiverId = "user-2";

        _mockRepository
            .Setup(r => r.GetFriendById(userId, receiverId))
            .ReturnsAsync((Friendship?)null);

        var result = await _controller.GetFriendById(userId, receiverId);

        Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
    }

    // CancelFriendRequest

    [TestMethod]
    public async Task CancelFriendRequest_Success_ReturnsNoContent()
    {
        var userId = "user-1";
        var receiverId = "user-2";

        _mockRepository
            .Setup(r => r.CancelFriendRequest(userId, receiverId))
            .ReturnsAsync(new Friendship { SenderId = userId, ReceiverId = receiverId, FriendShipStatus = FriendshipStatus.Pending });

        var result = await _controller.CancelFriendRequest(userId, receiverId);

        Assert.IsInstanceOfType(result, typeof(NoContentResult));
        _mockRepository.Verify(r => r.CancelFriendRequest(userId, receiverId), Times.Once);
    }

    [TestMethod]
    public async Task CancelFriendRequest_WhenRepoThrowsInvalidOperation_ReturnsNotFound()
    {
        var userId = "user-1";
        var receiverId = "user-2";

        _mockRepository
            .Setup(r => r.CancelFriendRequest(userId, receiverId))
            .ThrowsAsync(new InvalidOperationException("Pending friend request not found"));

        var result = await _controller.CancelFriendRequest(userId, receiverId);

        Assert.IsInstanceOfType(result, typeof(NotFoundObjectResult));
    }

    // EditAPost

    [TestMethod]
    public async Task EditAPost_Success_ReturnsOkWithEditedPost()
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

        _mockRepository
            .Setup(r => r.EditAPost(It.IsAny<Post>()))
            .ReturnsAsync(new Post
            {
                Id = "post-1",
                UserId = "user-42",
                PostTitle = "New title",
                PostContent = "New content",
                FitnessClassId = "3",
                WorkoutId = "4"
            });

        var result = await _controller.EditAPost(inputPost);

        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        _mockRepository.Verify(r => r.EditAPost(It.IsAny<Post>()), Times.Once);
    }

    [TestMethod]
    public async Task EditAPost_WhenRepoThrowsKeyNotFound_ReturnsNotFound()
    {
        _mockRepository
            .Setup(r => r.EditAPost(It.IsAny<Post>()))
            .ThrowsAsync(new KeyNotFoundException("Post not found or access denied"));

        var result = await _controller.EditAPost(new Post { Id = "post-1" });

        Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
    }

    // EditComment

    [TestMethod]
    public async Task EditComment_Success_ReturnsOk()
    {
        var postId = "post-123";
        var inputComment = new Comment
        {
            Id = "comment-1",
            AuthorId = "user-1",
            CommentDate = DateTime.UtcNow,
            CommentText = "Edited text"
        };

        _mockRepository
            .Setup(r => r.EditComment(postId, inputComment))
            .ReturnsAsync(new Post { Id = postId, UserId = "user-1" });

        var result = await _controller.EditComment(postId, inputComment);

        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        _mockRepository.Verify(r => r.EditComment(postId, inputComment), Times.Once);
    }

    [TestMethod]
    public async Task EditComment_WhenRepoThrowsKeyNotFound_ReturnsNotFound()
    {
        var postId = "post-123";
        var inputComment = new Comment { Id = "comment-1", CommentText = "Edited text" };

        _mockRepository
            .Setup(r => r.EditComment(postId, inputComment))
            .ThrowsAsync(new KeyNotFoundException("Post not found or comment not found"));

        var result = await _controller.EditComment(postId, inputComment);

        Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
    }
}
