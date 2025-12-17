using Microsoft.AspNetCore.Mvc;
using AnalyticsService.Models;
using Microsoft.AspNetCore.Authorization;
 

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

    
    [HttpPost("classes")]
    
    public async Task<IActionResult> PostClassesAnalytics(
        [FromBody] ClassResultDTO dto)
    {
        if (dto == null)
            return BadRequest("Payload required.");

        if (string.IsNullOrWhiteSpace(dto.UserId))
            return BadRequest("UserId required.");

        if (string.IsNullOrWhiteSpace(dto.ClassId))
            return BadRequest("ClassId required.");

        var result = await _analyticsRepository.PostClassesAnalytics(dto);

        return Ok(result);
    }

    // Post entered users
    [HttpPost("entered/{userId}/{entryTime}")]
    
    public async Task<IActionResult> AddUserToCrowd(string userId, DateTime entryTime)
    {
        var PostedUser = await _analyticsRepository.PostEnteredUser(userId, entryTime, DateTime.MinValue);
        return Ok(PostedUser);
    }
    
    // Change status to exit for entered users
    [HttpPut("Exited/{userId}/{exitTime}")]
    
    public async Task<IActionResult> UpdateUserExitTime(string userId, DateTime exitTime)
    {
        var UpdatedExitTime = await _analyticsRepository.UpdateUserExitTime(userId, exitTime);
        return Ok(UpdatedExitTime);
    }
    
    // Post solotraining results
    [HttpPost("solotraining")]
    
    public async Task<IActionResult> PostSoloTrainingResult([FromBody] SoloTrainingResultsDTO dto)
    {
        if (dto == null)
            return BadRequest("Payload required.");

        if (string.IsNullOrWhiteSpace(dto.UserId))
            return BadRequest("UserId required.");

        var result = await _analyticsRepository.PostSoloTrainingResult(dto);
        return Ok(result);
    }
    
    // Get crowd result
    [HttpGet("crowd")]
    [Authorize]
    public async Task<IActionResult> GetCrowdCount()
    {
        var crowdCount = await _analyticsRepository.GetCrowdCount();
        return Ok(crowdCount);
    }
    
    // Get all solo training result
    [HttpGet("solotrainingresult/{userId}")]
    [Authorize]
    public async Task<IActionResult> GetSoloTrainingResult(string userId)
    {
        var soloTrainingResult = await _analyticsRepository.GetSoloTrainingResult(userId);
        return Ok(soloTrainingResult);
    }
    
    // Get all class training results

    [HttpGet("classresult/{userId}")]
    [Authorize]
    public async Task<IActionResult> GetClassResult(string userId)
    {
        var classTrainingResult = await _analyticsRepository.GetClassResult(userId);
        return Ok(classTrainingResult);
    }
    
    [HttpGet("dashboard/{userId}")]
    [Authorize]
    public async Task<IActionResult> GetDashboard(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest("UserId required.");

        var dto = await _analyticsRepository.GetDashboardResult(userId);
        return Ok(dto);
    }
    
    [HttpGet("compare/month/{userId}")]
    [Authorize]
    public async Task<IActionResult> GetCompareForMonth(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest("UserId required.");

        var dto = await _analyticsRepository.GetCompareResultForCurrentMonth(userId);
        return Ok(dto);
    }
}