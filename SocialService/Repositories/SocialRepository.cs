using MongoDB.Driver;
using SocialService.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Data;


namespace SocialService.Repositories;

public class SocialRepository : ISocialRepository
{
    private readonly IMongoCollection<Friendship> _friendshipCollection;
    private readonly IMongoCollection<Post> _postCollection;
    
    public SocialRepository(IMongoDatabase database)
    {
       
        _friendshipCollection = database.GetCollection<Friendship>("Friendships"); 
        _postCollection =  database.GetCollection<Post>("Posts");
    }

    public async Task<Friendship> SendFriendRequestAsync(string userId, string receiverId)
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
    
    public async Task<Friendship> DeclineFriendRequestAsync(string userId, string receiverId)
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


    public async Task<IEnumerable<Friendship?>> GetAllFriends(string userId)
    {
        var findFriendsForUser = await _friendshipCollection
            .FindAsync(f => 
                f.FriendShipStatus == FriendshipStatus.Accepted &&
                            (f.SenderId == userId || f.ReceiverId == userId));
        
        return await findFriendsForUser.ToListAsync();
    }

    public async Task<Friendship?> GetFriendById(string userId, string receiverId)
    {
        
        var findFriendForUser = await _friendshipCollection
            .FindAsync(f => f.ReceiverId == receiverId && f.SenderId == userId  && f.FriendShipStatus == FriendshipStatus.Accepted);
        
        return await findFriendForUser.SingleOrDefaultAsync();
    }

    public async Task<Friendship> CancelFriendRequest(string userId, string receiverId)
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

    public async Task<IEnumerable<Friendship>?> GetOutgoingFriendRequestsAsync(string userId)
    {
        var findFriendRequestForUser = await _friendshipCollection
            .Find(f => f.SenderId == userId 
                       && f.FriendShipStatus == FriendshipStatus.Pending)
            .ToListAsync();
    
        return findFriendRequestForUser;
    }
    
    
    public async Task<IEnumerable<Friendship>?> GetAllIncomingFriendRequests(string userId)
    {
        return await _friendshipCollection
            .Find(f => f.ReceiverId == userId 
                       && f.FriendShipStatus == FriendshipStatus.Pending)
            .ToListAsync();
    }



