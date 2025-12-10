using Microsoft.AspNetCore.Mvc;
using FitnessApp.Shared.Models;
using SocialService.Models;
using SocialService.Repositories;

namespace SocialService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FriendshipController : ControllerBase
{
    private readonly IFriendshipRepository _friendshipRepository;

    public FriendshipController(IFriendshipRepository friendshipRepository)
    {
        _friendshipRepository = friendshipRepository;
    }

    [HttpGet("user/{userId}")]
    public ActionResult<IEnumerable<Friend>> GetUserFriends(string userId)
    {
        // TBA: Implement get user's friends
        return Ok(new { message = $"Get friends for user {userId} - TBA" });
    }

    [HttpPost]
    public async Task<ActionResult<FriendshipStatus>> SendFriendRequestAsync([FromBody] Friendship friendship)
    {
        if (friendship.SenderId == friendship.ReceiverId)
            return BadRequest("You cannot send a friend request to yourself.");
    
        if (friendship.SenderId <= 0 || friendship.ReceiverId <= 0)
            return BadRequest("SenderId and ReceiverId must be valid ids.");

        try
        {
            var createdFriendship = await _friendshipRepository
                .SendFriendRequestAsync(friendship.SenderId, friendship.ReceiverId);

            return Ok(createdFriendship.FriendShipStatus);
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


    [HttpPut("decline/{senderId}/{receiverId}")]
    public async Task<IActionResult> DeclineFriendRequestAsync(int senderId, int receiverId)
    {
        if (senderId == receiverId)
            return BadRequest("You cannot decline a friend request to yourself.");
    
        if (senderId <= 0 || receiverId <= 0)
            return BadRequest("SenderId and ReceiverId must be valid ids.");

        try
        {
            var declineFriendship = await _friendshipRepository
                .DeclineFriendRequestAsync(senderId, receiverId);

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

    [HttpGet("{senderId}/friends")]
    public async Task<ActionResult<IEnumerable<Friendship>>> GetAllFriends(int senderId)
    {

        try
        {
            var listOfFriends = await _friendshipRepository.GetAllFriends(senderId);
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
    
    [HttpGet("{senderId}/friends/{receiverId}")]
    public async Task<ActionResult<Friendship>> GetFriendById(int senderId, int receiverId)
    {
        try
        {
            var friendFound = await _friendshipRepository.GetFriendById(senderId, receiverId);

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
}
