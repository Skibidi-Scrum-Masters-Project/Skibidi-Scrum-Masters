using System.Runtime.CompilerServices;
using ClassService.Model;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

public class ClassRepository : IClassRepository
{
    private readonly IMongoCollection<FitnessClass> _classesCollection;

    public ClassRepository(IMongoDatabase database)
    {
        _classesCollection = database.GetCollection<FitnessClass>("Classes");
    }

    public async Task<FitnessClass> AddUserToClassWaitlistAsync(string classId, string userId)
    {
        FitnessClass? fitnessClass = await GetClassByIdAsync(classId);
        if (fitnessClass.WaitlistUserIds.Contains(userId))
        {
            throw new Exception("User is already on the waitlist for this class.");
        }
        _classesCollection.UpdateOne(
            c => c.Id == classId,
            Builders<FitnessClass>.Update.Push(c => c.WaitlistUserIds, userId)
        );
        FitnessClass? updatedClass = await GetClassByIdAsync(classId);
        return updatedClass;
    }

    public async Task<FitnessClass> BookClassForUserNoSeatAsync(string classId, string userId)
    {
        FitnessClass? fitnessClass = _classesCollection.Find(c => c.Id == classId).FirstOrDefault();
        if (fitnessClass.BookingList.Any(b => b.UserId == userId))
        {
            throw new Exception("User already booked in this class.");
        }
        if (fitnessClass.MaxCapacity <= fitnessClass.BookingList.Count)
        {
            await AddUserToClassWaitlistAsync(classId, userId);
            FitnessClass? updatedClassWaitlist = await GetClassByIdAsync(classId);
            return updatedClassWaitlist;
        }
        Booking booking = new Booking
        {
            UserId = userId,
            SeatNumber = 0,
            CheckedInAt = DateTime.MinValue
        };
        _classesCollection.UpdateOne(
            c => c.Id == classId,
            Builders<FitnessClass>.Update.Push(c => c.BookingList, booking)
        );
        FitnessClass? updatedClass = await GetClassByIdAsync(classId);
        return updatedClass;
    }

    public async Task<FitnessClass> BookClassForUserWithSeatAsync(string classId, string userId, int seatNumber)
    {
        FitnessClass? fitnessClass = _classesCollection.Find(c => c.Id == classId).FirstOrDefault();
        if (fitnessClass.SeatBookingEnabled == false)
        {
            throw new Exception("Seat booking is not enabled for this class.");
        }

        if (fitnessClass.BookingList.Any(b => b.UserId == userId))
        {
            throw new Exception("User already booked in this class.");
        }

        if (fitnessClass.SeatMap![seatNumber] == true)
        {
            throw new Exception("Seat already booked.");
        }

        if (fitnessClass.MaxCapacity <= fitnessClass.BookingList.Count)
        {
            await AddUserToClassWaitlistAsync(classId, userId);
            FitnessClass? updatedClassWaitlist = await GetClassByIdAsync(classId);
            return updatedClassWaitlist;
        }
        Booking booking = new Booking
        {
            UserId = userId,
            SeatNumber = seatNumber,
            CheckedInAt = DateTime.MinValue
        };
        await _classesCollection.UpdateOneAsync(
            c => c.Id == classId,
            Builders<FitnessClass>.Update.Push(c => c.BookingList, booking)
        );
        await _classesCollection.UpdateOneAsync(
            c => c.Id == classId,
            Builders<FitnessClass>.Update.Set(c => c.SeatMap![seatNumber], true)
        );
        FitnessClass? updatedClass = await GetClassByIdAsync(classId);
        return updatedClass;
    }

