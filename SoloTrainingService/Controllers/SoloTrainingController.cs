using Microsoft.AspNetCore.Mvc;
using FitnessApp.Shared.Models;

namespace SoloTrainingService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SoloTrainingController : ControllerBase
{
    private readonly ISoloTrainingRepository _soloTrainingRepository;

    public SoloTrainingController(ISoloTrainingRepository soloTrainingRepository)
    {
        _soloTrainingRepository = soloTrainingRepository;
    }

    [HttpPost("{userId}")]
    public ActionResult<SoloTrainingSession> CreateSoloTraining(string userId, SoloTrainingSession soloTraining)
    {
        if(soloTraining == null)
        {
            return BadRequest(new { error = "Invalid input", message = "Solo training session cannot be null." });
        }
        if(string.IsNullOrEmpty(userId))
        {
            return BadRequest(new { error = "Invalid input", message = "User ID cannot be null or empty." });
        }
        try
        {
            SoloTrainingSession soloTrainingSession = _soloTrainingRepository.CreateSoloTraining(userId, soloTraining);
            return Ok(soloTrainingSession);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
       
    }
    [HttpGet("{userId}")]
    public ActionResult<IEnumerable<SoloTrainingSession>> GetAllSoloTrainingsForUser(string userId)
    {
        if(string.IsNullOrEmpty(userId))
            return BadRequest(new { error = "Invalid input", message = "User ID cannot be null or empty." });
    
        List<SoloTrainingSession> soloTrainings = _soloTrainingRepository.GetAllSoloTrainingsForUser(userId);
        return Ok(soloTrainings);
    }
    [HttpGet("recent/{userId}")]
    public ActionResult<SoloTrainingSession> GetMostRecentSoloTrainingForUser(string userId)
    {
        if(string.IsNullOrEmpty(userId))
            return BadRequest(new { error = "Invalid input", message = "User ID cannot be null or empty." });
    
        SoloTrainingSession recentSession = _soloTrainingRepository.GetMostRecentSoloTrainingForUser(userId);
        return Ok(recentSession);
    }
    [HttpDelete("{trainingId}")]
    public IActionResult DeleteSoloTraining(string trainingId)
    {
        if (string.IsNullOrEmpty(trainingId))
        {
            return BadRequest(new { error = "Invalid input", message = "Training ID cannot be null or empty." });
        }

        try
        {
            _soloTrainingRepository.DeleteSoloTraining(trainingId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }
}