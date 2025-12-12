using Microsoft.AspNetCore.Mvc;
using AnalyticsService.Models;
 

namespace AnalyticsService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsRepository _analyticsRepository;

    public AnalyticsController(IAnalyticsRepository analyticsRepository)
    {
        _analyticsRepository = analyticsRepository;
    }

    [HttpGet]
    public ActionResult<IEnumerable<object>> GetAnalytics()
    {
        // TBA: Implement get analytics data
        return Ok(new { message = "Get users - TBA" });
    }
    
    
    // Endpoint to receive class analytics data
    [HttpPost("{classId}/{userId}/{totalCaloriesBurned}/{category}/{durationMin}/{date}")]
    public async Task<IActionResult> PostClassesAnalytics(string classId, string userId, double totalCaloriesBurned, string category, int durationMin, DateTime date)
    {
        var classResult = await _analyticsRepository.PostClassesAnalytics(classId, userId, totalCaloriesBurned, category, durationMin, date);
        return Ok(classResult);
    }


    // Endpoint to post entered users
    [HttpPost("entered/{userId}/{entryTime}")]
    public async Task<IActionResult> AddUserToCrowd(string userId, DateTime entryTime)
    {
        var PostedUser = await _analyticsRepository.PostEnteredUser(userId, entryTime, DateTime.MinValue);
        return Ok(PostedUser);
    }
    
    // Endpoint to edit status for entered users
    [HttpPut("Exited/{userId}/{exitTime}")]
    public async Task<IActionResult> UpdateUserExitTime(string userId, DateTime exitTime)
    {
        var UpdatedExitTime = await _analyticsRepository.UpdateUserExitTime(userId, exitTime);
        return Ok(UpdatedExitTime);
    }
    
    [HttpPost("")]
    
    
    // Endpoint to get crowd
    [HttpGet("crowd")]
    public async Task<IActionResult> GetCrowdCount()
    {
        var crowdCount = await _analyticsRepository.GetCrowdCount();
        return Ok(crowdCount);
    }

    [HttpPost("solotraining")]
    public async Task<IActionResult> PostSoloTrainingResult([FromBody] SoloTrainingResultsDTO dto)
    {
        if (dto == null)
            return BadRequest("Payload required.");

        // Optional: basic validation
        if (string.IsNullOrWhiteSpace(dto.UserId))
            return BadRequest("UserId required.");

        // Pass through to repository
        var result = await _analyticsRepository.PostSoloTrainingResult(
            dto.UserId,
            dto.Date,
            dto.Exercises,
            dto.TrainingType,
            dto.DurationMinutes);

        return Ok(result);
    }



}