// AccessControlService/Repositories/AccessControlRepository.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using AccessControlService.Models;
namespace AccessControlService.Repositories;

public class AccessControlRepository : IAccessControlRepository
{
    private readonly IMongoCollection<LockerRoom> _lockerRooms;

    public AccessControlRepository(IMongoDatabase database)
    {
        
        _lockerRooms = database.GetCollection<LockerRoom>("LockerRooms");
    }

    public async Task<LockerRoom?> GetByIdAsync(int lockerRoomId)
    {
        // FIND LOCKERS FOR LOCKER ROOM
        return await _lockerRooms
            .Find(lr => lr.LockerRoomId == lockerRoomId)
            .FirstOrDefaultAsync();
    }
    
    public async Task SaveAsync(LockerRoom lockerRoom)
    {
        // REPLACE THE WHOLE DOCUMENT
        await _lockerRooms.ReplaceOneAsync(
            lr => lr.LockerRoomId == lockerRoom.LockerRoomId,
            lockerRoom,
            new ReplaceOptions { IsUpsert = true }  
        );
    }
}