   public async Task<FitnessClass> CancelClassBookingForUserAsync(string classId, string userId)
{
    //Get the class
    FitnessClass? fitnessClass = await GetClassByIdAsync(classId);
    if (fitnessClass == null)
    {
        throw new Exception("Class not found.");
    }

    Booking? bookingToRemove = fitnessClass.BookingList.FirstOrDefault(b => b.UserId == userId);
    string? waitlistToRemove = fitnessClass.WaitlistUserIds.FirstOrDefault(id => id == userId);
    
    //If user has no booking or waitlist entry, throw error
    if (bookingToRemove == null && waitlistToRemove == null)
    {
        throw new Exception("User does not have a booking or waitlist entry in this class.");
    }

    // Handle waitlist cancellation (user is on waitlist, not booked)
    if (bookingToRemove == null && waitlistToRemove != null)
    {
        await _classesCollection.UpdateOneAsync(
            c => c.Id == classId,
            Builders<FitnessClass>.Update.Pull(c => c.WaitlistUserIds, userId)
        );
        return await GetClassByIdAsync(classId);
    }

    // Handle booking cancellation
    if (bookingToRemove != null)
    {
        //remove the seat from seatmap if applicable
        if (fitnessClass.SeatBookingEnabled)
        {
            await _classesCollection.UpdateOneAsync(
                c => c.Id == classId,
                Builders<FitnessClass>.Update.Set(c => c.SeatMap![bookingToRemove.SeatNumber], false)
            );
        }
        
        //Remove booking
        await _classesCollection.UpdateOneAsync(
            c => c.Id == classId,
            Builders<FitnessClass>.Update.PullFilter(c => c.BookingList, b => b.UserId == userId)
        );

        // Move waitlist to booking if there's a waitlist
        if (fitnessClass.WaitlistUserIds.Count > 0)
        {
            if (fitnessClass.SeatBookingEnabled)
            {
                await MoveWaitlistToBookingWithSeat(classId, bookingToRemove.SeatNumber);
            }
            else
            {
                await MoveWaitlistToBookingWithNoSeat(classId);
            }
        }
    }

    FitnessClass? updatedClass = await GetClassByIdAsync(classId);
    return updatedClass;
}

    public async Task<FitnessClass> CreateClassAsync(FitnessClass fitnessClass)
    {
        if (fitnessClass.SeatBookingEnabled)
        {
            fitnessClass.SeatMap = new bool[fitnessClass.MaxCapacity];
        }
        fitnessClass.IsActive = true;
        await _classesCollection.InsertOneAsync(fitnessClass);
        return fitnessClass;
    }

    public Task DeleteClassAsync(string classId)
    {
        try
        {
            FitnessClass? fitnessClass = GetClassByIdAsync(classId).Result;
            if (fitnessClass == null)
            {
                throw new Exception("Class not found.");
            }
        }
        catch (Exception)
        {
            throw new Exception("Class not found.");
        }
        return _classesCollection.DeleteOneAsync(c => c.Id == classId);
    }

    public Task<IEnumerable<FitnessClass>> GetAllActiveClassesAsync()
    {
        List<FitnessClass> classes = _classesCollection.Find(c => c.IsActive).ToList();
        if (classes == null)
        {
            return Task.FromResult(Enumerable.Empty<FitnessClass>());
        }
        return Task.FromResult(classes.AsEnumerable());
    }

    public Task<FitnessClass> GetClassByIdAsync(string classId)
    {
        FitnessClass? fitnessClass = _classesCollection.Find(c => c.Id == classId).FirstOrDefault();
        return Task.FromResult(fitnessClass);
    }

    public async Task MoveWaitlistToBookingWithNoSeat(string classId)
    {
        Booking booking = new Booking();
        FitnessClass? fitnessClass = GetClassByIdAsync(classId).Result;
        string? nextUserId = fitnessClass.WaitlistUserIds.FirstOrDefault();

        booking.UserId = nextUserId!;
        booking.SeatNumber = 0;
        booking.CheckedInAt = DateTime.MinValue;

        await _classesCollection.UpdateOneAsync(
            c => c.Id == classId,
            Builders<FitnessClass>.Update.Push(c => c.BookingList, booking)
        );
        //remove from waitlist
        await _classesCollection.UpdateOneAsync(
            c => c.Id == classId,
            Builders<FitnessClass>.Update.Pull(c => c.WaitlistUserIds, nextUserId)
        );
    }

    public async Task MoveWaitlistToBookingWithSeat(string classId, int seatNumber)
    {
        Booking booking = new Booking();
        FitnessClass? fitnessClass = GetClassByIdAsync(classId).Result;
        string? nextUserId = fitnessClass.WaitlistUserIds.FirstOrDefault();

        booking.UserId = nextUserId!;
        booking.SeatNumber = seatNumber;
        booking.CheckedInAt = DateTime.MinValue;
        //Add booking
        await  _classesCollection.UpdateOneAsync(
            c => c.Id == classId,
            Builders<FitnessClass>.Update.Push(c => c.BookingList, booking)
        );
        //Remove from waitlist
         await _classesCollection.UpdateOneAsync(
            c => c.Id == classId,
            Builders<FitnessClass>.Update.Pull(c => c.WaitlistUserIds, nextUserId)
        );  
    }
}