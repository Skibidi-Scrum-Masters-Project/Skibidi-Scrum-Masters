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

    [HttpGet]
    public ActionResult<IEnumerable<Workout>> GetSoloTrainings()
    {
        // TBA: Implement get all solo trainings
        return Ok(new { message = "Get solo trainings - TBA" });
    }
}