using Microsoft.AspNetCore.Mvc;
using FitnessApp.Shared.Models;

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
    public ActionResult Login([FromBody] LoginRequest request)
    {
        // TBA: Implement authentication logic
        return Ok(new { message = "Login endpoint - TBA" });
    }
}