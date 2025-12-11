using Microsoft.AspNetCore.Mvc;
using FitnessApp.Shared.Models;
using SocialService.Models;
using SocialService.Repositories;

namespace SocialService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SocialController : ControllerBase
{
    private readonly ISocialRepository _socialRepository;

    public SocialController(ISocialRepository socialRepository)
    {
        _socialRepository = socialRepository;
    }

    [HttpGet("user/{userId}")]
    public ActionResult<IEnumerable<Friend>> GetUserFriends(string userId)
    {
        // TBA: Implement get user's friends
        return Ok(new { message = $"Get friends for user {userId} - TBA" });
    }

    [HttpPost("{userId}/sendFriendrequest/{receiverId}")]
    public async Task<ActionResult<Friendship>> SendFriendRequestAsync(int userId, int receiverId)
    {
        if (userId == receiverId)
            return BadRequest("You cannot send a friend request to yourself.");
    
        if (userId <= 0 || receiverId <= 0)
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
    public async Task<IActionResult> DeclineFriendRequestAsync(int userId, int receiverId)
    {
        if (userId == receiverId)
            return BadRequest("You cannot decline a friend request to yourself.");
    
        if (userId <= 0 || receiverId <= 0)
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
    public async Task<ActionResult<IEnumerable<Friendship>>> GetAllFriends(int userId)
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
    public async Task<ActionResult<Friendship>> GetFriendById(int userId, int receiverId)
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
    public async Task<ActionResult<Friendship>> CancelFriendRequest(int userId, int receiverId)
    {
        var friendRequestCanceled = await _socialRepository.CancelFriendRequest(userId, receiverId);
        
        return Ok(friendRequestCanceled);
    }

    [HttpGet("friendrequests/outgoing/{userId}")]
    public async Task<ActionResult<IEnumerable<Friendship>?>> GetOutgoingFriendRequests(int userId)
    {
        var friendRequests = await _socialRepository.GetOutgoingFriendRequestsAsync(userId);
        
        if (friendRequests == null)
            return BadRequest(friendRequests);
                
        return Ok(friendRequests);
    }
    
    [HttpGet("friendrequests/incoming/{userId}")]
    public async Task<ActionResult<IEnumerable<Friendship>?>> GetAllIncomingFriendRequests(int userId)
    {
        var friendRequests = await _socialRepository.GetAllIncomingFriendRequests(userId);
        
        if (friendRequests == null)
            return BadRequest(friendRequests);
                
        return Ok(friendRequests);
    }
    
    
    [HttpPut("accept/{userId}/{receiverId}")]
    public async Task<IActionResult> AcceptFriendRequestAsync(int userId, int receiverId)
    {
        if (userId == receiverId)
            return BadRequest("You cannot accept yourself as a friend.");
    
        if (userId <= 0 || receiverId <= 0)
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

}

