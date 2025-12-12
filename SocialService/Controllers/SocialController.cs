using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using FitnessApp.Shared.Models;
using SocialService.Models;
using SocialService.Repositories;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Driver;

namespace SocialService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SocialController : ControllerBase
{
    private readonly ISocialRepository _socialRepository;
    private readonly IMongoCollection<Post> _posts;


    public SocialController(ISocialRepository socialRepository)
    {
        _socialRepository = socialRepository;
    }
    
    [HttpPost("/internal/events/class-workout-completed")]
    public async Task<IActionResult> ClassWorkoutCompleted([FromBody] ClassResultEventDto metric)
    {
        if (string.IsNullOrWhiteSpace(metric.EventId) ||
            string.IsNullOrWhiteSpace(metric.UserId) ||
            string.IsNullOrWhiteSpace(metric.ClassId))
            return BadRequest("Invalid payload.");

        var draftId = await _socialRepository.CreateDraftFromClassWorkoutCompletedAsync(metric);

        // Hvis dedupe ramte, kan du stadig returnere Ok()
        if (draftId == null) return Ok();

        return Ok(new { draftId });
    }
    
    [HttpPost("{userId}/sendFriendrequest/{receiverId}")]
    public async Task<ActionResult<Friendship>> SendFriendRequestAsync(string userId, string receiverId)
    {
        if (userId == receiverId)
            return BadRequest("You cannot send a friend request to yourself.");
    
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(receiverId))
            return BadRequest("SenderId and ReceiverId must be valid ids.");

