namespace AccessControlService.Repositories;

using AccessControlService.Models;

public interface ILockerRepository
{
    Task<LockerRoom?> GetByIdAsync(int lockerRoomId);
    Task SaveAsync(LockerRoom lockerRoom);
}