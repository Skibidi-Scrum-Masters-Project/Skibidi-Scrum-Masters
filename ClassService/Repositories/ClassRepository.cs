using System.Runtime.CompilerServices;
using ClassService.Model;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Net.Http.Json;

public class ClassRepository : IClassRepository
{
    private readonly IMongoCollection<FitnessClass> _classesCollection;
    private readonly IMongoCollection<ClassResult> _classResultsCollection;
    private readonly IHttpClientFactory _httpClientFactory;


    public ClassRepository(IMongoDatabase database, IHttpClientFactory httpClientFactory)
    {
        _classesCollection = database.GetCollection<FitnessClass>("Classes");
        _classResultsCollection = database.GetCollection<ClassResult>("ClassResults");
        _httpClientFactory = httpClientFactory;

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

    public async Task FinishClass(string classId)
    {
        var finishedClass = await GetClassByIdAsync(classId);
        if (finishedClass == null) throw new Exception("Class not found.");

        var caloriesBurnedTotal = CalculateCaloriesBurned(
            finishedClass.Intensity,
            finishedClass.Category,
            finishedClass.Duration
        );

        var wattTotal = await CalculateWatt(
            finishedClass.Intensity,
            finishedClass.Category,
            finishedClass.Duration
        );

        var occurredAtUtc = DateTime.UtcNow;

        foreach (var attendant in finishedClass.BookingList)
        {
            var metric = new ClassResult
            {
                ClassId = classId,
                UserId = attendant.UserId,
                CaloriesBurned = caloriesBurnedTotal,
                Watt = wattTotal,
                DurationMin = finishedClass.Duration,
                Date = DateTime.UtcNow,
                EventId = Guid.NewGuid().ToString()
            };

            await _classResultsCollection.InsertOneAsync(metric);
            
        
            try
            {
                await NotifySocialService(metric);
                var analyticsClient = _httpClientFactory.CreateClient("AnalyticsService");

                await analyticsClient.PostAsJsonAsync(
                    "http://analyticsservice:8080/api/analytics/classes",
                    new ClassResult
                    {
                        ClassId = metric.ClassId,
                        UserId = metric.UserId,
                        CaloriesBurned = metric.CaloriesBurned,
                        Watt = metric.Watt,
                        Category = finishedClass.Category, // enum → JSON → enum
                        DurationMin = metric.DurationMin,
                        Date = metric.Date
                    }
                );

            }
            catch (Exception ex)
            {
                // Stopper ikke finish hvis socialservice er nede
                Console.WriteLine($"NotifySocialService failed for user {attendant.UserId} in class {classId}: {ex.Message}");
            }
            
        }

        finishedClass.IsActive = false;
        await _classesCollection.ReplaceOneAsync(c => c.Id == classId, finishedClass);
    }



    private async Task NotifySocialService(ClassResult evt)
    {
        var client = _httpClientFactory.CreateClient("SocialService");
        var res = await client.PostAsJsonAsync("/internal/events/class-workout-completed", evt);

        if (!res.IsSuccessStatusCode)
        {
            var body = await res.Content.ReadAsStringAsync();
            throw new Exception("SocialService event failed: " + (int)res.StatusCode + " " + body);
        }
    }


    public async Task<Double> CalculateWatt(Intensity intensity,
     Category category, int DurationMinutes)
    {
        if (Category.Yoga == category)
        {
            switch (intensity)
            {
                case Intensity.Easy:
                    return DurationMinutes * 30;
                case Intensity.Medium:
                    return DurationMinutes * 50;
                case Intensity.Hard:
                    return DurationMinutes * 70;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        else if (Category.Pilates == category)
        {
            switch (intensity)
            {
                case Intensity.Easy:
                    return DurationMinutes * 40;
                case Intensity.Medium:
                    return DurationMinutes * 60;
                case Intensity.Hard:
                    return DurationMinutes * 80;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        else if (Category.Crossfit == category)
        {
            switch (intensity)
            {
                case Intensity.Easy:
                    return DurationMinutes * 70;
                case Intensity.Medium:
                    return DurationMinutes * 90;
                case Intensity.Hard:
                    return DurationMinutes * 110;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        else if (Category.Spinning == category)
        {
            switch (intensity)
            {
                case Intensity.Easy:
                    return DurationMinutes * 50;
                case Intensity.Medium:
                    return DurationMinutes * 80;
                case Intensity.Hard:
                    return DurationMinutes * 100;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        else
        {
            throw new ArgumentOutOfRangeException();
        }
    }
    public double CalculateCaloriesBurned(Intensity intensity,
     Category category, int DurationMinutes)
    {
        if (Category.Yoga == category)
        {
            switch (intensity)
            {
                case Intensity.Easy:
                    return DurationMinutes * 4;
                case Intensity.Medium:
                    return DurationMinutes * 6;
                case Intensity.Hard:
                    return DurationMinutes * 8;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        else if (Category.Pilates == category)
        {
            switch (intensity)
            {
                case Intensity.Easy:
                    return DurationMinutes * 5;
                case Intensity.Medium:
                    return DurationMinutes * 7;
                case Intensity.Hard:
                    return DurationMinutes * 9;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        else if (Category.Crossfit == category)
        {
            switch (intensity)
            {
                case Intensity.Easy:
                    return DurationMinutes * 8;
                case Intensity.Medium:
                    return DurationMinutes * 10;
                case Intensity.Hard:
                    return DurationMinutes * 12;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        else if (Category.Spinning == category)
        {
            switch (intensity)
            {
                case Intensity.Easy:
                    return DurationMinutes * 6;
                case Intensity.Medium:
                    return DurationMinutes * 9;
                case Intensity.Hard:
                    return DurationMinutes * 11;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        else
        {
            throw new ArgumentOutOfRangeException();
        }
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
        await _classesCollection.UpdateOneAsync(
            c => c.Id == classId,
            Builders<FitnessClass>.Update.Push(c => c.BookingList, booking)
        );
        //Remove from waitlist
        await _classesCollection.UpdateOneAsync(
           c => c.Id == classId,
           Builders<FitnessClass>.Update.Pull(c => c.WaitlistUserIds, nextUserId)
       );
    }
    public async Task<IEnumerable<FitnessClass>> GetClassesByUserIdAsync(string userId)
    {
        //find classes where bookinglist or waitlist contains userId
        var filter = Builders<FitnessClass>.Filter.Or(
            Builders<FitnessClass>.Filter.ElemMatch(c => c.BookingList, b => b.UserId == userId),
            Builders<FitnessClass>.Filter.ElemMatch(c => c.WaitlistUserIds, id => id == userId)
        );
        var classes = _classesCollection.Find(filter).ToList();
        return classes;
    }

    public Task<IEnumerable<FitnessClass>> GetAllAvailableClassesAsync(string userId)
    {
        var filter = Builders<FitnessClass>.Filter.And(
            Builders<FitnessClass>.Filter.Eq(c => c.IsActive, true),
            Builders<FitnessClass>.Filter.Not(
                Builders<FitnessClass>.Filter.ElemMatch(c => c.BookingList, b => b.UserId == userId)
            ),
            Builders<FitnessClass>.Filter.Not(
                Builders<FitnessClass>.Filter.ElemMatch(c => c.WaitlistUserIds, id => id == userId)
            )
        );

        var classes = _classesCollection.Find(filter).ToList();
        return Task.FromResult(classes.AsEnumerable());
    }

    public Task<IEnumerable<FitnessClass>> GetClassesByCoachIdAsync(string coachId)
    {
        var filter = Builders<FitnessClass>.Filter.Eq(c => c.InstructorId, coachId);
        var classes = _classesCollection.Find(filter).ToList();
        return Task.FromResult(classes.AsEnumerable());
    }

    public Task<List<ClassResult>> GetUserStatisticsAsync(string userId)
    {

        List<ClassResult> results = _classResultsCollection.Find(r => r.UserId == userId).ToList();
        if (results == null)
        {
            return Task.FromResult(new List<ClassResult>());
        }
        return Task.FromResult(results);

        
    }
}