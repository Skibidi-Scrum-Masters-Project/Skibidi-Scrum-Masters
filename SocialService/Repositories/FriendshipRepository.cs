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
    
}