        try
        {
            var createdFriendship = await _socialRepository
                .SendFriendRequestAsync(userId, receiverId);

            return Ok(createdFriendship);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception)
        {
            
            return StatusCode(500, "An unexpected error occurred.");
        }
    }


    [HttpPut("declineRequest/{userId}/{receiverId}")]
    public async Task<IActionResult> DeclineFriendRequestAsync(string userId, string receiverId)
    {
        if (userId == receiverId)
            return BadRequest("You cannot decline a friend request to yourself.");
    
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(receiverId))
            return BadRequest("SenderId and ReceiverId must be valid ids.");

        try
        {
            var declineFriendship = await _socialRepository
                .DeclineFriendRequestAsync(userId, receiverId);

            return Ok(declineFriendship.FriendShipStatus);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
            // log her
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    [HttpGet("{userId}/friends")]
    public async Task<ActionResult<IEnumerable<Friendship>>> GetAllFriends(string userId)
    {

        try
        {
            var listOfFriends = await _socialRepository.GetAllFriends(userId);
            return Ok(listOfFriends);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
            // log her
            return StatusCode(500, "An unexpected error occurred.");
        }

    }
    
    [HttpGet("{userId}/friends/{receiverId}")]
    public async Task<ActionResult<Friendship>> GetFriendById(string userId, string receiverId)
    {
        try
        {
            var friendFound = await _socialRepository.GetFriendById(userId, receiverId);

            if (friendFound == null)
                return NotFound();

            return Ok(friendFound);
        }
        catch (KeyNotFoundException ex)
        {
            // Hvis repo kaster KeyNotFoundException selv
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            // hvis der er dubletter da der bruges SingleOrDefaultAsync
            return Conflict(ex.Message);
        }
        catch (ArgumentException ex)
        {
            // Hvis der er noget galt med input
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
            // log her
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    [HttpPut("{userId}/cancel/{receiverId}")]
    public async Task<ActionResult<Friendship>> CancelFriendRequest(string userId, string receiverId)
    {
        var friendRequestCanceled = await _socialRepository.CancelFriendRequest(userId, receiverId);
        
        return Ok(friendRequestCanceled);
    }

    [HttpGet("friendrequests/outgoing/{userId}")]
    public async Task<ActionResult<IEnumerable<Friendship>?>> GetOutgoingFriendRequests(string userId)
    {
        var friendRequests = await _socialRepository.GetOutgoingFriendRequestsAsync(userId);
        
        if (friendRequests == null)
            return BadRequest(friendRequests);
                
        return Ok(friendRequests);
    }
    
    [HttpGet("friendrequests/incoming/{userId}")]
    public async Task<ActionResult<IEnumerable<Friendship>?>> GetAllIncomingFriendRequests(string userId)
    {
        var friendRequests = await _socialRepository.GetAllIncomingFriendRequests(userId);
        
        if (friendRequests == null)
            return BadRequest(friendRequests);
                
        return Ok(friendRequests);
    }
    
    
    [HttpPut("accept/{userId}/{receiverId}")]
    public async Task<IActionResult> AcceptFriendRequestAsync(string userId, string receiverId)
    {
        if (userId == receiverId)
            return BadRequest("You cannot accept yourself as a friend.");
    
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(receiverId))
            return BadRequest("SenderId and ReceiverId must be valid ids.");

        try
        {
            var acceptFriendshipRequest = await _socialRepository
                .AcceptFriendRequest(userId, receiverId);

            return Ok(acceptFriendshipRequest.FriendShipStatus);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
            // log her
            return StatusCode(500, "An unexpected error occurred.");
        }
    }


    [HttpPost("PostAPost")]
    public Task<Post> PostAPost([FromBody]Post post)
    {
        var createdPost = _socialRepository.PostAPost(post);
        
        return createdPost;
    }


    [HttpDelete("RemoveAPost/{postId}")]
    public Task<Post> RemoveAPost(string postId)
    {
        return _socialRepository.RemoveAPost(postId);
    }

    
    [Authorize]
    [HttpPut("EditAPost")]
    public async Task<ActionResult<Post>> EditAPost([FromBody] Post post)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(currentUserId))
        {
            return Unauthorized();
        }

        // Ignorér hvad klienten sender og sæt ejer ud fra JWT
        post.UserId = currentUserId;

        try
        {
            var editedPost = await _socialRepository.EditAPost(post, currentUserId);
            return Ok(editedPost);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }


    [HttpPut("AddACommentToPost/{postId}")]
    public async Task<ActionResult<Post>> AddACommentToPost(string postId, [FromBody]Comment comment)
    {
        try
        {
            var addedComment = await _socialRepository.AddCommentToPost(postId, comment);
        
            return Ok(addedComment);
        }

        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }


    [HttpDelete("RemoveACommentFromPost/{postId}/{commentId}")]
    public async Task<ActionResult<Post>> RemoveCommentFromPost(string postId, string commentId)
    {
        try
        {
            var updatedPost = await _socialRepository.RemoveCommentFromPost(postId, commentId);
            return Ok(updatedPost);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpPut("EditComment/{postId}")]
    public async Task<ActionResult<Post>> EditComment(string postId, [FromBody] Comment comment)
    {
        try
        {
            var editedComment = await _socialRepository.EditComment(postId, comment);
            return Ok(editedComment);

        }
        catch (KeyNotFoundException ex)
        {
            // Post eller comment fandtes ikke
            return NotFound(ex.Message);
        }
        catch (Exception)
        {
            // Anden type fejl: database nede, null reference, osv.
            return StatusCode(500, "An unexpected error occurred");
        }
    }

    [HttpGet("SeeAllCommentForPost/{postId}")] 
    public async Task<IEnumerable<Comment>> SeeAllCommentForPost(string postId) 
    { 
        var listOfCommentsForPost = await _socialRepository.SeeAllCommentForPostId(postId); 
        return listOfCommentsForPost; 
    }

    [HttpGet("SeeAllPostsForUser/{userId}")]
    public async Task<IEnumerable<Post>> SeeAllPostsForUser(string userId)
    {
        var listOfPostForUser = await _socialRepository.SeeAllPostsForUser(userId);

        return listOfPostForUser;
    }


    [HttpGet("SeeAllDraftPostsForUser/{userId}")]
    public async Task<IEnumerable<Post>> SeeAllDraftPostsForUser(string userId)
    {
        var listOfDraftPosts = await _socialRepository.SeeAllDraftPostsForUser(userId);

        return listOfDraftPosts;
    }



}

