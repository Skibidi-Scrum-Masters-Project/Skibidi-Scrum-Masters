// AccessControlService/Repositories/AccessControlRepository.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using AccessControlService.Models;
using FitnessApp.Shared.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace AccessControlService.Repositories;

public class AccessControlRepository : IAccessControlRepository
{
    private readonly IMongoCollection<LockerRoom> _lockerRooms;
    private readonly IMongoCollection<EntryPoint> _entryPoints;

    public AccessControlRepository(IMongoDatabase database)
    {
        
        _lockerRooms = database.GetCollection<LockerRoom>("LockerRooms");
        _entryPoints = database.GetCollection<EntryPoint>("EntryPoints");
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

    public Task<EntryPoint> OpenDoor(string userid)
    {
        EntryPoint entryPoint = new EntryPoint();
        entryPoint.UserId = userid;
        entryPoint.EnteredAt = DateTime.Now;
        _entryPoints.InsertOne(entryPoint);
        
        return Task.FromResult(entryPoint);

        
        throw new NotImplementedException();
    }
}