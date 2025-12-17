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
    public async Task<IActionResult> CreateLockerRoom([FromBody] LockerRoom lockerRoom)
    {
        LockerRoom createdLockerRoom = await _accessControlRepository.CreateLockerRoom(lockerRoom);
        return Ok(createdLockerRoom);
    }

    [HttpPost("door/{userid}")]
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
    public async Task<IActionResult> GetAvailableLockersById(string lockerRoomId)
    {
        if (lockerRoomId == null)
        {
            return BadRequest();
        }

        var availableLockers = await _accessControlRepository.GetAllAvailableLockers(lockerRoomId);
        return Ok(availableLockers);
    }


        //LOCK A LOCKER AND ASSIGN IT TO A USER
        [HttpPut("{lockerRoomId}/{lockerId}/{userId}")]
        public async Task<IActionResult> LockLocker(
            string lockerRoomId, string lockerId, string userId)
        {
            if (lockerRoomId == null)
            {
                return BadRequest("LockerRoomId is required");
            }
            var locker = await _accessControlRepository.LockLocker(lockerRoomId, lockerId, userId);
            return Ok(locker);
        }
        
        
        // Get locker on user id
        [HttpGet("{lockerRoomId}/{userId}")]
        public async Task<IActionResult> GetLocker(string lockerRoomId,string userId)
        {
            if (lockerRoomId == null)
            {
                return BadRequest("LockerRoomId is required");
            }
            var locker = await _accessControlRepository.GetLocker(lockerRoomId, userId);
            return Ok(locker);
        }


        // UNLOCK A LOCKER AND REMOVE USER ASSIGNMENT
        [HttpPut("{lockerRoomId}/{lockerId}/{userId}/open")]
        public async Task<IActionResult> OpenLocker(string lockerRoomId, string lockerId, string userId)
        {
            var locker = await _accessControlRepository.UnlockLocker(lockerRoomId, lockerId, userId);
            if (locker == null)
            {
                return NotFound("Locker not found or already locked");
            }
            return Ok(locker);
            
        }
        
        // Get Crowd
        [HttpGet("crowd")]
        public async Task<IActionResult> GetCrowd()
        {
            var crowd = await _accessControlRepository.GetCrowd();
            return Ok(crowd);
        }

        [HttpGet("userstatus/{userid}")]
        public async Task<IActionResult> GetUserStatus(string userid)
        {
            var userStatus = await _accessControlRepository.GetUserStatus(userid);
            return Ok(userStatus); // null er gyldig status
        }
        
        
        [HttpGet("LockerRoomId")]
        public async Task<IActionResult> GetLockerRoomId()
        {
            var  lockerRoomId = await _accessControlRepository.GetLockerRoomId();
            return Ok(lockerRoomId);
        }
    
    
            
    
}
