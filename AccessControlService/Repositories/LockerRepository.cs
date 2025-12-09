// AccessControlService/Repositories/LockerRepository.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using AccessControlService.Models;
namespace AccessControlService.Repositories;

public class LockerRepository : ILockerRepository
{
    private readonly IMongoCollection<LockerRoom> _lockerRooms;

    public LockerRepository(IMongoDatabase database)
    {
        
        _lockerRooms = database.GetCollection<LockerRoom>("LockerRoom");
    }

    public async Task<LockerRoom?> GetByIdAsync(int lockerRoomId)
    {
        return await _lockerRooms
            .Find(lr => lr.LockerRoomId == lockerRoomId)
            .FirstOrDefaultAsync();
    }
    
    public async Task SaveAsync(LockerRoom lockerRoom)
    {
        // Replace the whole document
        await _lockerRooms.ReplaceOneAsync(
            lr => lr.LockerRoomId == lockerRoom.LockerRoomId,
            lockerRoom,
            new ReplaceOptions { IsUpsert = true }  
        );
    }
}