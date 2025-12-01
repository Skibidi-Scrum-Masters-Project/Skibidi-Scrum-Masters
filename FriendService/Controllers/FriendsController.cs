using Microsoft.AspNetCore.Mvc;
using FitnessApp.Shared.Models;

namespace FriendService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FriendsController : ControllerBase
{
    private readonly IFriendRepository _friendRepository;

    public FriendsController(IFriendRepository friendRepository)
    {
        _friendRepository = friendRepository;
    }

    [HttpGet("user/{userId}")]
    public ActionResult<IEnumerable<Friend>> GetUserFriends(string userId)
    {
        // TBA: Implement get user's friends
        return Ok(new { message = $"Get friends for user {userId} - TBA" });
    }
}