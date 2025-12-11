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
    private readonly HttpClient _httpClient = new HttpClient(); 
    public AccessControlRepository(IMongoDatabase database, HttpClient httpClient)
    {

        _lockerRooms = database.GetCollection<LockerRoom>("LockerRooms");
        _entryPoints = database.GetCollection<EntryPoint>("EntryPoints");
        _httpClient = httpClient;
    }

   

    public async Task<EntryPoint> OpenDoor(string userid)
    {
        EntryPoint entryPoint = new EntryPoint();
        entryPoint.UserId = userid;
        entryPoint.EnteredAt = DateTime.Now;
        entryPoint.ExitedAt = DateTime.MinValue;
        _entryPoints.InsertOne(entryPoint);

        await _httpClient.PostAsync(
            $"http://analyticsservice:8080/api/Analytics/entered/{userid}/{entryPoint.EnteredAt}",
            null);
        
        return entryPoint;
    }

    public async Task<EntryPoint> CloseDoor(string userid)
    {
        var filter = Builders<EntryPoint>.Filter.Eq("UserId", userid) & Builders<EntryPoint>.Filter.Eq("ExitedAt", DateTime.MinValue);
        var update = Builders<EntryPoint>.Update.Set("ExitedAt", DateTime.Now);
        var options = new FindOneAndUpdateOptions<EntryPoint>
        {
            ReturnDocument = ReturnDocument.After
        };

        var updatedEntryPoint = _entryPoints.FindOneAndUpdate(filter, update, options);

        if (updatedEntryPoint != null)
        {
            await _httpClient.PutAsync(
                $"http://analyticsservice:8080/api/Analytics/Exited/{userid}/{updatedEntryPoint.ExitedAt}",
                null);
        }

        return updatedEntryPoint;

    
    }

    public Task<List<Models.Locker>> GetAllAvailableLockers(string lockerRoomId)
    {
        if (lockerRoomId == null)
        {
            return Task.FromResult(new List<Locker>());
        }
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
        if (userId == null)
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
        if (userId == null)
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

    public Task<int> GetCrowd()
    {
        var count = _entryPoints.CountDocuments(ep => ep.ExitedAt == DateTime.MinValue);
        return Task.FromResult((int)count);
    }

    public Task<LockerRoom?> GetByIdAsync(string id)
    {
        var lockerRoom = _lockerRooms.Find(lr => lr.Id == id).FirstOrDefault();
        return Task.FromResult(lockerRoom);
    }

    public Task<LockerRoom> SaveAsync(LockerRoom lockerRoom)
    {
        _lockerRooms.ReplaceOne(lr => lr.Id == lockerRoom.Id, lockerRoom);
        return Task.FromResult(lockerRoom);
    }
}