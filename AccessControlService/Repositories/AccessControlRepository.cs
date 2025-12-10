// AccessControlService/Repositories/AccessControlRepository.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using AccessControlService.Models;

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

    public async Task<LockerRoom?> GetByIdAsync(string lockerRoomId)
    {
        // FIND LOCKERS FOR LOCKER ROOM
        return await _lockerRooms
            .Find(lr => lr.Id == lockerRoomId)
            .FirstOrDefaultAsync();
    }
    
    public async Task SaveAsync(LockerRoom lockerRoom)
    {
        // REPLACE THE WHOLE DOCUMENT
        await _lockerRooms.ReplaceOneAsync(
            lr => lr.Id == lockerRoom.Id,
            lockerRoom,
            new ReplaceOptions { IsUpsert = true }  
        );
    }

    public Task<EntryPoint> OpenDoor(string userid)
    {
        EntryPoint entryPoint = new EntryPoint();
        entryPoint.UserId = userid;
        entryPoint.EnteredAt = DateTime.Now;
        entryPoint.ExitedAt = DateTime.MinValue;
        _entryPoints.InsertOne(entryPoint);
        
        return Task.FromResult(entryPoint);

        
        throw new NotImplementedException();
    }

    public Task<EntryPoint> CloseDoor(string userid)
    {
        var filter = Builders<EntryPoint>.Filter.Eq("UserId", userid) & Builders<EntryPoint>.Filter.Eq("ExitedAt", DateTime.MinValue);
        var update = Builders<EntryPoint>.Update.Set("ExitedAt", DateTime.Now);
        var options = new FindOneAndUpdateOptions<EntryPoint>
        {
            ReturnDocument = ReturnDocument.After
        };

        var updatedEntryPoint = _entryPoints.FindOneAndUpdate(filter, update, options);

        return Task.FromResult(updatedEntryPoint);

      
    }

    public Task<List<Models.Locker>> GetAllAvailableLockers(string lockerRoomId)
    {
        var lockerRoom = _lockerRooms.Find(lr => lr.Id == lockerRoomId).FirstOrDefault();

        if (lockerRoom == null)
        {
            return Task.FromResult(new List<Locker>());
        }

        var availableLockers = lockerRoom.Lockers?
            .Where(l => !l.IsLocked)
            .ToList() ?? new List<Locker>();

        return Task.FromResult(availableLockers);
    }

    public Task<LockerRoom> CreateLockerRoom(LockerRoom lockerRoom)
    {
        _lockerRooms.InsertOne(lockerRoom);
        return Task.FromResult(lockerRoom);
    }

    public Task<Locker> LockLocker(string lockerRoomId, string lockerId, string userId)
    {
        var lockerRoom = _lockerRooms.Find(lr => lr.Id == lockerRoomId).FirstOrDefault();

        if (lockerRoom == null)
        {
            return Task.FromResult<Locker>(null);
        }

        var locker = lockerRoom.Lockers?
            .FirstOrDefault(l => l.LockerId == lockerId);

        if (locker == null)
        {
            return Task.FromResult<Locker>(null);
        }
        if(userId == null)
        {
            return Task.FromResult<Locker>(null);
        }
        locker.UserId = userId;
        locker.IsLocked = true;

        _lockerRooms.ReplaceOne(lr => lr.Id == lockerRoomId, lockerRoom);

        return Task.FromResult(locker);
    }

    public Task<Locker> UnlockLocker(string lockerRoomId, string lockerId, string userId)
    {
        var lockerRoom = _lockerRooms.Find(lr => lr.Id == lockerRoomId).FirstOrDefault();

        if (lockerRoom == null)
        {
            return Task.FromResult<Locker>(null);
        }

        var locker = lockerRoom.Lockers?
            .FirstOrDefault(l => l.LockerId == lockerId);

        if (locker == null)
        {
            return Task.FromResult<Locker>(null);
        }
        if(userId == null)
        {
            return Task.FromResult<Locker>(null);
        }

        if (locker.UserId != userId)
        {
            return Task.FromResult<Locker>(null);
        }
        locker.UserId = null;
        locker.IsLocked = false;

        _lockerRooms.ReplaceOne(lr => lr.Id == lockerRoomId, lockerRoom);

        return Task.FromResult(locker);
    }
}