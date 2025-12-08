using MongoDB.Driver;
using SocialService.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;


namespace SocialService.Repositories;

public class FriendshipRepository : IFriendshipRepository
{
    private readonly IMongoCollection<Friendship> FriendshipCollection;
    
    public FriendshipRepository(IConfiguration configuration)
    {
        // Henter MongoDB connection string fra appsettings.json
        var connectionString = configuration.GetConnectionString("MongoDb");
        var mongoClient = new MongoClient(connectionString);

        // Database navn
        var database = mongoClient.GetDatabase("SocialServiceDb");

        // Collection navn
        FriendshipCollection = database.GetCollection<Friendship>("Friendships");
    }

    public async Task<Friendship> SendFriendRequestAsync(int senderId, int receiverId)
    {

        //Vi tjekker for, at se om de har en aktiv friendrequest.
        var existingFriendshipRequest = await FriendshipCollection
            .Find(friendship => (friendship.SenderId == senderId && friendship.ReceiverId == receiverId)
                                || (friendship.ReceiverId == senderId && friendship.SenderId == receiverId))
            .FirstOrDefaultAsync();
        
        if (existingFriendshipRequest != null) 
        {
            throw new Exception("Friendship request already exists"); 
        }
        
        var friendship = new Friendship
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                FriendShipStatus = FriendshipStatus.Pending,
            };
            
            await  FriendshipCollection.InsertOneAsync(friendship);
            return friendship;

    }
    
    public async Task<Friendship> DeclineFriendRequestAsync(int senderId, int receiverId)
    {

        //Vi tjekker for, at se om de har en aktiv friendrequest.
        var existingFriendshipRequest = await FriendshipCollection
            .Find(friendship => (friendship.SenderId == senderId && friendship.ReceiverId == receiverId)
                                || (friendship.ReceiverId == senderId && friendship.SenderId == receiverId))
            .FirstOrDefaultAsync();
        
        //Der findes ikke nogen friendship request
        if (existingFriendshipRequest == null)
        {
            throw new Exception("Friendship request not found");
        }

        //Der findes en request, og den bliver ændret til Declined istedet for pending
        if (existingFriendshipRequest.FriendShipStatus == FriendshipStatus.Pending)
        {
            var update = Builders<Friendship>.Update.Set(friendship => friendship.FriendShipStatus, FriendshipStatus.Declined);
             
            await FriendshipCollection.UpdateOneAsync(
                friendship => friendship.FriendshipId == existingFriendshipRequest.FriendshipId,
                update
            );
            
            existingFriendshipRequest.FriendShipStatus = FriendshipStatus.Declined;
            return existingFriendshipRequest;

        }
        
        //Der findes en FriendShipRequest, men den er enten accepteret eller declined.
        throw new Exception("Friendship request is not pending");
        
    }
}