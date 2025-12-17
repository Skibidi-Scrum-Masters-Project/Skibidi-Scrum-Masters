using Microsoft.AspNetCore.Mvc;

namespace AccessControlService.Repositories;

using AccessControlService.Models;

public interface IAccessControlRepository
{
    Task<List<Locker>> GetAllAvailableLockers(string lockerRoomId);
    
    Task<LockerRoom> CreateLockerRoom(LockerRoom lockerRoom);

    Task<EntryPoint> OpenDoor(string userId);
    Task<EntryPoint> CloseDoor(string userId);
    Task<Locker> LockLocker(string lockerRoomId, string lockerId, string userId);

    Task<Locker> UnlockLocker(string lockerRoomId, string lockerId, string userId);
    Task<int> GetCrowd();

    Task<DateTime?> GetUserStatus(string userid);

    Task<Locker?> GetLocker(string lockerRoomId,string userId);

    Task<string> GetLockerRoomId();


}