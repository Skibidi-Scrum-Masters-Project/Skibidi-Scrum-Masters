using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mongo2Go;
using MongoDB.Driver;
using MongoDB.Bson;
using SocialService.Models;
using SocialService.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SocialService.Tests;

[TestClass]
public class SocialRepositoryTests
{
    private static MongoDbRunner _runner = null!;
    private IMongoDatabase _database = null!;
    private SocialRepository _repository = null!;
    
    private IMongoCollection<Friendship> _friendships = null!;
    private IMongoCollection<Post> _posts = null!;

    [ClassInitialize]
    public static void ClassInit(TestContext _)
    {
        _runner = MongoDbRunner.Start();
    }

    [TestInitialize]
    public void TestInit()
    {
        var client = new MongoClient(_runner.ConnectionString);

        _database = client.GetDatabase("SocialServiceTests");
        _database.DropCollection("Friendships");
        _database.DropCollection("Posts");

        _friendships = _database.GetCollection<Friendship>("Friendships");
        _posts = _database.GetCollection<Post>("Posts");
        
        _repository = new SocialRepository(_database);
    }

    [ClassCleanup]
    public static void ClassCleanup()
    {
        _runner.Dispose();
    }

    // SendFriendRequestAsync

    [TestMethod]
    [DoNotParallelize]
    public async Task SendFriendRequestAsync_WhenNoExistingFriendship_ShouldCreatePendingRequest()
    {
        // Arrange
        var userId = 1;
        var receiverId = 2;

        // Act
        var result = await _repository.SendFriendRequestAsync(userId, receiverId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(userId, result.SenderId);
        Assert.AreEqual(receiverId, result.ReceiverId);
        Assert.AreEqual(FriendshipStatus.Pending, result.FriendShipStatus);

        var stored = await _friendships
            .Find(f => f.SenderId == userId && f.ReceiverId == receiverId)
            .SingleOrDefaultAsync();

        Assert.IsNotNull(stored);
        Assert.AreEqual(FriendshipStatus.Pending, stored.FriendShipStatus);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task SendFriendRequestAsync_WhenPendingExistsSameDirection_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var userId = 1;
        var receiverId = 2;

        var pending = new Friendship
        {
            SenderId = userId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Pending
        };

        await _friendships.InsertOneAsync(pending);

        // Act + Assert
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            async () => await _repository.SendFriendRequestAsync(userId, receiverId));
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task SendFriendRequestAsync_WhenPendingExistsOppositeDirection_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var userId = 1;
        var receiverId = 2;

        var pending = new Friendship
        {
            SenderId = receiverId,
            ReceiverId = userId,
            FriendShipStatus = FriendshipStatus.Pending
        };

        await _friendships.InsertOneAsync(pending);

        // Act + Assert
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            async () => await _repository.SendFriendRequestAsync(userId, receiverId));
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task SendFriendRequestAsync_WhenExistingDeclined_ShouldUpdateToPending()
    {
        // Arrange
        var userId = 1;
        var receiverId = 2;

        var declined = new Friendship
        {
            SenderId = userId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Declined
        };

        await _friendships.InsertOneAsync(declined);

        // Act
        var result = await _repository.SendFriendRequestAsync(userId, receiverId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(FriendshipStatus.Pending, result.FriendShipStatus);

        var stored = await _friendships
            .Find(f => f.SenderId == userId && f.ReceiverId == receiverId)
            .SingleOrDefaultAsync();

        Assert.IsNotNull(stored);
        Assert.AreEqual(FriendshipStatus.Pending, stored.FriendShipStatus);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task SendFriendRequestAsync_WhenExistingAccepted_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var userId = 1;
        var receiverId = 2;

        var accepted = new Friendship
        {
            SenderId = userId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Accepted
        };

        await _friendships.InsertOneAsync(accepted);

        // Act + Assert
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            async () => await _repository.SendFriendRequestAsync(userId, receiverId));
    }

    // DeclineFriendRequestAsync

    [TestMethod]
    [DoNotParallelize]
    public async Task DeclineFriendRequestAsync_WhenPendingExists_ShouldUpdateStatusToDeclined()
    {
        // Arrange
        var senderId = 1;
        var receiverId = 2;

        var pending = new Friendship
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Pending
        };

