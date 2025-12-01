using Microsoft.AspNetCore.Mvc;
using FitnessApp.Shared.Models;

namespace LockerService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LockersController : ControllerBase
{
    private readonly ILockerRepository _lockerRepository;

    public LockersController(ILockerRepository lockerRepository)
    {
        _lockerRepository = lockerRepository;
    }

    [HttpGet]
    public ActionResult<IEnumerable<Locker>> GetLockers()
    {
        // TBA: Implement get all lockers
        return Ok(new { message = "Get lockers - TBA" });
    }
}