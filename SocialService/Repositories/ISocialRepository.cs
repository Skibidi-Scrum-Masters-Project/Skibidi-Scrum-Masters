using FitnessApp.Shared.Models;
using SocialService.Models;

namespace SocialService.Repositories;

public interface ISocialRepository
{
    
    //Sender friendRequest mellem 2 users
    Task<Friendship> SendFriendRequestAsync(string userId, string receiverId);
    
    //Metode til at acceptere en friend request
    Task<Friendship?> AcceptFriendRequest (string userId, string receiverId);
    
    //Metode til at afvise en friend request
    Task<Friendship> DeclineFriendRequestAsync (string userId, string receiverId);
    
    //Metode til at hente AllFriends på en bruger.
    Task<IEnumerable<Friendship?>> GetAllFriends(string userId);
    
    //Metode til at hente en specfik bruger.
    Task<Friendship?> GetFriendById(string userId, string receiverId);
    
    //Metode til at Cancel en pending request.
    Task<Friendship> CancelFriendRequest (string userId, string receiverId);
    
    //Metode til at hente alle ens sendte friend requests.
    Task<IEnumerable<Friendship>?> GetOutgoingFriendRequestsAsync (string userId);
    
    //Metode til at hente alle friend requests der er sendt til user
    Task<IEnumerable<Friendship>?> GetAllIncomingFriendRequests (string userId);
    
    
    
    
    
    
    
    //Post delen af SocialService
    
    
    //Metode til at oprette et post
    Task<Post> PostAPost(Post post);
    
    //Metode til at fjerne et post
    Task<Post> RemoveAPost(string postId);
    
    //Metode til at redigere et post
    Task<Post> EditAPost(Post post, string currentUserId);
    
    //Metode til at tilføje Comment til et post
    Task<Post> AddCommentToPost(string postId, Comment comment);
    
    //Metode til at fjerne Comment fra et post
    Task<Post> RemoveCommentFromPost(string postId, string commentId);
    
    //Metode til at redigere en comment
    Task<Post> EditComment(string postId, Comment comment);
    
    //Metode til at se alle comment for et Post
    Task<IEnumerable<Comment>> SeeAllCommentForPostId(string postId);
    
    //Metode til at se alle post for user
    Task<IEnumerable<Post>> SeeAllPostsForUser(string userId);
    
    //Event handler der subscriber til ClassService.FinishClass
    Task<string?> CreateDraftFromClassWorkoutCompletedAsync(ClassResultEventDto metric);
    
    //Metode til at se alle Draft post for user.
    Task<IEnumerable<Post>> SeeAllDraftPostsForUser(string userId);
    
    //Metode til at ændre draft status.
    Task<Post> ChangeDraftStatusForPost(string postId);

}