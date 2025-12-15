using Microsoft.AspNetCore.Mvc;
using FitnessApp.Shared.Models;
using FitnessApp.SoloTrainingService.Models;
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
    [HttpPost("create/program")]
    public async Task<ActionResult<WorkoutProgram>> CreateWorkoutProgram([FromBody] WorkoutProgram workoutProgram)
    {
        if (workoutProgram == null)
        {
            return BadRequest(new { error = "Invalid input", message = "Workout program cannot be null." });
        }

        try
        {
            var createdProgram = await _soloTrainingRepository.CreateWorkoutProgram(workoutProgram);
            return Ok(createdProgram);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
     
    }

    [HttpPost("{userId}")]
    public async Task<ActionResult<SoloTrainingSession>> CreateSoloTraining(string userId, [FromBody] SoloTrainingSession soloTraining)
    {
        if (soloTraining == null)
        {
            return BadRequest(new { error = "Invalid input", message = "Solo training session cannot be null." });
        }

        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest(new { error = "Invalid input", message = "User ID cannot be null or empty." });
        }

        try
        {
            var soloTrainingSession = await _soloTrainingRepository.CreateSoloTraining(userId, soloTraining);
            return Ok(soloTrainingSession);
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
        {
            return BadRequest(new { error = "Invalid input", message = "User ID cannot be null or empty." });
        }

        try
        {
            var soloTrainings = await _soloTrainingRepository.GetAllSoloTrainingsForUser(userId);
            return Ok(soloTrainings);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    [HttpGet("recent/{userId}")]
    public async Task<ActionResult<SoloTrainingSession>> GetMostRecentSoloTrainingForUser(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return BadRequest(new { error = "Invalid input", message = "User ID cannot be null or empty." });
        }

        try
        {
            var recentSession = await _soloTrainingRepository.GetMostRecentSoloTrainingForUser(userId);

            if (recentSession == null)
            {
                return NotFound(new { error = "Not found", message = "No solo training sessions found for this user." });
            }

            return Ok(recentSession);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }

    [HttpDelete("{trainingId}")]
    public async Task<IActionResult> DeleteSoloTraining(string trainingId)
    {
        if (string.IsNullOrEmpty(trainingId))
        {
            return BadRequest(new { error = "Invalid input", message = "Training ID cannot be null or empty." });
        }

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
    [HttpGet("programs")]
    public async Task<ActionResult<IEnumerable<WorkoutProgram>>> GetAllWorkoutPrograms()
    {
        try
        {
            var workoutPrograms = await _soloTrainingRepository.GetAllWorkoutPrograms();
            return Ok(workoutPrograms);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Internal server error", message = ex.Message });
        }
    }
}