    public async Task<Friendship?> AcceptFriendRequest(string senderId, string receiverId)
    {
        // 1. Find venskabet uanset status
        var existingFriendshipRequest = await _friendshipCollection
            .Find(f => f.SenderId == senderId 
                       && f.ReceiverId == receiverId)
            .FirstOrDefaultAsync();
    
        if (existingFriendshipRequest == null)
        {
            throw new KeyNotFoundException("Friend request not found");
        }

        // 2. Det må ikke være muligt at acceptere en Declined request
        if (existingFriendshipRequest.FriendShipStatus == FriendshipStatus.Declined)
        {
            throw new InvalidOperationException("Cannot accept a declined friend request.");
        }

        // 3. Kun Pending må kunne accepteres
        if (existingFriendshipRequest.FriendShipStatus != FriendshipStatus.Pending)
        {
            throw new InvalidOperationException("Only pending friend requests can be accepted.");
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


    public async Task<Post> PostAPost(Post post)
    {
        var newPost = new Post
        {
            UserName = post.UserName,
            UserId = post.UserId,
            FitnessClassId = post.FitnessClassId,
            WorkoutId = post.WorkoutId,
            PostTitle = post.PostTitle,
            PostContent = post.PostContent,
            PostDate = DateTime.UtcNow,
            Comments = new List<Comment>()

        };
        
        await _postCollection.InsertOneAsync(newPost);
        
        return newPost;
        
    }

    public Task<Post> SeeSpecficPostByPostId(string postId)
    {
        var findPostForId = _postCollection
            .Find(p => p.Id == postId)
            .FirstOrDefaultAsync();
        
        if (findPostForId == null)
        {
            throw new KeyNotFoundException("Post not found");
        }
        
        return findPostForId;
    }

    
    
    public async Task<Post> RemoveAPost(string postId)
    {
        var existingPost = await _postCollection
            .Find(p => p.Id == postId)
            .FirstOrDefaultAsync();

        if (existingPost == null)
        {
            throw new KeyNotFoundException("Post not found");
        }
        
        await _postCollection.DeleteOneAsync(p => p.Id == postId);
        
        return existingPost;
    }


    public async Task<Post> EditAPost(Post post, string currentUserId)
    {
        var filter = Builders<Post>.Filter.And(
            Builders<Post>.Filter.Eq(p => p.Id, post.Id),
            Builders<Post>.Filter.Eq(p => p.UserId, currentUserId)
        );

        var updateDefinition = Builders<Post>.Update
            .Set(p => p.PostTitle, post.PostTitle)
            .Set(p => p.PostContent, post.PostContent)
            .Set(p => p.FitnessClassId, post.FitnessClassId)
            .Set(p => p.WorkoutId, post.WorkoutId);

        var options = new FindOneAndUpdateOptions<Post>
        {
            ReturnDocument = ReturnDocument.After
        };

        var updated = await _postCollection.FindOneAndUpdateAsync(
            filter,
            updateDefinition,
            options
        );

        if (updated == null)
        {
            throw new KeyNotFoundException("Post not found or access denied");
        }

        return updated;
    }


    public async Task<Post> AddCommentToPost(string postId, Comment comment)
    {
        comment.Id ??= ObjectId.GenerateNewId().ToString();
        comment.CommentDate = DateTime.UtcNow;

        var update = Builders<Post>.Update.Push(p => p.Comments, comment);
        var options = new FindOneAndUpdateOptions<Post> { ReturnDocument = ReturnDocument.After };

        var updatedPost = await _postCollection.FindOneAndUpdateAsync(
            p => p.Id == postId,
            update,
            options);

        if (updatedPost == null) throw new KeyNotFoundException("Post not found");
        return updatedPost;

    }
    
    
    public async Task<Post> RemoveCommentFromPost(string postId, string commentId)
    {
        var post = await _postCollection
            .Find(p => p.Id == postId)
            .FirstOrDefaultAsync();

        if (post == null)
        {
            throw new  KeyNotFoundException("Post not found");
        }
        
        var findComment = post.Comments.Any(c => c.Id == commentId);

        if (!findComment)
        {
            throw new KeyNotFoundException("Comment not found");
        }
        
        post.Comments.Remove(post.Comments.First(c => c.Id == commentId));
        
        await _postCollection.ReplaceOneAsync(p => p.Id == postId, post);


        return post;
    }


    public async Task<Post> EditComment(string postId, Comment comment)
    {
        // 1. Find the post
        var post = await _postCollection
            .Find(p => p.Id == postId)
            .FirstOrDefaultAsync();
    
        if (post == null)
        {
            throw new KeyNotFoundException("Post not found");
        }
    
        // 2. Check that the comment exists on this post
        var exists = post.Comments.Any(c => c.Id == comment.Id);
    
        if (!exists)
        {
            throw new KeyNotFoundException("Comment not found");
        }

        // 3. Build a filter that matches the post AND the specific comment
        var filter = Builders<Post>.Filter.And(
            Builders<Post>.Filter.Eq(p => p.Id, postId),
            Builders<Post>.Filter.ElemMatch(p => p.Comments, c => c.Id == comment.Id)
        );

        // 4. "$" refererer til det comment i Comments-arrayet, som matchede ElemMatch-filteret
        var update = Builders<Post>.Update
            .Set("Comments.$.CommentText", comment.CommentText);

        var options = new FindOneAndUpdateOptions<Post>
        {
            ReturnDocument = ReturnDocument.After
        };

        // 5. Correct order of parameters: filter, update, options
        var updated = await _postCollection.FindOneAndUpdateAsync(
            filter,
            update,
            options
        );

        if (updated == null)
        {
            throw new KeyNotFoundException("Post not found or access denied");
        }

        return updated;
    }

    public async Task<IEnumerable<Comment>> SeeAllCommentForPostId(string postId)
    {
        var post = await _postCollection
            .Find(p => p.Id == postId)
            .FirstOrDefaultAsync();

        return post?.Comments ?? Enumerable.Empty<Comment>();
    }

    public async Task<IEnumerable<Post>> SeeAllPostsForUser(string userId)
    {
        var posts = await _postCollection
            .Find(p => p.UserId == userId)
            .ToListAsync();

        return posts;

    }
    
    
    public async Task<string?> CreateDraftFromClassWorkoutCompletedAsync(ClassResultEventDto metric)
    {
        // dedupe
        var already = await _postCollection.Find(p => p.SourceEventId == metric.EventId).AnyAsync();
        if (already) return null;

        var draft = new Post
        {
            UserName = metric.UserName,
            UserId = metric.UserId,
            FitnessClassId = metric.ClassId,
            PostDate = metric.Date,
            PostTitle = "Class completed",
            PostContent = $"Duration: {metric.DurationMin} min. Calories: {metric.CaloriesBurned:0}. Watt: {metric.Watt:0}",
            Type = PostType.Workout,
            IsDraft = true,
            SourceEventId = metric.EventId,
            WorkoutStats = new WorkoutStatsSnapshot
            {
                DurationSeconds = metric.DurationMin * 60,
                Calories = (int?)Math.Round(metric.CaloriesBurned)
            }
        };

        await _postCollection.InsertOneAsync(draft);
        return draft.Id;
    }


    public async Task<string?> CreateDraftFromSoloTrainingCompletedAsync(SoloTrainingCompletedEventDto metric)
    {
        var already = await _postCollection.Find(p => p.SourceEventId == metric.EventId).AnyAsync();
        if (already) return null;

        var draft = new Post
        {
            UserName = metric.UserName,
            UserId = metric.UserId!,
            WorkoutId = metric.SoloTrainingSessionId!,
            PostDate = metric.Date,
            PostTitle = "SoloTraining completed",
            PostContent =
                $"Just Finished a {metric.TrainingType} Duration: {metric.DurationMinutes} min Exercises: {metric.ExerciseCount}",
            Type = PostType.Workout,
            IsDraft = true,
            SourceEventId = metric.EventId,
            WorkoutStats = new WorkoutStatsSnapshot
            {
                DurationSeconds = metric.DurationMinutes * 60
            }
        };
        
        await  _postCollection.InsertOneAsync(draft);
        return draft.Id;
    }

    
    public async Task<IEnumerable<Post>> SeeAllDraftPostsForUser(string userId)
    {
        var posts = await _postCollection
            .Find(p => p.UserId == userId && p.IsDraft == true)
            .ToListAsync();

        return posts;
    }

    
    public async Task<Post> ChangeDraftStatusForPost(string postId)
    {
        var update = Builders<Post>.Update
            .Set(p => p.IsDraft, false);

        var options = new FindOneAndUpdateOptions<Post>
        {
            ReturnDocument = ReturnDocument.After
        };

        var post = await _postCollection.FindOneAndUpdateAsync(
            p => p.Id == postId,
            update,
            options
        );

        if (post == null)
        {
            throw new KeyNotFoundException("Post not found or access denied");
        }

        return post;
    }


    public async Task<IEnumerable<Post>> SeeAllFriendsPosts(string userId)
    {
         //Hent accepterede friendships
        var friendships = await GetAllFriends(userId);

        //// Udled friend ids (den modsatte af userId)
        var friendIds = friendships
            .Where(f => f != null)
            .Select(f => f!.SenderId == userId ? f.ReceiverId : f.SenderId)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct()
            .ToList();
        
        friendIds.Add(userId);

        // 3) Find posts for alle friendIds
        var postsWithFriendIds = await _postCollection.FindAsync(p => friendIds.Contains(p.UserId));

        // 4) (valgfrit) sortér
        var posts = await postsWithFriendIds.ToListAsync();
        return posts
            .OrderByDescending(p => p.PostDate); 
    }




}
