using Microsoft.AspNetCore.Mvc;
using FitnessApp.Shared.Models;

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
}