using CoachingService.Models;
using Microsoft.AspNetCore.Mvc;
using FitnessApp.Shared.Models;
using Microsoft.AspNetCore.Authorization;

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

   
    [HttpGet("AllSessions")]
    public ActionResult<IEnumerable<Session>> GetAllSessions()
    {
        try
        {
            var sessions = _coachingRepository.GetAllSessions();
            return Ok(sessions);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving sessions.", details = ex.Message });
        }
    }

   
    [HttpPut("Session")]
    public ActionResult<Session> BookSession([FromBody] Session session)
    {
        try
        {
            if (session == null)
            {
                return BadRequest(new { message = "Session cannot be null" });
            }

            var created = _coachingRepository.BookSession(session);
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
    
    [HttpGet("Session/{id}")]
    public ActionResult<Session> GetSessionById(string id)
    {
        try
        {
            var session = _coachingRepository.GetSessionById(id);

            if (session == null)
                return NotFound(new { message = $"Session with id {id} not found" });

            return Ok(session);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving the session.", details = ex.Message });
        }
    }
    
    [HttpPut("CancelSession/{id}")]
    public ActionResult<Session> CancelSession(string id)
    {
        try
        {
            var cancelled = _coachingRepository.CancelSession(id);
            return Ok(cancelled);
        }
        catch (ArgumentNullException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while cancelling the session.", details = ex.Message });
        }
    }
    
    [HttpPut("CompleteSession/{id}")]
    public ActionResult<Session> CompleteSession(string id)
    {
        try
        {
            var session = _coachingRepository.CompleteSession(id);
            return Ok(session);
        }
        catch (ArgumentNullException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    
    //Kun for Coach
    [HttpPost("MakeSessionAsCoach")]
    public ActionResult<Session> CreateSession([FromBody] Session session)
    {
        var created = _coachingRepository.CreateSession(session);
        
        return Ok(created);
    }

    [HttpDelete("RemoveSessionAsCoach/{id}")]
    public ActionResult<Session> DeleteSession(string id)
    {
        var deleted = _coachingRepository.DeleteSession(id);
        
        return Ok(deleted);
    }
    
    [HttpGet("AvailableSessions")]
    public ActionResult<IEnumerable<Session>> GetAvailableSessions()
    {
        var sessions = _coachingRepository.GetAllAvaliableCoachSessions();

        return Ok(sessions);
    }
    
    [HttpGet("AvailableSessions/{coachId}")]
    public ActionResult<IEnumerable<Session>> GetAvailableSessionsForCoachId(string coachId)
    {
        var sessions = _coachingRepository.GetAllAvailableCoachSessionsForCoachId(coachId);

        return Ok(sessions);
    }

    [HttpGet("AllSessions/{coachId}")]
    public ActionResult<IEnumerable<Session>> GetAllSessionsByCoachId(string coachId)
    {
        var sessions = _coachingRepository.GetAllSessionsByCoachId(coachId);
        
        return Ok(sessions);
    }
    
}