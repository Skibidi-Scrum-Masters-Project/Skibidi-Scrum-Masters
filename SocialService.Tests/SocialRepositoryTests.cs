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
        var userId = "user-1";
        var receiverId = "user-2";

        var result = await _repository.SendFriendRequestAsync(userId, receiverId);

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
        var userId = "user-1";
        var receiverId = "user-2";

        var pending = new Friendship
        {
            SenderId = userId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Pending
        };

        await _friendships.InsertOneAsync(pending);

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            async () => await _repository.SendFriendRequestAsync(userId, receiverId));
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task SendFriendRequestAsync_WhenPendingExistsOppositeDirection_ShouldThrowInvalidOperationException()
    {
        var userId = "user-1";
        var receiverId = "user-2";

        var pending = new Friendship
        {
            SenderId = receiverId,
            ReceiverId = userId,
            FriendShipStatus = FriendshipStatus.Pending
        };

        await _friendships.InsertOneAsync(pending);

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            async () => await _repository.SendFriendRequestAsync(userId, receiverId));
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task SendFriendRequestAsync_WhenExistingDeclined_ShouldUpdateToPending()
    {
        var userId = "user-1";
        var receiverId = "user-2";

        var declined = new Friendship
        {
            SenderId = userId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Declined
        };

        await _friendships.InsertOneAsync(declined);

        var result = await _repository.SendFriendRequestAsync(userId, receiverId);

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
        var userId = "user-1";
        var receiverId = "user-2";

        var accepted = new Friendship
        {
            SenderId = userId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Accepted
        };

        await _friendships.InsertOneAsync(accepted);

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            async () => await _repository.SendFriendRequestAsync(userId, receiverId));
    }

    // DeclineFriendRequestAsync

    [TestMethod]
    [DoNotParallelize]
    public async Task DeclineFriendRequestAsync_WhenPendingExists_ShouldUpdateStatusToDeclined()
    {
        var senderId = "user-1";
        var receiverId = "user-2";

        var pending = new Friendship
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Pending
        };

        await _friendships.InsertOneAsync(pending);

        // bemærk: du kalder med (receiverId, senderId) i din test
        var result = await _repository.DeclineFriendRequestAsync(receiverId, senderId);

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
        var userId = "user-1";
        var receiverId = "user-2";

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            async () => await _repository.DeclineFriendRequestAsync(userId, receiverId));
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task DeclineFriendRequestAsync_WhenStatusNotPending_ShouldThrowInvalidOperationException()
    {
        var senderId = "user-1";
        var receiverId = "user-2";

        var accepted = new Friendship
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Accepted
        };

        await _friendships.InsertOneAsync(accepted);

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            async () => await _repository.DeclineFriendRequestAsync(receiverId, senderId));
    }

    // GetAllFriends

    [TestMethod]
    [DoNotParallelize]
    public async Task GetAllFriends_WhenMultipleStatuses_ShouldOnlyReturnAcceptedForUser()
    {
        var userId = "user-1";

        var acceptedAsSender = new Friendship
        {
            SenderId = userId,
            ReceiverId = "user-2",
            FriendShipStatus = FriendshipStatus.Accepted
        };

        var acceptedAsReceiver = new Friendship
        {
            SenderId = "user-3",
            ReceiverId = userId,
            FriendShipStatus = FriendshipStatus.Accepted
        };

        var pending = new Friendship
        {
            SenderId = userId,
            ReceiverId = "user-4",
            FriendShipStatus = FriendshipStatus.Pending
        };

        var declined = new Friendship
        {
            SenderId = "user-5",
            ReceiverId = userId,
            FriendShipStatus = FriendshipStatus.Declined
        };

        var otherUser = new Friendship
        {
            SenderId = "user-10",
            ReceiverId = "user-11",
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

        var result = await _repository.GetAllFriends(userId);
        var list = result.ToList();

        Assert.AreEqual(2, list.Count);
        Assert.IsTrue(list.All(f => f.FriendShipStatus == FriendshipStatus.Accepted));
        Assert.IsTrue(list.All(f => f.SenderId == userId || f.ReceiverId == userId));
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task GetAllFriends_WhenNoFriends_ShouldReturnEmptyList()
    {
        var userId = "user-1";

        var result = await _repository.GetAllFriends(userId);
        var list = result.ToList();

        Assert.AreEqual(0, list.Count);
    }

    // GetFriendById

    [TestMethod]
    [DoNotParallelize]
    public async Task GetFriendById_WhenAcceptedExists_ShouldReturnFriendship()
    {
        var userId = "user-1";
        var receiverId = "user-2";

        var accepted = new Friendship
        {
            SenderId = userId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Accepted
        };

        await _friendships.InsertOneAsync(accepted);

        var result = await _repository.GetFriendById(userId, receiverId);

        Assert.IsNotNull(result);
        Assert.AreEqual(userId, result!.SenderId);
        Assert.AreEqual(receiverId, result.ReceiverId);
        Assert.AreEqual(FriendshipStatus.Accepted, result.FriendShipStatus);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task GetFriendById_WhenAcceptedExistsOppositeDirection_ShouldReturnNull()
    {
        var userId = "user-1";
        var receiverId = "user-2";

        var acceptedOpposite = new Friendship
        {
            SenderId = receiverId,
            ReceiverId = userId,
            FriendShipStatus = FriendshipStatus.Accepted
        };

        await _friendships.InsertOneAsync(acceptedOpposite);

        var result = await _repository.GetFriendById(userId, receiverId);

        Assert.IsNull(result);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task GetFriendById_WhenStatusNotAccepted_ShouldReturnNull()
    {
        var userId = "user-1";
        var receiverId = "user-2";

        var pending = new Friendship
        {
            SenderId = userId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Pending
        };

        await _friendships.InsertOneAsync(pending);

        var result = await _repository.GetFriendById(userId, receiverId);

        Assert.IsNull(result);
    }

    // CancelFriendRequest

    [TestMethod]
    [DoNotParallelize]
    public async Task CancelFriendRequest_WhenPendingExists_ShouldUpdateStatusToNone()
    {
        var userId = "user-1";
        var receiverId = "user-2";

        var pending = new Friendship
        {
            SenderId = userId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Pending
        };

        await _friendships.InsertOneAsync(pending);

        var result = await _repository.CancelFriendRequest(userId, receiverId);

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
        var userId = "user-1";
        var receiverId = "user-2";

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            async () => await _repository.CancelFriendRequest(userId, receiverId));
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task CancelFriendRequest_WhenStatusNotPending_ShouldThrowInvalidOperationException()
    {
        var userId = "user-1";
        var receiverId = "user-2";

        var accepted = new Friendship
        {
            SenderId = userId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Accepted
        };

        await _friendships.InsertOneAsync(accepted);

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            async () => await _repository.CancelFriendRequest(userId, receiverId));
    }

    // GetOutgoingFriendRequestsAsync

    [TestMethod]
    [DoNotParallelize]
    public async Task GetOutgoingFriendRequestsAsync_WhenThereIsMultipleStatus_ShouldNotReturnAcceptedOrDeclinedRequests()
    {
        var userId = "user-1";

        var pending = new Friendship
        {
            SenderId = userId,
            ReceiverId = "user-2",
            FriendShipStatus = FriendshipStatus.Pending
        };

        var accepted = new Friendship
        {
            SenderId = userId,
            ReceiverId = "user-3",
            FriendShipStatus = FriendshipStatus.Accepted
        };

        var rejected = new Friendship
        {
            SenderId = userId,
            ReceiverId = "user-4",
            FriendShipStatus = FriendshipStatus.Declined
        };

        await _friendships.InsertManyAsync(new[] { pending, accepted, rejected });

        var result = await _repository.GetOutgoingFriendRequestsAsync(userId);
        var list = result.ToList();

        Assert.AreEqual(1, list.Count, "Kun pending requests skal returneres");
        Assert.AreEqual(FriendshipStatus.Pending, list.Single().FriendShipStatus);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task GetOutgoingFriendRequestsAsync_WhenItsSuccessfull_ShouldReturnAllFriendRequests()
    {
        var userId = "user-1";

        var shouldBeReturned1 = new Friendship
        {
            SenderId = userId,
            ReceiverId = "user-2",
            FriendShipStatus = FriendshipStatus.Pending
        };

        var shouldBeReturned2 = new Friendship
        {
            SenderId = userId,
            ReceiverId = "user-3",
            FriendShipStatus = FriendshipStatus.Pending
        };

        var otherSender = new Friendship
        {
            SenderId = userId,
            ReceiverId = "user-4",
            FriendShipStatus = FriendshipStatus.Pending
        };

        var otherStatus = new Friendship
        {
            SenderId = userId,
            ReceiverId = "user-5",
            FriendShipStatus = FriendshipStatus.Pending
        };

        await _friendships.InsertManyAsync(new[]
        {
            shouldBeReturned1,
            shouldBeReturned2,
            otherSender,
            otherStatus
        });

        var result = await _repository.GetOutgoingFriendRequestsAsync(userId);
        var list = result.ToList();

        Assert.IsTrue(list.All(f => f.SenderId == userId),
            "Alle resultater skal have samme SenderId som i testen");

        Assert.IsTrue(list.All(f => f.FriendShipStatus == FriendshipStatus.Pending),
            "Alle resultater skal have status Pending");
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task GetOutgoingFriendRequestsAsync_WhenNoRequestsExist_ShouldReturnEmptyList()
    {
        var userId = "user-1";

        var result = await _repository.GetOutgoingFriendRequestsAsync(userId);
        var list = result.ToList();

        Assert.AreEqual(0, list.Count);
    }

    // GetAllIncomingFriendRequests

    [TestMethod]
    [DoNotParallelize]
    public async Task GetAllIncomingFriendRequests_WhenThereIsMultipleStatuses_ShouldOnlyReturnPendingForUser()
    {
        var userId = "user-1";

        var pendingForUser = new Friendship
        {
            SenderId = "user-2",
            ReceiverId = userId,
            FriendShipStatus = FriendshipStatus.Pending
        };

        var acceptedForUser = new Friendship
        {
            SenderId = "user-3",
            ReceiverId = userId,
            FriendShipStatus = FriendshipStatus.Accepted
        };

        var declinedForUser = new Friendship
        {
            SenderId = "user-4",
            ReceiverId = userId,
            FriendShipStatus = FriendshipStatus.Declined
        };

        var pendingOtherUser = new Friendship
        {
            SenderId = "user-5",
            ReceiverId = "user-6",
            FriendShipStatus = FriendshipStatus.Pending
        };

        await _friendships.InsertManyAsync(new[]
        {
            pendingForUser,
            acceptedForUser,
            declinedForUser,
            pendingOtherUser
        });

        var result = await _repository.GetAllIncomingFriendRequests(userId);
        var list = result.ToList();

        Assert.AreEqual(1, list.Count);
        Assert.IsTrue(list.All(f => f.ReceiverId == userId));
        Assert.IsTrue(list.All(f => f.FriendShipStatus == FriendshipStatus.Pending));
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task GetAllIncomingFriendRequests_WhenNoRequestsExist_ShouldReturnEmptyList()
    {
        var userId = "user-1";

        var result = await _repository.GetAllIncomingFriendRequests(userId);
        var list = result.ToList();

        Assert.AreEqual(0, list.Count);
    }

    // AcceptFriendRequest

    [TestMethod]
    [DoNotParallelize]
    public async Task AcceptFriendRequest_WhenPendingExists_ShouldUpdateToAccepted()
    {
        var senderId = "user-1";
        var receiverId = "user-2";

        var pending = new Friendship
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Pending
        };

        await _friendships.InsertOneAsync(pending);

        var result = await _repository.AcceptFriendRequest(senderId, receiverId);

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
        var senderId = "user-1";
        var receiverId = "user-2";

        await Assert.ThrowsExceptionAsync<KeyNotFoundException>(
            async () => await _repository.AcceptFriendRequest(senderId, receiverId));
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task AcceptFriendRequest_WhenStatusIsAccepted_ShouldThrowInvalidOperationException()
    {
        var senderId = "user-1";
        var receiverId = "user-2";

        var accepted = new Friendship
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Accepted
        };

        await _friendships.InsertOneAsync(accepted);

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            async () => await _repository.AcceptFriendRequest(senderId, receiverId));
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task AcceptFriendRequest_WhenStatusIsDeclined_ShouldThrowInvalidOperationException()
    {
        var senderId = "user-1";
        var receiverId = "user-2";

        var declined = new Friendship
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Declined
        };

        await _friendships.InsertOneAsync(declined);

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            async () => await _repository.AcceptFriendRequest(senderId, receiverId));
    }

    // Post Tests

    [TestMethod]
    [DoNotParallelize]
    public async Task PostAPost_inserts_new_post_and_returns_it()
    {
        var inputPost = new Post
        {
            UserId = "user-1",
            FitnessClassId = "class-2",
            WorkoutId = "workout-3",
            PostTitle = "Some title",
            PostContent = "Some content",
            PostDate = new DateTime(2000, 1, 1)
        };

        var before = DateTime.UtcNow;

        var result = await _repository.PostAPost(inputPost);

        var after = DateTime.UtcNow;

        Assert.IsNotNull(result);

        Assert.AreEqual(inputPost.UserId, result.UserId);
        Assert.AreEqual(inputPost.FitnessClassId, result.FitnessClassId);
        Assert.AreEqual(inputPost.WorkoutId, result.WorkoutId);
        Assert.AreEqual(inputPost.PostTitle, result.PostTitle);
        Assert.AreEqual(inputPost.PostContent, result.PostContent);

        Assert.IsTrue(result.PostDate >= before && result.PostDate <= after,
            "PostDate skal ligge mellem before og after");

        Assert.IsNotNull(result.Comments);
        Assert.AreEqual(0, result.Comments.Count);

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

    [TestMethod]
    [DoNotParallelize]
    public async Task RemoveAPost_WhenPostExists_ShouldDeleteAndReturnExistingPost()
    {
        var postId = ObjectId.GenerateNewId().ToString();

        var existingPost = new Post
        {
            Id = postId,
            UserId = "user-1",
            FitnessClassId = "class-2",
            WorkoutId = "workout-3",
            PostTitle = "Title to be deleted",
            PostContent = "Content to be deleted",
            PostDate = new DateTime(2020, 1, 1)
        };

        await _posts.InsertOneAsync(existingPost);

        var before = await _posts
            .Find(p => p.Id == postId)
            .SingleOrDefaultAsync();
        Assert.IsNotNull(before, "Posten skal eksistere før RemoveAPost kaldes");

        var result = await _repository.RemoveAPost(postId);

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
        var nonExistingPostId = ObjectId.GenerateNewId().ToString();

        var existing = await _posts
            .Find(p => p.Id == nonExistingPostId)
            .SingleOrDefaultAsync();
        Assert.IsNull(existing, "Der må ikke eksistere en post med dette id i testen");

        await Assert.ThrowsExceptionAsync<KeyNotFoundException>(
            async () => await _repository.RemoveAPost(nonExistingPostId));
    }

    // EditAPost

    [TestMethod]
    [DoNotParallelize]
    public async Task EditAPost_WhenPostExistsAndBelongsToUser_ShouldUpdateAndReturnUpdatedPost()
    {
        var postId = ObjectId.GenerateNewId().ToString();
        var userId = "user-42";

        var existingPost = new Post
        {
            Id = postId,
            UserId = userId,
            FitnessClassId = "class-1",
            WorkoutId = "workout-2",
            PostTitle = "Old title",
            PostContent = "Old content",
            PostDate = new DateTime(2020, 1, 1),
            Comments = new List<Comment>()
        };

        await _posts.InsertOneAsync(existingPost);

        var updatedInput = new Post
        {
            Id = postId,
            UserId = "user-999",
            FitnessClassId = "class-10",
            WorkoutId = "workout-20",
            PostTitle = "New title",
            PostContent = "New content"
        };

        var result = await _repository.EditAPost(updatedInput, userId);

        Assert.IsNotNull(result);
        Assert.AreEqual(postId, result.Id);
        Assert.AreEqual(userId, result.UserId);
        Assert.AreEqual(updatedInput.FitnessClassId, result.FitnessClassId);
        Assert.AreEqual(updatedInput.WorkoutId, result.WorkoutId);
        Assert.AreEqual(updatedInput.PostTitle, result.PostTitle);
        Assert.AreEqual(updatedInput.PostContent, result.PostContent);

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
        var nonExistingPostId = ObjectId.GenerateNewId().ToString();
        var userId = "user-42";

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

        await Assert.ThrowsExceptionAsync<KeyNotFoundException>(
            async () => await _repository.EditAPost(input, userId));
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task EditAPost_WhenPostBelongsToOtherUser_ShouldThrowKeyNotFoundException_AndNotChangePost()
    {
        var postId = ObjectId.GenerateNewId().ToString();
        var ownerUserId = "user-1";
        var otherUserId = "user-2";

        var existingPost = new Post
        {
            Id = postId,
            UserId = ownerUserId,
            FitnessClassId = "class-1",
            WorkoutId = "workout-2",
            PostTitle = "Original title",
            PostContent = "Original content",
            PostDate = new DateTime(2020, 1, 1)
        };

        await _posts.InsertOneAsync(existingPost);

        var attemptedUpdate = new Post
        {
            Id = postId,
            UserId = otherUserId,
            FitnessClassId = "class-99",
            WorkoutId = "workout-88",
            PostTitle = "Hacked title",
            PostContent = "Hacked content"
        };

        await Assert.ThrowsExceptionAsync<KeyNotFoundException>(
            async () => await _repository.EditAPost(attemptedUpdate, otherUserId));

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

    // Comment Test

    [TestMethod]
    [DoNotParallelize]
    public async Task AddCommentToPost_WhenPostExists_ShouldAppendCommentAndReturnUpdatedPost()
    {
        var postId = ObjectId.GenerateNewId().ToString();

        var existingPost = new Post
        {
            Id = postId,
            UserId = "user-1",
            FitnessClassId = "class-2",
            WorkoutId = "workout-3",
            PostTitle = "Original title",
            PostContent = "Original content",
            PostDate = new DateTime(2020, 1, 1),
            Comments = new List<Comment>()
        };

        await _posts.InsertOneAsync(existingPost);

        var newComment = new Comment
        {
            Id = ObjectId.GenerateNewId().ToString(),
            AuthorId = "user-99",
            CommentDate = DateTime.UtcNow,
            CommentText = "New comment"
        };

        var result = await _repository.AddCommentToPost(postId, newComment);

        Assert.IsNotNull(result, "Method should return updated post");
        Assert.AreEqual(postId, result.Id);
        Assert.IsNotNull(result.Comments);
        Assert.AreEqual(1, result.Comments.Count);
        Assert.AreEqual(newComment.CommentText, result.Comments[0].CommentText);
        Assert.AreEqual(newComment.AuthorId, result.Comments[0].AuthorId);

        var stored = await _posts
            .Find(p => p.Id == postId)
            .SingleOrDefaultAsync();

        Assert.IsNotNull(stored, "Post should still exist in database");
        Assert.IsNotNull(stored.Comments);
        Assert.AreEqual(1, stored.Comments.Count);
        Assert.AreEqual(newComment.CommentText, stored.Comments[0].CommentText);
        Assert.AreEqual(newComment.AuthorId, stored.Comments[0].AuthorId);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task RemoveCommentFromPost_WhenPostAndCommentExist_RemovesCommentAndReturnsUpdatedPost()
    {
        var postId = ObjectId.GenerateNewId().ToString();

        var commentToRemove = new Comment
        {
            Id = ObjectId.GenerateNewId().ToString(),
            AuthorId = "user-1",
            CommentDate = DateTime.UtcNow,
            CommentText = "Delete me"
        };

        var otherComment = new Comment
        {
            Id = ObjectId.GenerateNewId().ToString(),
            AuthorId = "user-2",
            CommentDate = DateTime.UtcNow,
            CommentText = "Keep me"
        };

        var existingPost = new Post
        {
            Id = postId,
            UserId = "user-1",
            FitnessClassId = "class-2",
            WorkoutId = "workout-3",
            PostTitle = "Title",
            PostContent = "Content",
            PostDate = new DateTime(2020, 1, 1),
            Comments = new List<Comment> { commentToRemove, otherComment }
        };

        await _posts.InsertOneAsync(existingPost);

        var result = await _repository.RemoveCommentFromPost(postId, commentToRemove.Id!);

        Assert.IsNotNull(result);
        Assert.AreEqual(postId, result.Id);
        Assert.IsNotNull(result.Comments);
        Assert.AreEqual(1, result.Comments.Count);
        Assert.AreEqual(otherComment.Id, result.Comments[0].Id);

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
        var postId = ObjectId.GenerateNewId().ToString();

        var commentToEdit = new Comment
        {
            Id = ObjectId.GenerateNewId().ToString(),
            AuthorId = "user-1",
            CommentDate = DateTime.UtcNow,
            CommentText = "New text"
        };

        var existing = await _posts
            .Find(p => p.Id == postId)
            .SingleOrDefaultAsync();
        Assert.IsNull(existing, "There must not be a post with this id");

        await Assert.ThrowsExceptionAsync<KeyNotFoundException>(
            async () => await _repository.EditComment(postId, commentToEdit));
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task EditComment_WhenCommentDoesNotExistOnPost_ShouldThrowKeyNotFoundException()
    {
        var postId = ObjectId.GenerateNewId().ToString();

        var existingPost = new Post
        {
            Id = postId,
            UserId = "user-1",
            FitnessClassId = "class-2",
            WorkoutId = "workout-3",
            PostTitle = "Title",
            PostContent = "Content",
            PostDate = new DateTime(2020, 1, 1),
            Comments = new List<Comment>
            {
                new Comment
                {
                    Id = ObjectId.GenerateNewId().ToString(),
                    AuthorId = "user-1",
                    CommentDate = DateTime.UtcNow,
                    CommentText = "Existing comment"
                }
            }
        };

        await _posts.InsertOneAsync(existingPost);

        var nonExistingComment = new Comment
        {
            Id = ObjectId.GenerateNewId().ToString(),
            AuthorId = "user-1",
            CommentDate = DateTime.UtcNow,
            CommentText = "Should not be found"
        };

        await Assert.ThrowsExceptionAsync<KeyNotFoundException>(
            async () => await _repository.EditComment(postId, nonExistingComment));
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task EditComment_WhenPostAndCommentExist_ShouldUpdateCommentTextAndReturnUpdatedPost()
    {
        var postId = ObjectId.GenerateNewId().ToString();

        var commentId = ObjectId.GenerateNewId().ToString();

        var originalComment = new Comment
        {
            Id = commentId,
            AuthorId = "user-1",
            CommentDate = new DateTime(2020, 1, 1),
            CommentText = "Old text"
        };

        var otherComment = new Comment
        {
            Id = ObjectId.GenerateNewId().ToString(),
            AuthorId = "user-2",
            CommentDate = new DateTime(2020, 1, 2),
            CommentText = "Other comment"
        };

        var existingPost = new Post
        {
            Id = postId,
            UserId = "user-1",
            FitnessClassId = "class-2",
            WorkoutId = "workout-3",
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

        var result = await _repository.EditComment(postId, updatedComment);

        Assert.IsNotNull(result);
        Assert.AreEqual(postId, result.Id);
        Assert.IsNotNull(result.Comments);
        Assert.AreEqual(2, result.Comments.Count);

        var editedFromResult = result.Comments.Single(c => c.Id == commentId);
        Assert.AreEqual("New edited text", editedFromResult.CommentText);
        Assert.AreEqual(originalComment.AuthorId, editedFromResult.AuthorId);

        var otherFromResult = result.Comments.Single(c => c.Id == otherComment.Id);
        Assert.AreEqual(otherComment.CommentText, otherFromResult.CommentText);

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

    [TestMethod]
    [DoNotParallelize]
    public async Task SeeAllCommentForPostId_WhenPostExistsWithComments_ReturnsAllComments()
    {
        var postId = ObjectId.GenerateNewId().ToString();

        var comment1 = new Comment
        {
            Id = ObjectId.GenerateNewId().ToString(),
            AuthorId = "user-1",
            CommentDate = new DateTime(2020, 1, 1),
            CommentText = "First comment"
        };

        var comment2 = new Comment
        {
            Id = ObjectId.GenerateNewId().ToString(),
            AuthorId = "user-2",
            CommentDate = new DateTime(2020, 1, 2),
            CommentText = "Second comment"
        };

        var post = new Post
        {
            Id = postId,
            UserId = "user-10",
            FitnessClassId = "class-20",
            WorkoutId = "workout-30",
            PostTitle = "Some title",
            PostContent = "Some content",
            PostDate = new DateTime(2020, 1, 1),
            Comments = new List<Comment> { comment1, comment2 }
        };

        await _posts.InsertOneAsync(post);

        var result = await _repository.SeeAllCommentForPostId(postId);
        var list = result.ToList();

        Assert.IsNotNull(result, "Method should return a list, not null");
        Assert.AreEqual(2, list.Count, "Expected exactly 2 comments");

        Assert.IsTrue(list.Any(c => c.Id == comment1.Id && c.CommentText == comment1.CommentText));
        Assert.IsTrue(list.Any(c => c.Id == comment2.Id && c.CommentText == comment2.CommentText));
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task SeeAllPostsForUser_WhenUserHasMultiplePosts_ReturnsOnlyThosePosts()
    {
        var userId = "user-1";
        var otherUserId = "user-2";

        var post1 = new Post
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = userId,
            FitnessClassId = "class-10",
            WorkoutId = "workout-100",
            PostTitle = "User1 post 1",
            PostContent = "Content 1",
            PostDate = new DateTime(2020, 1, 1),
            Comments = new List<Comment>()
        };

        var post2 = new Post
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = userId,
            FitnessClassId = "class-11",
            WorkoutId = "workout-101",
            PostTitle = "User1 post 2",
            PostContent = "Content 2",
            PostDate = new DateTime(2020, 1, 2),
            Comments = new List<Comment>()
        };

        var otherUsersPost = new Post
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = otherUserId,
            FitnessClassId = "class-20",
            WorkoutId = "workout-200",
            PostTitle = "Other user post",
            PostContent = "Other content",
            PostDate = new DateTime(2020, 1, 3),
            Comments = new List<Comment>()
        };

        await _posts.InsertManyAsync(new[] { post1, post2, otherUsersPost });

        var result = await _repository.SeeAllPostsForUser(userId);
        var list = result.ToList();

        Assert.IsNotNull(result, "Method should not return null");
        Assert.AreEqual(2, list.Count, "Expected exactly the two posts for this user");
        Assert.IsTrue(list.All(p => p.UserId == userId), "All returned posts must belong to the requested user");
        Assert.IsTrue(list.Any(p => p.Id == post1.Id));
        Assert.IsTrue(list.Any(p => p.Id == post2.Id));
        Assert.IsFalse(list.Any(p => p.Id == otherUsersPost.Id), "Posts from other users must not be returned");
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task SeeAllPostsForUser_WhenUserHasNoPosts_ReturnsEmptyList()
    {
        var userId = "user-999";

        var otherPost = new Post
        {
            Id = ObjectId.GenerateNewId().ToString(),
            UserId = "user-1",
            FitnessClassId = "class-10",
            WorkoutId = "workout-100",
            PostTitle = "Other users post",
            PostContent = "Content",
            PostDate = new DateTime(2020, 1, 1),
            Comments = new List<Comment>()
        };

        await _posts.InsertOneAsync(otherPost);

        var result = await _repository.SeeAllPostsForUser(userId);
        var list = result.ToList();

        Assert.IsNotNull(result, "Method should not return null");
        Assert.AreEqual(0, list.Count, "Expected no posts for this user");
    }
    
    
    [TestMethod]
    [DoNotParallelize]
    public async Task CreateDraftFromClassWorkoutCompletedAsync_WhenCalledTwiceWithSameEventId_ShouldInsertOnceAndReturnNullSecondTime()
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

        // first call should create draft and return id
        var firstDraftId = await _repository.CreateDraftFromClassWorkoutCompletedAsync(dto);

        Assert.IsFalse(string.IsNullOrWhiteSpace(firstDraftId), "Expected a draft id on first call");

        var inserted = await _posts.Find(p => p.SourceEventId == eventId).FirstOrDefaultAsync();

        Assert.IsNotNull(inserted, "Expected a Post inserted in MongoDB");
        Assert.AreEqual(dto.UserId, inserted.UserId);
        Assert.AreEqual(dto.ClassId, inserted.FitnessClassId);
        Assert.AreEqual(dto.ClassId, inserted.WorkoutId);
        Assert.AreEqual(eventId, inserted.SourceEventId);
        Assert.IsTrue(inserted.IsDraft, "Expected IsDraft = true");
        Assert.AreEqual(PostType.Workout, inserted.Type);

        Assert.IsNotNull(inserted.WorkoutStats, "Expected WorkoutStatsSnapshot to be set");
        Assert.AreEqual(dto.DurationMin * 60, inserted.WorkoutStats.DurationSeconds);
        Assert.AreEqual((int)Math.Round(dto.CaloriesBurned), inserted.WorkoutStats.Calories);

        // second call should dedupe and return null
        var secondDraftId = await _repository.CreateDraftFromClassWorkoutCompletedAsync(dto);

        Assert.IsNull(secondDraftId, "Expected null on second call because of dedupe");

        var count = await _posts.Find(p => p.SourceEventId == eventId).CountDocumentsAsync();
        Assert.AreEqual(1, (int)count, "Expected only one post for the same SourceEventId");
    }

}
