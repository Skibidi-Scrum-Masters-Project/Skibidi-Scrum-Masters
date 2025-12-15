using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using AnalyticsService.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

public class AnalyticsRepository : IAnalyticsRepository
{
    private readonly HttpClient? _httpClient;

    private readonly IMongoCollection<ClassResultDTO> _classesResultsCollection;
    private readonly IMongoCollection<CrowdResultDTO> _crowdResultsCollection;
    private readonly IMongoCollection<SoloTrainingResultsDTO> _soloTrainingResultsCollection;
    
    public AnalyticsRepository(IMongoDatabase database, HttpClient httpClient)
    {
        if (database == null) throw new ArgumentNullException(nameof(database));
        _classesResultsCollection = database.GetCollection<ClassResultDTO>("ClassResults");
        _crowdResultsCollection = database.GetCollection<CrowdResultDTO>("CrowdResults");
        _soloTrainingResultsCollection = database.GetCollection<SoloTrainingResultsDTO>("SoloTrainingResults");
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<ClassResultDTO> PostClassesAnalytics(string classId, string userId, double caloriesBurned, Double watt, Category category, int durationMin, DateTime date)
    {
        if (string.IsNullOrWhiteSpace(classId)) throw new ArgumentNullException(nameof(classId));
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentNullException(nameof(userId));
        if (durationMin < 0) throw new ArgumentOutOfRangeException(nameof(durationMin));

        var classResult = new ClassResultDTO
        {
            ClassId = classId,
            UserId = userId,
            CaloriesBurned = caloriesBurned,
            Category = category,
            Watt = watt,
            DurationMin = durationMin,
            Date = date
        };

        await _classesResultsCollection.InsertOneAsync(classResult).ConfigureAwait(false);

        return classResult;
    }

    public async Task<int> GetCrowdCount()
    {
        try
        {
            var filter = Builders<CrowdResultDTO>.Filter.Eq(x => x.Status, CrowdResultDTO.timestatus.Entered);
            var count = await _crowdResultsCollection.CountDocumentsAsync(filter).ConfigureAwait(false);
            return (int)count;
        }
        catch (Exception ex)
        {
            throw new Exception("Error retrieving crowd count", ex);
        }
    }

    public async Task<string> PostEnteredUser(string userId, DateTime entryTime, DateTime exitTime)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId));

        var crowdResult = new CrowdResultDTO
        {
            UserId = userId,
            EntryTime = entryTime,
            ExitTime = exitTime,
            Status = CrowdResultDTO.timestatus.Entered
        };

        await _crowdResultsCollection.InsertOneAsync(crowdResult).ConfigureAwait(false);

        // return inserted id or message
        return crowdResult.Id ?? "User entered crowd data posted successfully";
    }

    public async Task<string> UpdateUserExitTime(string userId, DateTime exitTime)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId));
        if (exitTime == DateTime.MinValue)
            throw new ArgumentException("Exit time cannot be DateTime.MinValue.", nameof(exitTime));

        var filter = Builders<CrowdResultDTO>.Filter.Eq(x => x.UserId, userId) &
                     Builders<CrowdResultDTO>.Filter.Eq(x => x.ExitTime, DateTime.MinValue);

        var update = Builders<CrowdResultDTO>.Update
            .Set(x => x.ExitTime, exitTime)
            .Set(x => x.Status, CrowdResultDTO.timestatus.Exited);

        var options = new FindOneAndUpdateOptions<CrowdResultDTO>
        {
            ReturnDocument = ReturnDocument.After
        };

        var updated = await _crowdResultsCollection.FindOneAndUpdateAsync(filter, update, options).ConfigureAwait(false);

        if (updated == null)
            throw new InvalidOperationException("No matching crowd entry found to update.");

        return updated.Id ?? "User exit time updated successfully";
    }

    public async Task<string> PostSoloTrainingResult(string userId, DateTime date, List<Exercise> exercises, TrainingTypes trainingType, double durationMinutes)
    {
        if (string.IsNullOrWhiteSpace(userId)) throw new ArgumentNullException(nameof(userId));
        if (exercises == null) throw new ArgumentNullException(nameof(exercises));
        if (durationMinutes < 0) throw new ArgumentOutOfRangeException(nameof(durationMinutes));

        var soloTrainingResults = new SoloTrainingResultsDTO
        {
            UserId = userId,
            Date = date,
            Exercises = exercises,
            TrainingType = trainingType,
            DurationMinutes = durationMinutes
        };
        
        await _soloTrainingResultsCollection.InsertOneAsync(soloTrainingResults).ConfigureAwait(false);

        return soloTrainingResults.Id ?? "Solo training result added successfully";
    }

    public async Task<List<SoloTrainingResultsDTO>> GetSoloTrainingResult(string userId)
    {
        if (userId == null) 
            throw new ArgumentNullException(nameof(userId));

        var filter = Builders<SoloTrainingResultsDTO>
            .Filter.Eq(x => x.UserId, userId);

        return await _soloTrainingResultsCollection
            .Find(filter)
            .ToListAsync();
    }

    public async Task<List<ClassResultDTO>> GetClassResult(string userId)
    {
        if (userId == null) 
            throw new ArgumentNullException(nameof(userId));

        var filter = Builders<ClassResultDTO>
            .Filter.Eq(x => x.UserId, userId);

        return await _classesResultsCollection
            .Find(filter)
            .ToListAsync();
    }
}
