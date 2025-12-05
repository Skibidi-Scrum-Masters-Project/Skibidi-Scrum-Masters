using Microsoft.AspNetCore.Mvc;
using FitnessApp.Shared.Models;

namespace CoachingService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoachesController : ControllerBase
{
    private readonly ICoachRepository _coachRepository;

    public CoachesController(ICoachRepository coachRepository)
    {
        _coachRepository = coachRepository;
    }

    [HttpGet]
    public ActionResult<IEnumerable<User>> GetCoaches()
    {
        // TBA: Implement get all coaches
        return Ok(new { message = "Get users - TBA" });
    }
}