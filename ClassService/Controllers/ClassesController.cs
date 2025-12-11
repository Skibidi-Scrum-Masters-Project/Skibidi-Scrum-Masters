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
    [HttpPut("classes/{classId}/{userId}/friends")]
    public async Task<ActionResult> BookClassForUserWithFriendsNoSeats(string classId, string userId, List<string> friends)
    {
        var classInfo = await _classRepository.GetClassByIdAsync(classId);
        if (classInfo == null)
        {
            return NotFound(new { error = "Class not found", message = $"Class with ID '{classId}' does not exist." });
        }
        if (classInfo.SeatBookingEnabled)
        {
            return BadRequest(new { error = "Invalid operation", message = "Class requires seat booking. Use the appropriate endpoint." });
        }
        if (classInfo.BookingList.Count + friends.Count + 1 > classInfo.MaxCapacity)
        {
            return BadRequest(new { error = "Booking failed", message = "Not enough available spots for the group booking." });
        }


        try
        {
            foreach (var id in friends)
            {
                var classes = await _classRepository.BookClassForUserNoSeatAsync(classId, id);
            }
            var classesMainUser = await _classRepository.BookClassForUserNoSeatAsync(classId, userId);
            return Ok(classesMainUser);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "Booking failed", message = ex.Message });
        }

    }
    [HttpPut("classes/{classId}/{userId}/friends/seats")]
    public async Task<ActionResult> BookClassForUserWithFriendsWithSeats(string classId, string userId, [FromBody] BookClassWithFriendsRequestDTO request)
    {
        var classInfo = await _classRepository.GetClassByIdAsync(classId);
        if (classInfo == null)
        {
            return NotFound(new { error = "Class not found", message = $"Class with ID '{classId}' does not exist." });
        }
        if (request.Seats.Count != request.Friends.Count + 1)
        {
            return BadRequest(new { error = "Invalid input", message = "Number of seats must match number of users (including main user)." });
        }
        if (!classInfo.SeatBookingEnabled)
        {
            return BadRequest(new { error = "Invalid operation", message = "Class doesnt have seat booking. Use the appropriate endpoint." });
        }
        if (classInfo.BookingList.Count + request.Friends.Count + 1 > classInfo.MaxCapacity)
        {
            return BadRequest(new { error = "Booking failed", message = "Not enough available spots for the group booking." });
        }
        foreach (var seat in request.Seats)
        {
            if (seat < 0 || seat >= classInfo.SeatMap!.Length || classInfo.SeatMap[seat])
            {
                return BadRequest(new { error = "Invalid input", message = $"Seat number {seat} is invalid or already booked." });
            }
        }
        // Book main user
        request.Friends.Insert(0, userId);

        foreach (var id in request.Friends)
        {
            var index = request.Friends.IndexOf(id);
            var seatNumber = request.Seats[index];
            try
            {
                var classes = await _classRepository.BookClassForUserWithSeatAsync(classId, id, seatNumber);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "Booking failed", message = $"Failed to book seat {seatNumber} for user {id}: {ex.Message}" });
            }
        }
        return Ok(await _classRepository.GetClassByIdAsync(classId));
    }
    [HttpPut("classes/{classId}/{userId}/{seat}")]
    public async Task<ActionResult> BookClassForUserWithSeat(string classId, string userId, int seat)
    {
        if (string.IsNullOrEmpty(classId) || string.IsNullOrEmpty(userId))
        {
            return BadRequest(new { error = "Invalid input", message = "Class ID and User ID cannot be null or empty." });
        }
        if (seat < 0)
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
    [HttpDelete("classes/{classId}")]
    public async Task<ActionResult> DeleteClass(string classId)
    {
        if (string.IsNullOrEmpty(classId))
        {
            return BadRequest(new { error = "Invalid input", message = "Class ID cannot be null or empty." });
        }
        var FitnessClass = await _classRepository.GetClassByIdAsync(classId);
        if (FitnessClass == null)
        {
            return NotFound(new { error = "Class not found", message = $"Class with ID '{classId}' does not exist." });
        }
        try
        {
            await _classRepository.DeleteClassAsync(classId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "Deletion failed", message = ex.Message });
        }
    }
    [HttpPost("classes/{classId}/finish")]
    public async Task<ActionResult> FinishClass(string classId)
    {
        if (string.IsNullOrEmpty(classId))
        {
            return BadRequest(new { error = "Invalid input", message = "Class ID cannot be null or empty." });
        }
        var FitnessClass = await _classRepository.GetClassByIdAsync(classId);
        if (FitnessClass == null)
        {
            return NotFound(new { error = "Class not found", message = $"Class with ID '{classId}' does not exist." });
        }
        try
        {
            await _classRepository.FinishClass(classId);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "Finishing class failed", message = ex.Message });
        }
    }
}