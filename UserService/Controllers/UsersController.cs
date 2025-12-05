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
        try
        {
            List<User> users = _userRepository.GetAllUsers();
            return Ok(users);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }
    [HttpPost]
    public ActionResult<User> CreateUser(User user)
    {
        try
        {
            User createdUser = _userRepository.CreateUser(user);
            return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, createdUser);
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(new { error = "Invalid input", message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = "Validation error", message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(500, new { error = "Database error", message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }
    [HttpGet("{id}")]
    public ActionResult<User> GetUserById(string id)
    {
        // TBA: Implement get user by id
        return Ok(new { message = $"Get user by id {id} - TBA" });
    }
}