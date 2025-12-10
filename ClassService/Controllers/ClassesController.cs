using Microsoft.AspNetCore.Mvc;
using ClassService.Model;
using Microsoft.AspNetCore.Authorization;

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

    [HttpPost("Classes")]
    // [Authorize(Roles = "Admin,Coach")]
    public async Task<ActionResult<FitnessClass>> CreateClassAsync(FitnessClass fitnessClass)
    {
        if (fitnessClass == null)
        {
            return BadRequest(new { error = "Invalid input", message = "Fitness class cannot be null." });
        }
        if (string.IsNullOrEmpty(fitnessClass.InstructorId))
        {
            return BadRequest(new { error = "Invalid input", message = "Instructor ID cannot be null or empty." });
        }
        if (string.IsNullOrEmpty(fitnessClass.CenterId))
        {
            return BadRequest(new { error = "Invalid input", message = "Center ID cannot be null or empty." });
        }
        if (fitnessClass.MaxCapacity <= 0)
        {
            return BadRequest(new { error = "Invalid input", message = "Capacity must be greater than zero." });
        }
        if (fitnessClass.Duration <= 0)
        {
            return BadRequest(new { error = "Invalid input", message = "Duration must be greater than zero." });
        }
        if (fitnessClass.Description == null)
        {
            return BadRequest(new { error = "Invalid input", message = "Description cannot exceed 500 characters." });
        }
        if (fitnessClass.Name == null)
        {
            return BadRequest(new { error = "Invalid input", message = "Name cannot be null." });
        }
        FitnessClass createdClass = await _classRepository.CreateClassAsync(fitnessClass);
        return Ok(createdClass);
    }
    [HttpGet("classes")]
    public async Task<ActionResult<IEnumerable<FitnessClass>>> GetAllActiveClassesAsync()
    {
        var classes = await _classRepository.GetAllActiveClassesAsync();
        return Ok(classes);
    }
    [HttpPut("classes/{classId}/{userId}")]
    public async Task<ActionResult> BookClassForUser(string classId, string userId)
    {
        try
        {
            var classes = await _classRepository.BookClassForUserNoSeatAsync(classId, userId);
            return Ok(classes);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "Booking failed", message = ex.Message });
        }

    }
    [HttpPut("classes/{classId}/{userId}/{seat}")]
    public async Task<ActionResult> BookClassForUserWithSeat(string classId, string userId, int seat)
    {
        if (string.IsNullOrEmpty(classId) || string.IsNullOrEmpty(userId))
        {
            return BadRequest(new { error = "Invalid input", message = "Class ID and User ID cannot be null or empty." });
        }
        if( seat < 0)
        {
            return BadRequest(new { error = "Invalid input", message = "Seat number must be non-negative." });
        }
        try
        {
            var classes = await _classRepository.BookClassForUserWithSeatAsync(classId, userId, seat);
            return Ok(classes);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "Booking failed", message = ex.Message });
        }
    }
    [HttpPut("classes/{classId}/{userId}/cancel")]
    public async Task<ActionResult> CancelClassBookingForUser(string classId, string userId)
    {
        if (string.IsNullOrEmpty(classId) || string.IsNullOrEmpty(userId))
        {
            return BadRequest(new { error = "Invalid input", message = "Class ID and User ID cannot be null or empty." });
        }
        try
        {
            var classes = await _classRepository.CancelClassBookingForUserAsync(classId, userId);
            return Ok(classes);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "Cancellation failed", message = ex.Message });
        }
    }
}