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
    public async Task<ActionResult<SoloTrainingSession>> CreateSoloTraining(string userId, SoloTrainingSession soloTraining)
    {
        if (soloTraining == null)
            return BadRequest(new { error = "Invalid input", message = "Solo training session cannot be null." });

        if (string.IsNullOrEmpty(userId))
            return BadRequest(new { error = "Invalid input", message = "User ID cannot be null or empty." });

        try
        {
            var result = await _soloTrainingRepository.CreateSoloTraining(userId, soloTraining);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    [HttpGet("{userId}")]
    public async Task<ActionResult<IEnumerable<SoloTrainingSession>>> GetAllSoloTrainingsForUser(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return BadRequest(new { error = "Invalid input", message = "User ID cannot be null or empty." });

        var sessions = await _soloTrainingRepository.GetAllSoloTrainingsForUser(userId);
        return Ok(sessions);
    }

    [HttpGet("recent/{userId}")]
    public async Task<ActionResult<SoloTrainingSession>> GetMostRecentSoloTrainingForUser(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return BadRequest(new { error = "Invalid input", message = "User ID cannot be null or empty." });

        var session = await _soloTrainingRepository.GetMostRecentSoloTrainingForUser(userId);
        return Ok(session);
    }

    [HttpDelete("{trainingId}")]
    public async Task<IActionResult> DeleteSoloTraining(string trainingId)
    {
        if (string.IsNullOrEmpty(trainingId))
            return BadRequest(new { error = "Invalid input", message = "Training ID cannot be null or empty." });

        try
        {
            await _soloTrainingRepository.DeleteSoloTraining(trainingId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }
}
