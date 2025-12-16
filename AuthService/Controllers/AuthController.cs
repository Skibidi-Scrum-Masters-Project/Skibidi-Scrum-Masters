using Microsoft.AspNetCore.Mvc;
using FitnessApp.Shared.Models;
using AuthService.Models;

namespace AuthService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthRepository _authRepository;

    public AuthController(IAuthRepository authRepository)
    {
        _authRepository = authRepository;
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] LoginRequest request)
    {
        if(request == null)
        return BadRequest(new {error = "Request cant be null", message = "Request cant be null"});
        try
        {
            var loginResponse = await _authRepository.Login(request);
            if (loginResponse == null)
            {
                return Unauthorized(new { error = "Invalid credentials", message = "Username or password is incorrect" });
            }
            return Ok(loginResponse);
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(new { error = "Invalid input", message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }
    [HttpPost("status")]
    public ActionResult Status()
    {
        // TBA: Implement status check logic
        return Ok(new { message = "Status endpoint - TBA" });
    }

    [HttpPost("refresh/{userId}/{refreshToken}/{role}")]
    public async Task<IActionResult> RefreshToken(string userId, string refreshToken, Role role)
    {
        if (userId == null)
        {
            return BadRequest("userid cannot be null");
        }

        if (refreshToken == null)
        {
            return BadRequest("RefreshToken cannot be null");
        }

        if (role == null)
        {
            return BadRequest("role cannot be null");
        }
        
        if (refreshToken.Length < 10) return Unauthorized();

        return Ok(_authRepository.RefreshToken(userId, refreshToken, role));
    }


    [HttpPost("Logout/{userId}")]

    public async Task<IActionResult> Logout(string userId)
    {
        if (userId == null)
        {
            return BadRequest("userid cannot be null");
        }

        return Ok(_authRepository.Logout(userId));
    }
}