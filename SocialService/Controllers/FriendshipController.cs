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
        // Tjekker, at vi ikke sender Friend request til os selv.
        if (friendship.SenderId == friendship.ReceiverId)
            return BadRequest("You cannot send a friend request to yourself.");
        
        //Tjekker at man sender en friend request til andre end 0, da 0 ikke findes.
        if (friendship.SenderId <= 0 || friendship.ReceiverId <= 0)
            return BadRequest("SenderId and ReceiverId must be valid ids.");

        try
        {
            var createdFriendship = await _friendshipRepository.SendFriendRequestAsync(friendship.SenderId, friendship.ReceiverId);
            return Ok(createdFriendship.FriendShipStatus);
        }

        catch (Exception)
        {
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    [HttpPut("decline/{senderId}/{receiverId}")]
    public async Task<IActionResult> DeclineFriendRequestAsync(int senderId, int receiverId)
    {
        // Tjekker, at vi ikke afviser Friend request til os selv.
        if (senderId == receiverId)
            return BadRequest("You cannot decline a friend request to yourself.");
        
        //Tjekker at man afviser en friend request til andre end 0, da 0 ikke findes.
        if (senderId <= 0 || receiverId <= 0)
            return BadRequest("SenderId and ReceiverId must be valid ids.");

        try
        {
            var declineFriendship = await _friendshipRepository.DeclineFriendRequestAsync(senderId, receiverId);
            return Ok(declineFriendship.FriendShipStatus);
        }
        catch (Exception e)
        {
            return StatusCode(500, "An unexpected error occurred.");
        }
    }
    
}
