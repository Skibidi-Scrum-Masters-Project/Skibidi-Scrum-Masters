using Microsoft.AspNetCore.Mvc;
using FitnessApp.Shared.Models;
using AccessControlService.Repositories;

namespace AccessControlService.Controllers;

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

    [HttpPut("{lockerRoomId}/{lockerId}/{userId}")]
    public async Task<IActionResult> LockLocker(
        int lockerRoomId, int lockerId, int userId)
    {
        // get locker room from db
        var lockerRoom = await _lockerRepository.GetByIdAsync(lockerRoomId);
        if (lockerRoom == null)
            return NotFound("Locker room not found");

        // 2. Find locker in locker room
        var locker = lockerRoom.Lockers
            .FirstOrDefault(l => l.LockerId == lockerId);

        if (locker == null)
            return NotFound("Locker not found");

        // 3. Find lock on user id and set lock to locked
        locker.UserId = userId;
        locker.IsLocked = true;

        // 4. Save in database
        await _lockerRepository.SaveAsync(lockerRoom);

        // 5. Return Result
        return Ok(new 
        { 
            lockerRoomId,
            lockerId,
            userId,
            isLocked = locker.IsLocked 
        });
    }
}