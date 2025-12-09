using MongoDB.Driver;
using SocialService.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;


namespace SocialService.Repositories;

public class FriendshipRepository : IFriendshipRepository
{
    private readonly IMongoCollection<Friendship> _friendshipCollection;
    
    public FriendshipRepository(IMongoDatabase database)
    {
       
        _friendshipCollection = database.GetCollection<Friendship>("Friendships"); 
    }

    public async Task<Friendship> SendFriendRequestAsync(int senderId, int receiverId)
    {

        //Vi tjekker for, at se om de har en aktiv friendrequest.
        var existingFriendship = await _friendshipCollection
            .Find(f => (f.SenderId == senderId && f.ReceiverId == receiverId)
                                || (f.ReceiverId == senderId && f.SenderId == receiverId))
            .FirstOrDefaultAsync();

        
        if (existingFriendship != null)
        {
            //Tjekker om der allerede er en pending FriendRequest
            if (existingFriendship.FriendShipStatus == FriendshipStatus.Pending)
            {
                throw new Exception("Friendship request already exists");
            }
            
            if (existingFriendship.FriendShipStatus == FriendshipStatus.Declined)
            {
                //Hvis pending FriendRequest er Declined, laves der en ny update, hvor Status bliver sat tilbage til pending.
                var updateExistingFriendship = Builders<Friendship>.Update
                    .Set(f => f.FriendShipStatus, FriendshipStatus.Pending);
                
                await _friendshipCollection.UpdateOneAsync(
                    f => f.FriendshipId == existingFriendship.FriendshipId, updateExistingFriendship);
                
                
                existingFriendship.FriendShipStatus = FriendshipStatus.Pending;
                return existingFriendship;
            }
            throw new InvalidOperationException("Friendship already exists");
        }
        
        var friendship = new Friendship
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                FriendShipStatus = FriendshipStatus.Pending,
            };
            
            await  _friendshipCollection.InsertOneAsync(friendship);
            return friendship;

    }
    
    public async Task<Friendship> DeclineFriendRequestAsync(int senderId, int receiverId)
    {

        //Vi tjekker for, at se om de har en aktiv friendrequest.
        var existingFriendshipRequest = await _friendshipCollection
            .Find(f => (f.SenderId == senderId && f.ReceiverId == receiverId)
                                || (f.ReceiverId == senderId && f.SenderId == receiverId))
            .FirstOrDefaultAsync();
        
        //Der findes ikke nogen friendship request
        if (existingFriendshipRequest == null)
        {
            throw new Exception("Friendship request not found");
        }

        //Der findes en request, og den bliver ændret til Declined istedet for pending
        if (existingFriendshipRequest.FriendShipStatus == FriendshipStatus.Pending)
        {
            var update = Builders<Friendship>.Update.Set(f => f.FriendShipStatus, FriendshipStatus.Declined);
             
            await _friendshipCollection.UpdateOneAsync(
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