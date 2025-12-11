using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mongo2Go;
using MongoDB.Driver;
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
        var collection = _database.GetCollection<Friendship>("Friendships");

        // Act
        var result = await _repository.SendFriendRequestAsync(userId, receiverId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(userId, result.SenderId);
        Assert.AreEqual(receiverId, result.ReceiverId);
        Assert.AreEqual(FriendshipStatus.Pending, result.FriendShipStatus);

        var stored = await collection
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
        var collection = _database.GetCollection<Friendship>("Friendships");

        var pending = new Friendship
        {
            SenderId = userId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Pending
        };

        await collection.InsertOneAsync(pending);

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
        var collection = _database.GetCollection<Friendship>("Friendships");

        var pending = new Friendship
        {
            SenderId = receiverId,
            ReceiverId = userId,
            FriendShipStatus = FriendshipStatus.Pending
        };

        await collection.InsertOneAsync(pending);

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
        var collection = _database.GetCollection<Friendship>("Friendships");

        var declined = new Friendship
        {
            SenderId = userId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Declined
        };

        await collection.InsertOneAsync(declined);

        // Act
        var result = await _repository.SendFriendRequestAsync(userId, receiverId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(FriendshipStatus.Pending, result.FriendShipStatus);

        var stored = await collection
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
        var collection = _database.GetCollection<Friendship>("Friendships");

        var accepted = new Friendship
        {
            SenderId = userId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Accepted
        };

        await collection.InsertOneAsync(accepted);

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
        var collection = _database.GetCollection<Friendship>("Friendships");

        var pending = new Friendship
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Pending
        };

        await collection.InsertOneAsync(pending);

        // Act
        var result = await _repository.DeclineFriendRequestAsync(receiverId, senderId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(FriendshipStatus.Declined, result.FriendShipStatus);

        var stored = await collection
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
        var collection = _database.GetCollection<Friendship>("Friendships");

        var accepted = new Friendship
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Accepted
        };

        await collection.InsertOneAsync(accepted);

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
        var collection = _database.GetCollection<Friendship>("Friendships");

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

        await collection.InsertManyAsync(new[]
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
        var collection = _database.GetCollection<Friendship>("Friendships");

        var accepted = new Friendship
        {
            SenderId = userId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Accepted
        };

        await collection.InsertOneAsync(accepted);

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
        var collection = _database.GetCollection<Friendship>("Friendships");

        var acceptedOpposite = new Friendship
        {
            SenderId = receiverId,
            ReceiverId = userId,
            FriendShipStatus = FriendshipStatus.Accepted
        };

        await collection.InsertOneAsync(acceptedOpposite);

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
        var collection = _database.GetCollection<Friendship>("Friendships");

        var pending = new Friendship
        {
            SenderId = userId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Pending
        };

        await collection.InsertOneAsync(pending);

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
        var collection = _database.GetCollection<Friendship>("Friendships");

        var pending = new Friendship
        {
            SenderId = userId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Pending
        };

        await collection.InsertOneAsync(pending);

        // Act
        var result = await _repository.CancelFriendRequest(userId, receiverId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(FriendshipStatus.None, result.FriendShipStatus);

        var stored = await collection
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
        var collection = _database.GetCollection<Friendship>("Friendships");

        var accepted = new Friendship
        {
            SenderId = userId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Accepted
        };

        await collection.InsertOneAsync(accepted);

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
        var collection = _database.GetCollection<Friendship>("Friendships");

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

        await collection.InsertManyAsync(new[] { pending, accepted, rejected });

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
        var collection = _database.GetCollection<Friendship>("Friendships");

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

        await collection.InsertManyAsync(new[]
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
        var collection = _database.GetCollection<Friendship>("Friendships");

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

        await collection.InsertManyAsync(new[]
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
        var collection = _database.GetCollection<Friendship>("Friendships");

        var pending = new Friendship
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Pending
        };

        await collection.InsertOneAsync(pending);

        // Act
        var result = await _repository.AcceptFriendRequest(senderId, receiverId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(FriendshipStatus.Accepted, result.FriendShipStatus);

        var stored = await collection
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
        var collection = _database.GetCollection<Friendship>("Friendships");

        var accepted = new Friendship
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Accepted
        };

        await collection.InsertOneAsync(accepted);

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
        var collection = _database.GetCollection<Friendship>("Friendships");

        var declined = new Friendship
        {
            SenderId = senderId,
            ReceiverId = receiverId,
            FriendShipStatus = FriendshipStatus.Declined
        };

        await collection.InsertOneAsync(declined);

        // Act + Assert
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            async () => await _repository.AcceptFriendRequest(senderId, receiverId));
    }
}