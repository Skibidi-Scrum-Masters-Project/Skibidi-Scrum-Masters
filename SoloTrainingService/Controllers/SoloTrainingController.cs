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
    [HttpGet]
    public ActionResult<IEnumerable<Workout>> GetSoloTrainings()
    {
        // TBA: Implement get all solo trainings
        return Ok(new { message = "Get solo trainings - TBA" });
    }
}