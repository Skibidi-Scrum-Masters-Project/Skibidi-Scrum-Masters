using MongoDB.Driver;
using SocialService.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;


namespace SocialService.Repositories;

public class SocialRepository : ISocialRepository
{
    private readonly IMongoCollection<Friendship> _friendshipCollection;
    
    public SocialRepository(IMongoDatabase database)
    {
       
        _friendshipCollection = database.GetCollection<Friendship>("Friendships"); 
    }

    public async Task<Friendship> SendFriendRequestAsync(int userId, int receiverId)
    {

        //Vi tjekker for, at se om de har en aktiv friendrequest.
        var existingFriendship = await _friendshipCollection
            .Find(f => (f.SenderId == userId && f.ReceiverId == receiverId)
                                || (f.ReceiverId == userId && f.SenderId == receiverId))
            .FirstOrDefaultAsync();

        
        if (existingFriendship != null)
        {
            //Tjekker om der allerede er en pending FriendRequest
            if (existingFriendship.FriendShipStatus == FriendshipStatus.Pending)
            {
                throw new InvalidOperationException("Friendship request already exists");
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
                SenderId = userId,
                ReceiverId = receiverId,
                FriendShipStatus = FriendshipStatus.Pending,
            };
            
            await  _friendshipCollection.InsertOneAsync(friendship);
            return friendship;

    }
    
    public async Task<Friendship> DeclineFriendRequestAsync(int userId, int receiverId)
    {

        //Vi tjekker for, at se om de har en aktiv friendrequest.
        var existingFriendshipRequest = await _friendshipCollection
            .Find(f => (
                f.ReceiverId == userId && 
                f.SenderId == receiverId &&
                f.FriendShipStatus == FriendshipStatus.Pending))
            .FirstOrDefaultAsync();
        
        //Der findes ikke nogen friendship request
        if (existingFriendshipRequest == null)
        {
            throw new InvalidOperationException("Friendship request not found or not pending");
        }


        var update = Builders<Friendship>.Update
            .Set(f => f.FriendShipStatus, FriendshipStatus.Declined);

        await _friendshipCollection.UpdateOneAsync(
            friendship => friendship.FriendshipId == existingFriendshipRequest.FriendshipId,
            update
        );

        existingFriendshipRequest.FriendShipStatus = FriendshipStatus.Declined;
        return existingFriendshipRequest;
        
    }


    public async Task<IEnumerable<Friendship?>> GetAllFriends(int userId)
    {
        var findFriendsForUser = await _friendshipCollection
            .FindAsync(f => 
                f.FriendShipStatus == FriendshipStatus.Accepted &&
                            (f.SenderId == userId || f.ReceiverId == userId));
        
        return await findFriendsForUser.ToListAsync();
    }

    public async Task<Friendship?> GetFriendById(int userId, int receiverId)
    {
        
        var findFriendForUser = await _friendshipCollection
            .FindAsync(f => f.ReceiverId == receiverId && f.SenderId == userId  && f.FriendShipStatus == FriendshipStatus.Accepted);
        
        return await findFriendForUser.SingleOrDefaultAsync();
    }

    public async Task<Friendship> CancelFriendRequest(int userId, int receiverId)
    {
        var existingFriendshipRequest = await _friendshipCollection
            .Find(f => f.SenderId == userId 
                       && f.ReceiverId == receiverId 
                       && f.FriendShipStatus == FriendshipStatus.Pending)
            .FirstOrDefaultAsync();
        
        if (existingFriendshipRequest == null)
        {
            throw new InvalidOperationException("There is no pending friendship request between these users.");
        }
        

        var newStatus = FriendshipStatus.None;

        var updateStatus = Builders<Friendship>.Update
            .Set(f => f.FriendShipStatus, newStatus);

        await _friendshipCollection.UpdateOneAsync(
            friendship => friendship.FriendshipId == existingFriendshipRequest.FriendshipId,
            updateStatus
        );

       
        existingFriendshipRequest.FriendShipStatus = newStatus;

        return existingFriendshipRequest;
    }

    public async Task<IEnumerable<Friendship>?> GetAllFriendRequests(int userId)
    {
        var findFriendRequestForUser = await _friendshipCollection
            .Find(f => f.ReceiverId == userId && f.FriendShipStatus == FriendshipStatus.Pending)
            .ToListAsync();
        
        return findFriendRequestForUser;
        
    }

    public async Task<Friendship?> AcceptFriendRequest(int senderId, int receiverId)
    {
        var existingFriendshipRequest = await _friendshipCollection
            .Find(f => f.SenderId == senderId 
                       && f.ReceiverId == receiverId 
                       && f.FriendShipStatus == FriendshipStatus.Pending)
            .FirstOrDefaultAsync();
        
        if (existingFriendshipRequest == null)
        {
            throw new KeyNotFoundException("Friend request not found");
        }
        

        var newStatus = FriendshipStatus.Accepted;

        var updateStatus = Builders<Friendship>.Update
            .Set(f => f.FriendShipStatus, newStatus);

        await _friendshipCollection.UpdateOneAsync(
            friendship => friendship.FriendshipId == existingFriendshipRequest.FriendshipId,
            updateStatus
        );
        
       
        existingFriendshipRequest.FriendShipStatus = newStatus;

        return existingFriendshipRequest;
    }
}
