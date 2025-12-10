using AccessControlService.Models;
using Microsoft.AspNetCore.Mvc;
using FitnessApp.Shared.Models;
using AccessControlService.Repositories;

namespace AccessControlService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccessControlController : ControllerBase
{
    private readonly IAccessControlRepository _accessControlRepository;

    public AccessControlController(IAccessControlRepository accessControlRepository)
    {
        _accessControlRepository = accessControlRepository;
    }

    [HttpPost("door/{userid}")]
    public async Task<IActionResult> Door(string userid)
    {
        var door = await _accessControlRepository.OpenDoor(userid);

        if (door == null)
        {
            return NotFound($"User {userid} not found");
        }
        
        return Ok(door);
    }
    

    
    // GET ALL AVAILABLE LOCKERS IN A LOCKER ROOM
    [HttpGet("{lockerRoomId}/available")]
    public async Task<IActionResult> GetAvailableLockers(int lockerRoomId)
    {
        // Retrieve locker room from database
        var lockerRoom = await _accessControlRepository.GetByIdAsync(lockerRoomId);
        if (lockerRoom == null)
            return NotFound("Locker room not found");
        
        // Filter available lockers (not locked and not assigned to a user)
        var availableLockers = lockerRoom.Lockers
            .Where(l => !l.IsLocked && l.UserId == 0)
            .ToList();
        
        // Return available lockers
        return Ok(availableLockers);
    }

    
    // LOCK A LOCKER AND ASSIGN IT TO A USER
    [HttpPut("{lockerRoomId}/{lockerId}/{userId}")]
    public async Task<IActionResult> LockLocker(
        int lockerRoomId, int lockerId, int userId)
    {
        // Retrieve locker room from database
        var lockerRoom = await _accessControlRepository.GetByIdAsync(lockerRoomId);
        if (lockerRoom == null)
            return NotFound("Locker room not found");

        // Find locker inside locker room
        var locker = lockerRoom.Lockers
            .FirstOrDefault(l => l.LockerId == lockerId);

        if (locker == null)
            return NotFound("Locker not found");

        // Lock the locker and assign user ID
        locker.UserId = userId;
        locker.IsLocked = true;

        // Save changes to database
        await _accessControlRepository.SaveAsync(lockerRoom);

        // Return result
        return Ok(new 
        { 
            lockerRoomId,
            lockerId,
            userId,
            isLocked = locker.IsLocked 
        });
    }

    
    // UNLOCK A LOCKER AND REMOVE USER ASSIGNMENT
    [HttpPut("{lockerRoomId}/{lockerId}/open")]
    public async Task<IActionResult> OpenLocker(int lockerRoomId, int lockerId)
    {
        // Retrieve locker room from database
        var lockerRoom = await _accessControlRepository.GetByIdAsync(lockerRoomId);
        
        if (lockerRoom == null)
            return NotFound("Locker room not found");
        
        // Find locker inside locker room
        var locker = lockerRoom.Lockers
            .FirstOrDefault(l => l.LockerId == lockerId);
        
        if (locker == null)
            return NotFound("Locker not found");

        // Unlock the locker and clear user assignment
        locker.UserId = 0;
        locker.IsLocked = false;
        
        // Save changes to database
        await _accessControlRepository.SaveAsync(lockerRoom);

        // Return result
        return Ok(new
        {
            lockerRoomId,
            lockerId,
            userId = locker.UserId,
            isLocked = locker.IsLocked
        });
    }
}
