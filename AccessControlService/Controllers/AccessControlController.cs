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
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateLockerRoom([FromBody] LockerRoom lockerRoom)
    {
        LockerRoom createdLockerRoom = await _accessControlRepository.CreateLockerRoom(lockerRoom);
        return Ok(createdLockerRoom);
    }

    [HttpPost("door/{userid}")]
    [Authorize]
    public async Task<IActionResult> OpenDoor(string userid)
    {
        var door = await _accessControlRepository.OpenDoor(userid);

        if (door == null)
        {
            return NotFound($"User {userid} not found");
        }

        return Ok(door);
    }
    [HttpPut("door/{userid}/close")]
    [Authorize]
    public async Task<IActionResult> CloseDoor(string userid)
    {
        var door = await _accessControlRepository.CloseDoor(userid);

        if (door == null)
        {
            return NotFound($"User {userid} not found");
        }
        return Ok(door);
    }




    //GET ALL AVAILABLE LOCKERS IN A LOCKER ROOM
    [HttpGet("{lockerRoomId}/available")]
    [Authorize]
    public async Task<IActionResult> GetAvailableLockersById(string lockerRoomId)
    {

        var availableLockers = await _accessControlRepository.GetAllAvailableLockers(lockerRoomId);
        return Ok(availableLockers);
    }


    //LOCK A LOCKER AND ASSIGN IT TO A USER
    [HttpPut("{lockerRoomId}/{lockerId}/{userId}")]
    [Authorize]
    public async Task<IActionResult> LockLocker(
        string lockerRoomId, string lockerId, string userId)
    {
        var locker = await _accessControlRepository.LockLocker(lockerRoomId, lockerId, userId);
        return Ok(locker);
    }


    // UNLOCK A LOCKER AND REMOVE USER ASSIGNMENT
        [HttpPut("{lockerRoomId}/{lockerId}/{userid}/open")]
        [Authorize]
        public async Task<IActionResult> OpenLocker(string lockerRoomId, string lockerId, string userId)
        {
            var locker = await _accessControlRepository.UnlockLocker(lockerRoomId, lockerId, userId);
            return Ok(locker);
        }
        [HttpGet("crowd")]
        [Authorize]
        public async Task<IActionResult> GetCrowd()
        {
            var crowd = await _accessControlRepository.GetCrowd();
            return Ok(crowd);
        }
    
    
            
    
}
