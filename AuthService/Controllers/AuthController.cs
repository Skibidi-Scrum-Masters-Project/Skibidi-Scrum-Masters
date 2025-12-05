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
}