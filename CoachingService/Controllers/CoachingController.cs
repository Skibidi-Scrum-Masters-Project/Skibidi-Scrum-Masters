using CoachingService.Models;
using Microsoft.AspNetCore.Mvc;
using FitnessApp.Shared.Models;

namespace CoachingService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CoachingController : ControllerBase
{
    private readonly ICoachingRepository _coachingRepository;

    public CoachingController(ICoachingRepository coachingRepository)
    {
        _coachingRepository = coachingRepository;
    }

    [HttpGet]
    public ActionResult<IEnumerable<Session>> GetSessions()
    {
        // TBA: Implement get all coaches
        return Ok(new { message = "Get users - TBA" });
    }

    [HttpPost("session")]
    public ActionResult<Session> CreateSession([FromBody] Session session)
    {
        try
        {
            if (session == null)
            {
                return BadRequest(new { message = "Session cannot be null" });
            }

            var created = _coachingRepository.CreateSession(session);
            return Ok(created);
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while creating the session.", details = ex.Message });
        }
    }

}