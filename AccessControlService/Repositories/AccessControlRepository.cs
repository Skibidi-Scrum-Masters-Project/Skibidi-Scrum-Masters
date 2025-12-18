// AccessControlService/Repositories/AccessControlRepository.cs
using System;
using System.Collections.Generic;
using System.Linq;
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
    private readonly HttpClient _httpClient;
    
    public AccessControlRepository(IMongoDatabase database, HttpClient httpClient)
    {
        _lockerRooms = database.GetCollection<LockerRoom>("LockerRooms");
        _entryPoints = database.GetCollection<EntryPoint>("EntryPoints");
        _httpClient = httpClient;
        try
        {
            var existing = _lockerRooms.CountDocuments(Builders<LockerRoom>.Filter.Empty);
            if (existing == 0)
            {
                Console.WriteLine("LockerRooms collection empty — seeding default locker room with 20 lockers");
                var lockers = Enumerable.Range(1, 20)
                    .Select(i => new Locker
                    {
                        LockerId = i.ToString(),
                        IsLocked = false,
                        UserId = null
                    })
                    .ToList();

                var lockerRoom = new LockerRoom
                {
                    Capacity = lockers.Count,
                    Lockers = lockers
                };

                _lockerRooms.InsertOne(lockerRoom);
                Console.WriteLine("Seeded locker room with 20 lockers");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while seeding locker room: {ex.Message}");
        }
    }

   

    public async Task<EntryPoint> OpenDoor(string userid)
    {
        EntryPoint entryPoint = new EntryPoint();
        entryPoint.UserId = userid;
        entryPoint.EnteredAt = DateTime.Now;
        entryPoint.ExitedAt = DateTime.MinValue;
        _entryPoints.InsertOne(entryPoint);

        try
        {
            var url = $"http://analyticsservice:8080/api/Analytics/entered/{userid}/{Uri.EscapeDataString(entryPoint.EnteredAt.ToString("o"))}";
            Console.WriteLine($"Calling analytics: {url}");
            var response = await _httpClient.PostAsync(url, null);
            Console.WriteLine($"Analytics response: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calling analytics service: {ex.Message}");
        }
        
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
            try
            {
                var url = $"http://analyticsservice:8080/api/Analytics/Exited/{userid}/{Uri.EscapeDataString(updatedEntryPoint.ExitedAt.ToString("o"))}";
                Console.WriteLine($"Calling analytics (exit): {url}");
                var response = await _httpClient.PutAsync(url, null);
                Console.WriteLine($"Analytics exit response: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling analytics service (exit): {ex.Message}");
            }
        }

        return updatedEntryPoint;

    
    }

    public Task<List<Locker>> GetAllAvailableLockers(string lockerRoomId)
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

    public Task<DateTime?> GetUserStatus(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Task.FromResult<DateTime?>(null);
        
        var entry = _entryPoints
            .Find(ep => ep.UserId == userId)
            .SortByDescending(ep => ep.EnteredAt)
            .FirstOrDefault();

        if (entry == null)
        {
            // User has never checked in
            return Task.FromResult<DateTime?>(null);
        }

        // Return ExitedAt
        return Task.FromResult<DateTime?>(entry.ExitedAt);
    }

    public Task<int> GetCrowd()
    {
        var count = _entryPoints.CountDocuments(ep => ep.ExitedAt == DateTime.MinValue);
        return Task.FromResult((int)count);
    }

    public Task<Locker?> GetLocker(string lockerRoomId, string userId)
    {
        if (string.IsNullOrEmpty(lockerRoomId) || string.IsNullOrEmpty(userId))
        {
            return Task.FromResult<Locker?>(null);
        }

        var lockerRoom = _lockerRooms
            .Find(lr => lr.Id == lockerRoomId)
            .FirstOrDefault();

        if (lockerRoom?.Lockers == null)
        {
            return Task.FromResult<Locker?>(null);
        }

        var locker = lockerRoom.Lockers
            .FirstOrDefault(l => l.UserId == userId);

        return Task.FromResult(locker);
    }

    public async Task<string> GetLockerRoomId()
    {
        var lockerRoomId = await _lockerRooms
            .Find(_ => true)
            .Project(x => x.Id)
            .FirstOrDefaultAsync();

        if (lockerRoomId == null)
            throw new InvalidOperationException("No locker room found.");

        return lockerRoomId;
    }



}