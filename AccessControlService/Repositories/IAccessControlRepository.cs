using Microsoft.AspNetCore.Mvc;

namespace AccessControlService.Repositories;

using AccessControlService.Models;

public interface IAccessControlRepository
{
    Task<LockerRoom?> GetByIdAsync(int lockerRoomId);
    Task SaveAsync(LockerRoom lockerRoom);

    Task<EntryPoint> OpenDoor(string userid);
}