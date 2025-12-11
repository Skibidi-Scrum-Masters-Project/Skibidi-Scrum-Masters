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
    [HttpPost("{classId}/{userId}/{totalcaloriesBurned}/{category}/{durationMin}/{date}")]
    public async Task<IActionResult> GetClassesAnalytics(string classId, string userId, double totalcaloriesBurned, string category, int durationMin, DateTime date)
    {
        var classResult = await _analyticsRepository.GetClassesAnalytics(classId, userId, totalcaloriesBurned, category, durationMin, date);
        return Ok(classResult);
    }


    // Endpoint to recieve crowd data
    [HttpPost("entered/{userId}/{entryTime}")]
    public async Task<IActionResult> AddUserToCrowd(string userId, DateTime entryTime)
    {
        var PostedUser = await _analyticsRepository.PostEnteredUser(userId, entryTime, DateTime.MinValue);
        return Ok(PostedUser);
    }

    [HttpPut("Exited/{userId}/{exitTime}")]
    public async Task<IActionResult> UpdateUserExitTime(string userId, DateTime exitTime)
    {
        var UpdatedExitTime = await _analyticsRepository.UpdateUserExitTime(userId, exitTime);
        return Ok(UpdatedExitTime);
    }
    
}