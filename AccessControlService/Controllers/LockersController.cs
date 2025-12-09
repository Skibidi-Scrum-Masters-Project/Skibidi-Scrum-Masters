using AccessControlService.Models;
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

    
    // FIND AVAILABLE LOCKERS
    [HttpGet("{lockerRoomId}/available")]
    public async Task<IActionResult> GetAvailableLockers(int lockerRoomId)
    {
        // GET ALL LOCKERS
        var lockerRoom = await _lockerRepository.GetByIdAsync(lockerRoomId);
        if (lockerRoom == null)
            return NotFound("Locker not found");
        
        // FILTER AVAILABLE LOCKERS
        var availableLockers = lockerRoom.Lockers
            .Where(l => !l.IsLocked && l.UserId == 0)
            .ToList();
        
        // RETURN RESULT
        return Ok(availableLockers);
    }

    
    // SET LOCK TO LOCKED
    [HttpPut("{lockerRoomId}/{lockerId}/{userId}")]
    public async Task<IActionResult> LockLocker(
        int lockerRoomId, int lockerId, int userId)
    {
        // GET LOCKER ROOM FROM DB
        var lockerRoom = await _lockerRepository.GetByIdAsync(lockerRoomId);
        if (lockerRoom == null)
            return NotFound("Locker room not found");

        // FIND LOCKER IN LOCKER ROOM
        var locker = lockerRoom.Lockers
            .FirstOrDefault(l => l.LockerId == lockerId);

        if (locker == null)
            return NotFound("Locker not found");

        // FIND LOCK ON USERID AND SET IT TO LOCKED
        locker.UserId = userId;
        locker.IsLocked = true;

        // SAVE IN DATABASE
        await _lockerRepository.SaveAsync(lockerRoom);

        // RETURN RESULT
        return Ok(new 
        { 
            lockerRoomId,
            lockerId,
            userId,
            isLocked = locker.IsLocked 
        });
    }
}