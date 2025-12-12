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
using Mongo2Go;
using MongoDB.Driver;
using FitnessApp.Shared.Models;

namespace SocialService.Tests;

[TestClass]
public class SocialControllerTests
{
    private MongoDbRunner _runner = null!;
    private IMongoDatabase _database = null!;
    private SocialController _controller = null!;
    private Mock<ISocialRepository> _mockRepository = null!;

    [TestInitialize]
    public void Setup()
    {
        _runner = MongoDbRunner.Start();
        var client = new MongoClient(_runner.ConnectionString);
        _database = client.GetDatabase("TestDatabase");

        _mockRepository = new Mock<ISocialRepository>();
        _controller = new SocialController(_mockRepository.Object, _database);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _runner.Dispose();
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

    // --------------------------
    // NEW: Internal event tests
    // --------------------------

    [TestMethod]
    public async Task ClassWorkoutCompleted_ShouldReturnBadRequest_WhenPayloadInvalid()
    {
        var dto = new ClassResultEventDto(
            EventId: "",
            ClassId: "class-1",
            UserId: "user-1",
            CaloriesBurned: 100,
            Watt: 200,
            DurationMin: 60,
            Date: DateTime.UtcNow
        );

        var result = await _controller.ClassWorkoutCompleted(dto);

        var bad = result as BadRequestObjectResult;
        Assert.IsNotNull(bad, "Expected BadRequestObjectResult");
        Assert.AreEqual(StatusCodes.Status400BadRequest, bad.StatusCode);
    }

    [TestMethod]
    public async Task ClassWorkoutCompleted_ShouldInsertDraftPost_AndReturnOkWithDraftId()
    {
        var eventId = Guid.NewGuid().ToString();

        var dto = new ClassResultEventDto(
            EventId: eventId,
            ClassId: "class-1",
            UserId: "user-1",
            CaloriesBurned: 123.4,
            Watt: 250.0,
            DurationMin: 60,
            Date: DateTime.UtcNow
        );

        var result = await _controller.ClassWorkoutCompleted(dto);

        var ok = result as OkObjectResult;
        Assert.IsNotNull(ok, "Expected OkObjectResult");
        Assert.AreEqual(StatusCodes.Status200OK, ok.StatusCode);

        var posts = _database.GetCollection<Post>("Posts");
        var inserted = await posts.Find(p => p.SourceEventId == dto.EventId).FirstOrDefaultAsync();

        Assert.IsNotNull(inserted, "Expected a Post inserted in MongoDB");
        Assert.AreEqual(dto.UserId, inserted.UserId);
        Assert.AreEqual(dto.ClassId, inserted.FitnessClassId);
        Assert.AreEqual(dto.EventId, inserted.SourceEventId);
        Assert.IsTrue(inserted.IsDraft, "Expected IsDraft = true");
        Assert.AreEqual(PostType.Workout, inserted.Type);

        Assert.IsNotNull(inserted.WorkoutStats, "Expected WorkoutStatsSnapshot to be set");
        Assert.AreEqual(dto.DurationMin * 60, inserted.WorkoutStats.DurationSeconds);
        Assert.AreEqual((int)Math.Round(dto.CaloriesBurned), inserted.WorkoutStats.Calories);
    }

    [TestMethod]
    public async Task ClassWorkoutCompleted_ShouldDedupe_WhenSameEventIdSentTwice()
    {
        var eventId = Guid.NewGuid().ToString();

        var dto = new ClassResultEventDto(
            EventId: eventId,
            ClassId: "class-1",
            UserId: "user-1",
            CaloriesBurned: 100,
            Watt: 200,
            DurationMin: 60,
            Date: DateTime.UtcNow
        );

        var first = await _controller.ClassWorkoutCompleted(dto);
        var second = await _controller.ClassWorkoutCompleted(dto);

        Assert.IsTrue(first is OkResult or OkObjectResult, "Expected Ok on first call");
        Assert.IsTrue(second is OkResult or OkObjectResult, "Expected Ok on second call");

        var posts = _database.GetCollection<Post>("Posts");
        var count = await posts.Find(p => p.SourceEventId == eventId).CountDocumentsAsync();

        Assert.AreEqual(1, (int)count, "Expected only one post for the same SourceEventId");
    }

    // --------------------------
    // Existing tests (updated only where controller constructor changed)
    // --------------------------

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

        var controller = new SocialController(repoMock.Object, _database);

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

    // Comment tests (unchanged except controller already has db)

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
