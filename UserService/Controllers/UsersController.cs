using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
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
    [Authorize(Roles = "Admin,Coach")]
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
    [Authorize]
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
    [HttpGet("username/{username}")]
    public ActionResult<User> GetUserByUsername(string username)
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
    [HttpGet("{id}")]
    public ActionResult<User> GetUserById(string id)
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
    public ActionResult<User> UpdateUser(User updatedUser)
    {
        try
        {
            var userInDb = _userRepository.GetUserById(updatedUser.Id!);
            if (userInDb == null)
            {
                return NotFound(new { error = "User not found", message = $"User with ID '{updatedUser.Id}' does not exist" });
            }

            User user = _userRepository.UpdateUser(updatedUser);
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
    public ActionResult<List<User>> GetUsersByRole(Role role)
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