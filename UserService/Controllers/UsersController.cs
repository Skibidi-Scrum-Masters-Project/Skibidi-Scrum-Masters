using Microsoft.AspNetCore.Mvc;
using FitnessApp.Shared.Models;

namespace UserService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public UsersController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpGet]
    public ActionResult<IEnumerable<User>> GetUsers()
    {
        // TBA: Implement get all users
        return Ok(new { message = "Get users - TBA" });
    }
}