        await _friendships.InsertOneAsync(pending);

        // Act
        var result = await _repository.DeclineFriendRequestAsync(receiverId, senderId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(FriendshipStatus.Declined, result.FriendShipStatus);

        var stored = await _friendships
            .Find(f => f.SenderId == senderId && f.ReceiverId == receiverId)
            .SingleOrDefaultAsync();

        Assert.IsNotNull(stored);
        Assert.AreEqual(FriendshipStatus.Declined, stored.FriendShipStatus);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task DeclineFriendRequestAsync_WhenNoPendingExists_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var userId = 1;
        var receiverId = 2;

        // Act + Assert
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            async () => await _repository.DeclineFriendRequestAsync(userId, receiverId));
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task DeclineFriendRequestAsync_WhenStatusNotPending_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var senderId = 1;
        var receiverId = 2;

        var accepted = new Friendship
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Accepted
        };

        await _friendships.InsertOneAsync(accepted);

        // Act + Assert
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            async () => await _repository.DeclineFriendRequestAsync(receiverId, senderId));
    }

    // GetAllFriends

    [TestMethod]
    [DoNotParallelize]
    public async Task GetAllFriends_WhenMultipleStatuses_ShouldOnlyReturnAcceptedForUser()
    {
        // Arrange
        var userId = 1;

        var acceptedAsSender = new Friendship
        {
            SenderId = userId,
            ReceiverId = 2,
            FriendShipStatus = FriendshipStatus.Accepted
        };

        var acceptedAsReceiver = new Friendship
        {
            SenderId = 3,
            ReceiverId = userId,
            FriendShipStatus = FriendshipStatus.Accepted
        };

        var pending = new Friendship
        {
            SenderId = userId,
            ReceiverId = 4,
            FriendShipStatus = FriendshipStatus.Pending
        };

        var declined = new Friendship
        {
            SenderId = 5,
            ReceiverId = userId,
            FriendShipStatus = FriendshipStatus.Declined
        };

        var otherUser = new Friendship
        {
            SenderId = 10,
            ReceiverId = 11,
            FriendShipStatus = FriendshipStatus.Accepted
        };

        await _friendships.InsertManyAsync(new[]
        {
            acceptedAsSender,
            acceptedAsReceiver,
            pending,
            declined,
            otherUser
        });

        // Act
        var result = await _repository.GetAllFriends(userId);
        var list = result.ToList();

        // Assert
        Assert.AreEqual(2, list.Count);
        Assert.IsTrue(list.All(f => f.FriendShipStatus == FriendshipStatus.Accepted));
        Assert.IsTrue(list.All(f => f.SenderId == userId || f.ReceiverId == userId));
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task GetAllFriends_WhenNoFriends_ShouldReturnEmptyList()
    {
        // Arrange
        var userId = 1;

        // Act
        var result = await _repository.GetAllFriends(userId);
        var list = result.ToList();

        // Assert
        Assert.AreEqual(0, list.Count);
    }

    // GetFriendById

    [TestMethod]
    [DoNotParallelize]
    public async Task GetFriendById_WhenAcceptedExists_ShouldReturnFriendship()
    {
        // Arrange
        var userId = 1;
        var receiverId = 2;

        var accepted = new Friendship
        {
            SenderId = userId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Accepted
        };

        await _friendships.InsertOneAsync(accepted);

        // Act
        var result = await _repository.GetFriendById(userId, receiverId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(userId, result!.SenderId);
        Assert.AreEqual(receiverId, result.ReceiverId);
        Assert.AreEqual(FriendshipStatus.Accepted, result.FriendShipStatus);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task GetFriendById_WhenAcceptedExistsOppositeDirection_ShouldReturnNull()
    {
        // Arrange
        var userId = 1;
        var receiverId = 2;

        var acceptedOpposite = new Friendship
        {
            SenderId = receiverId,
            ReceiverId = userId,
            FriendShipStatus = FriendshipStatus.Accepted
        };

        await _friendships.InsertOneAsync(acceptedOpposite);

        // Act
        var result = await _repository.GetFriendById(userId, receiverId);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task GetFriendById_WhenStatusNotAccepted_ShouldReturnNull()
    {
        // Arrange
        var userId = 1;
        var receiverId = 2;

        var pending = new Friendship
        {
            SenderId = userId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Pending
        };

        await _friendships.InsertOneAsync(pending);

        // Act
        var result = await _repository.GetFriendById(userId, receiverId);

        // Assert
        Assert.IsNull(result);
    }

    // CancelFriendRequest

    [TestMethod]
    [DoNotParallelize]
    public async Task CancelFriendRequest_WhenPendingExists_ShouldUpdateStatusToNone()
    {
        // Arrange
        var userId = 1;
        var receiverId = 2;

        var pending = new Friendship
        {
            SenderId = userId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Pending
        };

        await _friendships.InsertOneAsync(pending);

        // Act
        var result = await _repository.CancelFriendRequest(userId, receiverId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(FriendshipStatus.None, result.FriendShipStatus);

        var stored = await _friendships
            .Find(f => f.SenderId == userId && f.ReceiverId == receiverId)
            .SingleOrDefaultAsync();

        Assert.IsNotNull(stored);
        Assert.AreEqual(FriendshipStatus.None, stored.FriendShipStatus);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task CancelFriendRequest_WhenNoPendingExists_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var userId = 1;
        var receiverId = 2;

        // Act + Assert
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            async () => await _repository.CancelFriendRequest(userId, receiverId));
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task CancelFriendRequest_WhenStatusNotPending_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var userId = 1;
        var receiverId = 2;

        var accepted = new Friendship
        {
            SenderId = userId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Accepted
        };

        await _friendships.InsertOneAsync(accepted);

        // Act + Assert
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            async () => await _repository.CancelFriendRequest(userId, receiverId));
    }

    // GetOutgoingFriendRequestsAsync
    // DINE TO TDD-TESTS BEHOLDES UDEN ÆNDRING

    [TestMethod]
    [DoNotParallelize]
    public async Task GetOutgoingFriendRequestsAsync_WhenThereIsMultipleStatus_ShouldNotReturnAcceptedOrDeclinedRequests()
    {
        // Arrange
        var userId = 1;

        var pending = new Friendship
        {
            SenderId = userId,
            ReceiverId = 2,
            FriendShipStatus = FriendshipStatus.Pending
        };

        var accepted = new Friendship
        {
            SenderId = userId,
            ReceiverId = 3,
            FriendShipStatus = FriendshipStatus.Accepted
        };

        var rejected = new Friendship
        {
            SenderId = userId,
            ReceiverId = 4,
            FriendShipStatus = FriendshipStatus.Declined
        };

        await _friendships.InsertManyAsync(new[] { pending, accepted, rejected });

        // Act
        var result = await _repository.GetOutgoingFriendRequestsAsync(userId);
        var list = result.ToList();

        // Assert
        Assert.AreEqual(1, list.Count, "Kun pending requests skal returneres");
        Assert.AreEqual(FriendshipStatus.Pending, list.Single().FriendShipStatus);
    }
    
    [TestMethod]
    [DoNotParallelize]
    public async Task GetOutgoingFriendRequestsAsync_WhenItsSuccessfull_ShouldReturnAllFriendRequests()
    {
        // Arrange
        var userId = 1;

        var shouldBeReturned1 = new Friendship
        {
            SenderId = userId,
            ReceiverId = 2,
            FriendShipStatus = FriendshipStatus.Pending
        };

        var shouldBeReturned2 = new Friendship
        {
            SenderId = userId,
            ReceiverId = 3,
            FriendShipStatus = FriendshipStatus.Pending
        };

        var otherSender = new Friendship
        {
            SenderId = userId,
            ReceiverId = 4,
            FriendShipStatus = FriendshipStatus.Pending
        };

        var otherStatus = new Friendship
        {
            SenderId = userId,
            ReceiverId = 5,
            FriendShipStatus = FriendshipStatus.Pending
        };

        await _friendships.InsertManyAsync(new[]
        {
            shouldBeReturned1,
            shouldBeReturned2,
            otherSender,
            otherStatus
        });

        // Act
        var result = await _repository.GetOutgoingFriendRequestsAsync(userId);
        var list = result.ToList();

        // Assert
        Assert.IsTrue(list.All(f => f.SenderId == userId),
            "Alle resultater skal have samme SenderId som i testen");

        Assert.IsTrue(list.All(f => f.FriendShipStatus == FriendshipStatus.Pending),
            "Alle resultater skal have status Pending");
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task GetOutgoingFriendRequestsAsync_WhenNoRequestsExist_ShouldReturnEmptyList()
    {
        // Arrange
        var userId = 1;

        // Act
        var result = await _repository.GetOutgoingFriendRequestsAsync(userId);
        var list = result.ToList();

        // Assert
        Assert.AreEqual(0, list.Count);
    }

    // GetAllIncomingFriendRequests

    [TestMethod]
    [DoNotParallelize]
    public async Task GetAllIncomingFriendRequests_WhenThereIsMultipleStatuses_ShouldOnlyReturnPendingForUser()
    {
        // Arrange
        var userId = 1;

        var pendingForUser = new Friendship
        {
            SenderId = 2,
            ReceiverId = userId,
            FriendShipStatus = FriendshipStatus.Pending
        };

        var acceptedForUser = new Friendship
        {
            SenderId = 3,
            ReceiverId = userId,
            FriendShipStatus = FriendshipStatus.Accepted
        };

        var declinedForUser = new Friendship
        {
            SenderId = 4,
            ReceiverId = userId,
            FriendShipStatus = FriendshipStatus.Declined
        };

        var pendingOtherUser = new Friendship
        {
            SenderId = 5,
            ReceiverId = 6,
            FriendShipStatus = FriendshipStatus.Pending
        };

        await _friendships.InsertManyAsync(new[]
        {
            pendingForUser,
            acceptedForUser,
            declinedForUser,
            pendingOtherUser
        });

        // Act
        var result = await _repository.GetAllIncomingFriendRequests(userId);
        var list = result.ToList();

        // Assert
        Assert.AreEqual(1, list.Count);
        Assert.IsTrue(list.All(f => f.ReceiverId == userId));
        Assert.IsTrue(list.All(f => f.FriendShipStatus == FriendshipStatus.Pending));
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task GetAllIncomingFriendRequests_WhenNoRequestsExist_ShouldReturnEmptyList()
    {
        // Arrange
        var userId = 1;

        // Act
        var result = await _repository.GetAllIncomingFriendRequests(userId);
        var list = result.ToList();

        // Assert
        Assert.AreEqual(0, list.Count);
    }

    // AcceptFriendRequest

    [TestMethod]
    [DoNotParallelize]
    public async Task AcceptFriendRequest_WhenPendingExists_ShouldUpdateToAccepted()
    {
        // Arrange
        var senderId = 1;
        var receiverId = 2;

        var pending = new Friendship
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Pending
        };

        await _friendships.InsertOneAsync(pending);

        // Act
        var result = await _repository.AcceptFriendRequest(senderId, receiverId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(FriendshipStatus.Accepted, result.FriendShipStatus);

        var stored = await _friendships
            .Find(f => f.SenderId == senderId && f.ReceiverId == receiverId)
            .SingleOrDefaultAsync();

        Assert.IsNotNull(stored);
        Assert.AreEqual(FriendshipStatus.Accepted, stored.FriendShipStatus);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task AcceptFriendRequest_WhenNoRequestExists_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var senderId = 1;
        var receiverId = 2;

        // Act + Assert
        await Assert.ThrowsExceptionAsync<KeyNotFoundException>(
            async () => await _repository.AcceptFriendRequest(senderId, receiverId));
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task AcceptFriendRequest_WhenStatusIsAccepted_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var senderId = 1;
        var receiverId = 2;

        var accepted = new Friendship
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Accepted
        };

        await _friendships.InsertOneAsync(accepted);

        // Act + Assert
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            async () => await _repository.AcceptFriendRequest(senderId, receiverId));
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task AcceptFriendRequest_WhenStatusIsDeclined_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var senderId = 1;
        var receiverId = 2;
        var declined = new Friendship
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Declined
        };

        await _friendships.InsertOneAsync(declined);

        // Act + Assert
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            async () => await _repository.AcceptFriendRequest(senderId, receiverId));
    }
    
    
    
    //Post Tests
    
    //PostAPost
    [TestMethod]
    [DoNotParallelize]
    public async Task PostAPost_inserts_new_post_and_returns_it()
    {
        // Arrange


        var inputPost = new Post
        {
            UserId = 1,
            FitnessClassId = 2,
            WorkoutId = 3,
            PostTitle = "Some title",
            PostContent = "Some content",
            PostDate = new DateTime(2000, 1, 1) // bliver overskrevet af DateTime.UtcNow i metoden
        };

        var before = DateTime.UtcNow;

        // Act
        var result = await _repository.PostAPost(inputPost);

        var after = DateTime.UtcNow;

        // Assert - metoden returnerer noget
        Assert.IsNotNull(result);

        // Tjek at den er mappet korrekt
        Assert.AreEqual(inputPost.UserId, result.UserId);
        Assert.AreEqual(inputPost.FitnessClassId, result.FitnessClassId);
        Assert.AreEqual(inputPost.WorkoutId, result.WorkoutId);
        Assert.AreEqual(inputPost.PostTitle, result.PostTitle);
        Assert.AreEqual(inputPost.PostContent, result.PostContent);

        // DateTime.UtcNow er brugt
        Assert.IsTrue(result.PostDate >= before && result.PostDate <= after, "PostDate skal ligge mellem before og after");

        // Comments er initialiseret
        Assert.IsNotNull(result.Comments);
        Assert.AreEqual(0, result.Comments.Count);

        // Tjek at posten rent faktisk er gemt i databasen
        var stored = await _posts
            .Find(p => p.Id == result.Id)
            .SingleOrDefaultAsync();

        Assert.IsNotNull(stored);
        Assert.AreEqual(result.UserId, stored.UserId);
        Assert.AreEqual(result.FitnessClassId, stored.FitnessClassId);
        Assert.AreEqual(result.WorkoutId, stored.WorkoutId);
        Assert.AreEqual(result.PostTitle, stored.PostTitle);
        Assert.AreEqual(result.PostContent, stored.PostContent);
    }
    
        // RemoveAPost

        [TestMethod]
        [DoNotParallelize]
        public async Task RemoveAPost_WhenPostExists_ShouldDeleteAndReturnExistingPost()
        {
            // Arrange
            var postId = ObjectId.GenerateNewId().ToString();

            var existingPost = new Post
            {
                Id = postId,
                UserId = 1,
                FitnessClassId = 2,
                WorkoutId = 3,
                PostTitle = "Title to be deleted",
                PostContent = "Content to be deleted",
                PostDate = new DateTime(2020, 1, 1)
            };

            await _posts.InsertOneAsync(existingPost);

            var before = await _posts
                .Find(p => p.Id == postId)
                .SingleOrDefaultAsync();
            Assert.IsNotNull(before, "Posten skal eksistere før RemoveAPost kaldes");

            // Act
            var result = await _repository.RemoveAPost(postId);

            // Assert
            Assert.IsNotNull(result, "Metoden skal returnere den fundne post");
            Assert.AreEqual(postId, result.Id);
            Assert.AreEqual(existingPost.UserId, result.UserId);
            Assert.AreEqual(existingPost.FitnessClassId, result.FitnessClassId);
            Assert.AreEqual(existingPost.WorkoutId, result.WorkoutId);
            Assert.AreEqual(existingPost.PostTitle, result.PostTitle);
            Assert.AreEqual(existingPost.PostContent, result.PostContent);

            var stored = await _posts
                .Find(p => p.Id == postId)
                .SingleOrDefaultAsync();

            Assert.IsNull(stored, "Posten skal være slettet fra databasen");
        }

        [TestMethod]
        [DoNotParallelize]
        public async Task RemoveAPost_WhenPostDoesNotExist_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var nonExistingPostId = ObjectId.GenerateNewId().ToString();

            // For en sikkerheds skyld, tjek at der ikke ligger en post med det id
            var existing = await _posts
                .Find(p => p.Id == nonExistingPostId)
                .SingleOrDefaultAsync();
            Assert.IsNull(existing, "Der må ikke eksistere en post med dette id i testen");

            // Act + Assert
            await Assert.ThrowsExceptionAsync<KeyNotFoundException>(
                async () => await _repository.RemoveAPost(nonExistingPostId));
        }
        
            // EditAPost

    [TestMethod]
    [DoNotParallelize]
    public async Task EditAPost_WhenPostExistsAndBelongsToUser_ShouldUpdateAndReturnUpdatedPost()
    {
        // Arrange
        var postId = ObjectId.GenerateNewId().ToString();
        var userId = 42;

        var existingPost = new Post
        {
            Id = postId,
            UserId = userId,
            FitnessClassId = 1,
            WorkoutId = 2,
            PostTitle = "Old title",
            PostContent = "Old content",
            PostDate = new DateTime(2020, 1, 1),
            Comments = new List<Comment>()
        };

        await _posts.InsertOneAsync(existingPost);

        var updatedInput = new Post
        {
            Id = postId,
            UserId = 999, // bliver ikke brugt direkte, currentUserId styrer filteret
            FitnessClassId = 10,
            WorkoutId = 20,
            PostTitle = "New title",
            PostContent = "New content"
        };

        // Act
        var result = await _repository.EditAPost(updatedInput, userId);

        // Assert - returværdi
        Assert.IsNotNull(result);
        Assert.AreEqual(postId, result.Id);
        Assert.AreEqual(userId, result.UserId);
        Assert.AreEqual(updatedInput.FitnessClassId, result.FitnessClassId);
        Assert.AreEqual(updatedInput.WorkoutId, result.WorkoutId);
        Assert.AreEqual(updatedInput.PostTitle, result.PostTitle);
        Assert.AreEqual(updatedInput.PostContent, result.PostContent);

        // Tjek at den faktisk er opdateret i databasen
        var stored = await _posts
            .Find(p => p.Id == postId)
            .SingleOrDefaultAsync();

        Assert.IsNotNull(stored);
        Assert.AreEqual(userId, stored.UserId);
        Assert.AreEqual(updatedInput.FitnessClassId, stored.FitnessClassId);
        Assert.AreEqual(updatedInput.WorkoutId, stored.WorkoutId);
        Assert.AreEqual(updatedInput.PostTitle, stored.PostTitle);
        Assert.AreEqual(updatedInput.PostContent, stored.PostContent);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task EditAPost_WhenPostDoesNotExist_ShouldThrowKeyNotFoundException()
    {
        // Arrange
        var nonExistingPostId = ObjectId.GenerateNewId().ToString();
        var userId = 42;

        var input = new Post
        {
            Id = nonExistingPostId,
            PostTitle = "Does not matter",
            PostContent = "Does not matter"
        };

        var existing = await _posts
            .Find(p => p.Id == nonExistingPostId)
            .SingleOrDefaultAsync();
        Assert.IsNull(existing, "Der må ikke eksistere en post med dette id i testen");

        // Act + Assert
        await Assert.ThrowsExceptionAsync<KeyNotFoundException>(
            async () => await _repository.EditAPost(input, userId));
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task EditAPost_WhenPostBelongsToOtherUser_ShouldThrowKeyNotFoundException_AndNotChangePost()
    {
        // Arrange
        var postId = ObjectId.GenerateNewId().ToString();
        var ownerUserId = 1;
        var otherUserId = 2;

        var existingPost = new Post
        {
            Id = postId,
            UserId = ownerUserId,
            FitnessClassId = 1,
            WorkoutId = 2,
            PostTitle = "Original title",
            PostContent = "Original content",
            PostDate = new DateTime(2020, 1, 1)
        };

        await _posts.InsertOneAsync(existingPost);

        var attemptedUpdate = new Post
        {
            Id = postId,
            UserId = otherUserId, // forsøger at "snyde", men filter bruger currentUserId
            FitnessClassId = 99,
            WorkoutId = 88,
            PostTitle = "Hacked title",
            PostContent = "Hacked content"
        };

        // Act + Assert
        await Assert.ThrowsExceptionAsync<KeyNotFoundException>(
            async () => await _repository.EditAPost(attemptedUpdate, otherUserId));

        // Posten skal stadig være uændret i databasen
        var stored = await _posts
            .Find(p => p.Id == postId)
            .SingleOrDefaultAsync();

        Assert.IsNotNull(stored);
        Assert.AreEqual(existingPost.UserId, stored.UserId);
        Assert.AreEqual(existingPost.FitnessClassId, stored.FitnessClassId);
        Assert.AreEqual(existingPost.WorkoutId, stored.WorkoutId);
        Assert.AreEqual(existingPost.PostTitle, stored.PostTitle);
        Assert.AreEqual(existingPost.PostContent, stored.PostContent);
    }
    
    
    //Comment Test
    [TestMethod]
    [DoNotParallelize]
    public async Task AddCommentToPost_WhenPostExists_ShouldAppendCommentAndReturnUpdatedPost()
    {
        // Arrange
        var postId = ObjectId.GenerateNewId().ToString();

        var existingPost = new Post
        {
            Id = postId,
            UserId = 1,
            FitnessClassId = 2,
            WorkoutId = 3,
            PostTitle = "Original title",
            PostContent = "Original content",
            PostDate = new DateTime(2020, 1, 1),
            Comments = new List<Comment>()
        };

        await _posts.InsertOneAsync(existingPost);

        var newComment = new Comment
        {
            Id = ObjectId.GenerateNewId().ToString(),
            AuthorId = 99,
            CommentDate = DateTime.UtcNow,
            CommentText = "New comment"
        };

        // Act
        var result = await _repository.AddCommentToPost(postId, newComment);

        // Assert på returværdi
        Assert.IsNotNull(result, "Method should return updated post");
        Assert.AreEqual(postId, result.Id);
        Assert.IsNotNull(result.Comments);
        Assert.AreEqual(1, result.Comments.Count);
        Assert.AreEqual(newComment.CommentText, result.Comments[0].CommentText);
        Assert.AreEqual(newComment.AuthorId, result.Comments[0].AuthorId);

        // Tjek at det også er gemt i databasen
        var stored = await _posts
            .Find(p => p.Id == postId)
            .SingleOrDefaultAsync();

        Assert.IsNotNull(stored, "Post should still exist in database");
        Assert.IsNotNull(stored.Comments);
        Assert.AreEqual(1, stored.Comments.Count);
        Assert.AreEqual(newComment.CommentText, stored.Comments[0].CommentText);
        Assert.AreEqual(newComment.AuthorId, stored.Comments[0].AuthorId);
    }
    
    
    //RemoveComment Test
    [TestMethod]
    [DoNotParallelize]
    public async Task RemoveCommentFromPost_WhenPostAndCommentExist_RemovesCommentAndReturnsUpdatedPost()
    {
        // Arrange
        var postId = ObjectId.GenerateNewId().ToString();

        var commentToRemove = new Comment
        {
            Id = ObjectId.GenerateNewId().ToString(),
            AuthorId = 1,
            CommentDate = DateTime.UtcNow,
            CommentText = "Delete me"
        };

        var otherComment = new Comment
        {
            Id = ObjectId.GenerateNewId().ToString(),
            AuthorId = 2,
            CommentDate = DateTime.UtcNow,
            CommentText = "Keep me"
        };

        var existingPost = new Post
        {
            Id = postId,
            UserId = 1,
            FitnessClassId = 2,
            WorkoutId = 3,
            PostTitle = "Title",
            PostContent = "Content",
            PostDate = new DateTime(2020, 1, 1),
            Comments = new List<Comment> { commentToRemove, otherComment }
        };

        await _posts.InsertOneAsync(existingPost);

        // Act
        var result = await _repository.RemoveCommentFromPost(postId, commentToRemove.Id!);

        // Assert på returværdi
        Assert.IsNotNull(result);
        Assert.AreEqual(postId, result.Id);
        Assert.IsNotNull(result.Comments);
        Assert.AreEqual(1, result.Comments.Count);
        Assert.AreEqual(otherComment.Id, result.Comments[0].Id);

        // Assert i databasen
        var stored = await _posts
            .Find(p => p.Id == postId)
            .SingleOrDefaultAsync();

        Assert.IsNotNull(stored);
        Assert.IsNotNull(stored.Comments);
        Assert.AreEqual(1, stored.Comments.Count);
        Assert.AreEqual(otherComment.Id, stored.Comments[0].Id);
    }
    
    
    // EditComment tests

[TestMethod]
[DoNotParallelize]
public async Task EditComment_WhenPostDoesNotExist_ShouldThrowKeyNotFoundException()
{
    // Arrange
    var postId = ObjectId.GenerateNewId().ToString();

    var commentToEdit = new Comment
    {
        Id = ObjectId.GenerateNewId().ToString(),
        AuthorId = 1,
        CommentDate = DateTime.UtcNow,
        CommentText = "New text"
    };

    var existing = await _posts
        .Find(p => p.Id == postId)
        .SingleOrDefaultAsync();
    Assert.IsNull(existing, "There must not be a post with this id");

    // Act + Assert
    await Assert.ThrowsExceptionAsync<KeyNotFoundException>(
        async () => await _repository.EditComment(postId, commentToEdit));
}

[TestMethod]
[DoNotParallelize]
public async Task EditComment_WhenCommentDoesNotExistOnPost_ShouldThrowKeyNotFoundException()
{
    // Arrange
    var postId = ObjectId.GenerateNewId().ToString();

    var existingPost = new Post
    {
        Id = postId,
        UserId = 1,
        FitnessClassId = 2,
        WorkoutId = 3,
        PostTitle = "Title",
        PostContent = "Content",
        PostDate = new DateTime(2020, 1, 1),
        Comments = new List<Comment>
        {
            new Comment
            {
                Id = ObjectId.GenerateNewId().ToString(),
                AuthorId = 1,
                CommentDate = DateTime.UtcNow,
                CommentText = "Existing comment"
            }
        }
    };

    await _posts.InsertOneAsync(existingPost);

    var nonExistingComment = new Comment
    {
        Id = ObjectId.GenerateNewId().ToString(),
        AuthorId = 1,
        CommentDate = DateTime.UtcNow,
        CommentText = "Should not be found"
    };

    // Act + Assert
    await Assert.ThrowsExceptionAsync<KeyNotFoundException>(
        async () => await _repository.EditComment(postId, nonExistingComment));
}

[TestMethod]
[DoNotParallelize]
public async Task EditComment_WhenPostAndCommentExist_ShouldUpdateCommentTextAndReturnUpdatedPost()
{
    // Arrange
    var postId = ObjectId.GenerateNewId().ToString();

    var commentId = ObjectId.GenerateNewId().ToString();

    var originalComment = new Comment
    {
        Id = commentId,
        AuthorId = 1,
        CommentDate = new DateTime(2020, 1, 1),
        CommentText = "Old text"
    };

    var otherComment = new Comment
    {
        Id = ObjectId.GenerateNewId().ToString(),
        AuthorId = 2,
        CommentDate = new DateTime(2020, 1, 2),
        CommentText = "Other comment"
    };

    var existingPost = new Post
    {
        Id = postId,
        UserId = 1,
        FitnessClassId = 2,
        WorkoutId = 3,
        PostTitle = "Title",
        PostContent = "Content",
        PostDate = new DateTime(2020, 1, 1),
        Comments = new List<Comment> { originalComment, otherComment }
    };

    await _posts.InsertOneAsync(existingPost);

    var updatedComment = new Comment
    {
        Id = commentId,
        AuthorId = originalComment.AuthorId,
        CommentDate = originalComment.CommentDate,
        CommentText = "New edited text"
    };

    // Act
    var result = await _repository.EditComment(postId, updatedComment);

    // Assert returværdi
    Assert.IsNotNull(result);
    Assert.AreEqual(postId, result.Id);
    Assert.IsNotNull(result.Comments);
    Assert.AreEqual(2, result.Comments.Count);

    var editedFromResult = result.Comments.Single(c => c.Id == commentId);
    Assert.AreEqual("New edited text", editedFromResult.CommentText);
    Assert.AreEqual(originalComment.AuthorId, editedFromResult.AuthorId);

    var otherFromResult = result.Comments.Single(c => c.Id == otherComment.Id);
    Assert.AreEqual(otherComment.CommentText, otherFromResult.CommentText);

    // Assert i databasen
    var stored = await _posts
        .Find(p => p.Id == postId)
        .SingleOrDefaultAsync();

    Assert.IsNotNull(stored);
    Assert.IsNotNull(stored.Comments);
    Assert.AreEqual(2, stored.Comments.Count);

    var editedInDb = stored.Comments.Single(c => c.Id == commentId);
    Assert.AreEqual("New edited text", editedInDb.CommentText);

    var otherInDb = stored.Comments.Single(c => c.Id == otherComment.Id);
    Assert.AreEqual(otherComment.CommentText, otherInDb.CommentText);
}





}