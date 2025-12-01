using Microsoft.AspNetCore.Mvc;
using FitnessApp.Shared.Models;

namespace ClassService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClassesController : ControllerBase
{
    private readonly IClassRepository _classRepository;

    public ClassesController(IClassRepository classRepository)
    {
        _classRepository = classRepository;
    }

    [HttpGet]
    public ActionResult<IEnumerable<FitnessClass>> GetClasses()
    {
        // TBA: Implement get all classes
        return Ok(new { message = "Get classes - TBA" });
    }
}