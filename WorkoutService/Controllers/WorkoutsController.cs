using Microsoft.AspNetCore.Mvc;
using FitnessApp.Shared.Models;

namespace WorkoutService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkoutsController : ControllerBase
{
    private readonly IWorkoutRepository _workoutRepository;

    public WorkoutsController(IWorkoutRepository workoutRepository)
    {
        _workoutRepository = workoutRepository;
    }

    [HttpGet]
    public ActionResult<IEnumerable<Workout>> GetWorkouts()
    {
        // TBA: Implement get all workouts
        return Ok(new { message = "Get workouts - TBA" });
    }
}