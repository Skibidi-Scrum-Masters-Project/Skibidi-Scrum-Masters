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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

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

    // Friend requests

    [TestMethod]
    [DoNotParallelize]
    public async Task SendFriendRequestAsync_NoExisting_CreatesPending()
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
    public async Task SendFriendRequestAsync_WhenPendingExists_ThrowsInvalidOperationException()
    {
        var userId = "user-1";
        var receiverId = "user-2";

        await _friendships.InsertOneAsync(new Friendship
        {
            SenderId = receiverId,
            ReceiverId = userId,
            FriendShipStatus = FriendshipStatus.Pending
        });

        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            async () => await _repository.SendFriendRequestAsync(userId, receiverId));
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task SendFriendRequestAsync_WhenDeclinedExists_UpdatesToPending()
    {
        var userId = "user-1";
        var receiverId = "user-2";

        await _friendships.InsertOneAsync(new Friendship
        {
            SenderId = userId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Declined
        });

        var result = await _repository.SendFriendRequestAsync(userId, receiverId);

        Assert.AreEqual(FriendshipStatus.Pending, result.FriendShipStatus);

        var stored = await _friendships
            .Find(f => f.SenderId == userId && f.ReceiverId == receiverId)
            .SingleOrDefaultAsync();

        Assert.IsNotNull(stored);
        Assert.AreEqual(FriendshipStatus.Pending, stored.FriendShipStatus);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task DeclineFriendRequestAsync_WhenPendingExists_UpdatesToDeclined()
    {
        var senderId = "user-1";
        var receiverId = "user-2";

        await _friendships.InsertOneAsync(new Friendship
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Pending
        });

        var result = await _repository.DeclineFriendRequestAsync(receiverId, senderId);

        Assert.AreEqual(FriendshipStatus.Declined, result.FriendShipStatus);

        var stored = await _friendships
            .Find(f => f.SenderId == senderId && f.ReceiverId == receiverId)
            .SingleOrDefaultAsync();

        Assert.IsNotNull(stored);
        Assert.AreEqual(FriendshipStatus.Declined, stored.FriendShipStatus);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task AcceptFriendRequest_WhenPendingExists_UpdatesToAccepted()
    {
        var senderId = "user-1";
        var receiverId = "user-2";

        await _friendships.InsertOneAsync(new Friendship
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Pending
        });

        var result = await _repository.AcceptFriendRequest(senderId, receiverId);

        Assert.IsNotNull(result);
        Assert.AreEqual(FriendshipStatus.Accepted, result!.FriendShipStatus);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task CancelFriendRequest_WhenPendingExists_DeletesIt()
    {
        var userId = "user-1";
        var receiverId = "user-2";

        await _friendships.InsertOneAsync(new Friendship
        {
            SenderId = userId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Pending
        });

        var deleted = await _repository.CancelFriendRequest(userId, receiverId);

        Assert.IsNotNull(deleted);

        var stillThere = await _friendships
            .Find(f => f.SenderId == userId && f.ReceiverId == receiverId)
            .FirstOrDefaultAsync();

        Assert.IsNull(stillThere);
    }

    // Core queries

    [TestMethod]
    [DoNotParallelize]
    public async Task GetAllFriends_ReturnsOnlyAcceptedForUser()
    {
        var userId = "user-1";

        await _friendships.InsertManyAsync(new[]
        {
            new Friendship { SenderId = userId, ReceiverId = "user-2", FriendShipStatus = FriendshipStatus.Accepted },
            new Friendship { SenderId = "user-3", ReceiverId = userId, FriendShipStatus = FriendshipStatus.Accepted },
            new Friendship { SenderId = userId, ReceiverId = "user-4", FriendShipStatus = FriendshipStatus.Pending },
            new Friendship { SenderId = "user-5", ReceiverId = userId, FriendShipStatus = FriendshipStatus.Declined }
        });

        var result = (await _repository.GetAllFriends(userId)).ToList();

        Assert.AreEqual(2, result.Count);
        Assert.IsTrue(result.All(f => f!.FriendShipStatus == FriendshipStatus.Accepted));
        Assert.IsTrue(result.All(f => f!.SenderId == userId || f!.ReceiverId == userId));
    }


    [TestMethod]
    [DoNotParallelize]
    public async Task GetAllIncommingFriendRequests_WhenFound_ReturnsAList()
    {
        //Arrange
        var  userId = "user-1";
        
        await _friendships.InsertManyAsync(new[]
        {
            new Friendship { SenderId = userId, ReceiverId = "user-2", FriendShipStatus = FriendshipStatus.Pending },
            new Friendship { SenderId = "user-3", ReceiverId = userId, FriendShipStatus = FriendshipStatus.Pending },
            new Friendship { SenderId = userId, ReceiverId = "user-4", FriendShipStatus = FriendshipStatus.Pending },
            new Friendship { SenderId = "user-5", ReceiverId = userId, FriendShipStatus = FriendshipStatus.Pending }
        });
        
        //Act
        var result = await _repository.GetAllIncomingFriendRequests(userId);
       var resultAsList = result.ToList();
        

       //Assert
       Assert.AreEqual(2, resultAsList.Count);
       Assert.IsNotNull(resultAsList);
       Assert.IsTrue(result.All(f => f!.FriendShipStatus == FriendshipStatus.Pending));
    }
    
    [TestMethod]
    [DoNotParallelize]
    public async Task GetAllIncommingFriendRequests_WhenThereIsNone_ReturnsAEmptyList()
    {
        //Arrange
        var  userId = "user-1";
        
        await _friendships.InsertManyAsync(new[]
        {
            new Friendship { SenderId = "user-5", ReceiverId = "user-2", FriendShipStatus = FriendshipStatus.Pending },
            new Friendship { SenderId = "user-3", ReceiverId = "user-5", FriendShipStatus = FriendshipStatus.Pending },
            new Friendship { SenderId = "user-1", ReceiverId = "user-4", FriendShipStatus = FriendshipStatus.Pending },
            new Friendship { SenderId = "user-5", ReceiverId = "user-7", FriendShipStatus = FriendshipStatus.Pending }
        });
        
        //Act
        var result = await _repository.GetAllIncomingFriendRequests(userId);
        var resultAsList = result.ToList();
        

        //Assert
        Assert.AreEqual(0, resultAsList.Count);
        Assert.IsNotNull(resultAsList);
    }
    
    [TestMethod]
    [DoNotParallelize]
    public async Task GetAllIncomingFriendRequests_WhenUserIsReceiver_ReturnsOnlyPendingWhereReceiverIsUser()
    {
        var userId = "user-1";

        await _friendships.InsertManyAsync(new[]
        {
            new Friendship { SenderId = "user-2", ReceiverId = userId, FriendShipStatus = FriendshipStatus.Pending },
            new Friendship { SenderId = "user-3", ReceiverId = userId, FriendShipStatus = FriendshipStatus.Accepted },
            new Friendship { SenderId = "user-4", ReceiverId = userId, FriendShipStatus = FriendshipStatus.Declined },
            new Friendship { SenderId = "user-5", ReceiverId = "user-x", FriendShipStatus = FriendshipStatus.Pending }
        });

        var result = await _repository.GetAllIncomingFriendRequests(userId);
        var list = result!.ToList();

        Assert.AreEqual(1, list.Count);
        var req = list.Single();
        Assert.AreEqual(userId, req.ReceiverId);
        Assert.AreEqual(FriendshipStatus.Pending, req.FriendShipStatus);

    }


    // Posts

    [TestMethod]
    [DoNotParallelize]
    public async Task PostAPost_InsertsPost_WithUtcNowDate_AndEmptyComments()
    {
        var input = new Post
        {
            UserId = "user-1",
            FitnessClassId = "class-2",
            WorkoutId = "workout-3",
            PostTitle = "Some title",
            PostContent = "Some content"
        };

        var before = DateTime.UtcNow;
        var created = await _repository.PostAPost(input);
        var after = DateTime.UtcNow;

        Assert.IsNotNull(created);
        Assert.IsTrue(created.PostDate >= before && created.PostDate <= after);
        Assert.IsNotNull(created.Comments);
        Assert.AreEqual(0, created.Comments.Count);

        var stored = await _posts.Find(p => p.Id == created.Id).FirstOrDefaultAsync();
        Assert.IsNotNull(stored);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task EditAPost_WhenOwnerMatches_UpdatesFields()
    {
        var postId = ObjectId.GenerateNewId().ToString();
        var userId = "user-42";

        await _posts.InsertOneAsync(new Post
        {
            Id = postId,
            UserId = userId,
            FitnessClassId = "class-1",
            WorkoutId = "workout-2",
            PostTitle = "Old title",
            PostContent = "Old content",
            PostDate = new DateTime(2020, 1, 1),
            Comments = new List<Comment>()
        });

        var updated = await _repository.EditAPost(new Post
        {
            Id = postId,
            UserId = userId,
            FitnessClassId = "class-10",
            WorkoutId = "workout-20",
            PostTitle = "New title",
            PostContent = "New content"
        });

        Assert.AreEqual("New title", updated.PostTitle);
        Assert.AreEqual("New content", updated.PostContent);
        Assert.AreEqual("class-10", updated.FitnessClassId);
        Assert.AreEqual("workout-20", updated.WorkoutId);
    }

    [TestMethod]
    [DoNotParallelize]
    public async Task AddCommentToPost_WhenPostExists_AppendsComment()
    {
        var postId = ObjectId.GenerateNewId().ToString();

        await _posts.InsertOneAsync(new Post
        {
            Id = postId,
            UserId = "user-1",
            PostTitle = "Title",
            PostContent = "Content",
            PostDate = new DateTime(2020, 1, 1),
            Comments = new List<Comment>()
        });

        var result = await _repository.AddCommentToPost(postId, new Comment
        {
            AuthorId = "user-99",
            CommentText = "New comment"
        });

        Assert.IsNotNull(result.Comments);
        Assert.AreEqual(1, result.Comments.Count);
        Assert.AreEqual("New comment", result.Comments[0].CommentText);
        Assert.IsFalse(string.IsNullOrWhiteSpace(result.Comments[0].Id));
    }
    
    [TestMethod]
    [DoNotParallelize]
    public async Task CreateDraftFromClassWorkoutCompletedAsync_WhenCalledTwice_ReturnsNullSecondTime()
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

        var first = await _repository.CreateDraftFromClassWorkoutCompletedAsync(dto);
        var second = await _repository.CreateDraftFromClassWorkoutCompletedAsync(dto);

        Assert.IsFalse(string.IsNullOrWhiteSpace(first));
        Assert.IsNull(second);

        var count = await _posts.Find(p => p.SourceEventId == eventId).CountDocumentsAsync();
        Assert.AreEqual(1, (int)count);
    }
}
