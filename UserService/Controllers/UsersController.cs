using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FitnessApp.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace UserService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserRepository userRepository, ILogger<UsersController>? logger = null)
    {
        _userRepository = userRepository;
        _logger = logger ?? NullLogger<UsersController>.Instance;
    }

    [HttpGet]
    
    public ActionResult<IEnumerable<UserDTO>> GetUsers()
    {
        try
        {
            List<UserDTO> users = _userRepository.GetAllUsers();
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
        _logger.LogInformation("Creating a new user with username: {Username}", user.Username);
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
    [HttpGet("username/{username}")]
    public ActionResult<UserDTO> GetUserByUsername(string username)
    {
        try
        {
            var user = _userRepository.GetUserByUsername(username);
            if (user == null)
            {
                return NotFound(new { error = "User not found", message = $"User with username '{username}' does not exist" });
            }
            return Ok(user);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }
    [HttpGet("username/{username}/secure")]
    public ActionResult<User> GetUserByUsernameSecure(string username)
    {
        try
        {
            var user = _userRepository.GetUserByUsernameSecure(username);
            if (user == null)
            {
                return NotFound(new { error = "User not found", message = $"User with username '{username}' does not exist" });
            }
            return Ok(user);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }
    [HttpGet("{id}")]
    public ActionResult<UserDTO> GetUserById(string id)
    {
        try
        {
            var user = _userRepository.GetUserById(id);
            if (user == null)
            {
                return NotFound(new { error = "User not found", message = $"User with ID '{id}' does not exist" });
            }
            return Ok(user);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }
    [HttpDelete("{id}")]
    [Authorize]
    public ActionResult DeleteUser(string id)
    {
        try
        {
            bool deleted = _userRepository.DeleteUser(id);
            if (!deleted)
            {
                return NotFound(new { error = "User not found", message = $"User with ID '{id}' does not exist" });
            }
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }
    [HttpPut("{id}")]
    [Authorize]
    public ActionResult<UserDTO> UpdateUser(User updatedUser)
    {
        try
        {
            var userInDb = _userRepository.GetUserById(updatedUser.Id!);
            if (userInDb == null)
            {
                return NotFound(new { error = "User not found", message = $"User with ID '{updatedUser.Id}' does not exist" });
            }

            UserDTO user = _userRepository.UpdateUser(updatedUser);
            return Ok(user);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = "Validation error", message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }
    [HttpGet("role/{role}")]
    public ActionResult<List<UserDTO>> GetUsersByRole(Role role)
    {
        try
        {
            var users = _userRepository.GetUsersByRole(role);
            return Ok(users);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }
    
}