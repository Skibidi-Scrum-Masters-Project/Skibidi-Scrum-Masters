using MongoDB.Driver;
using AnalyticsService.Models;

public class AnalyticsRepository : IAnalyticsRepository
{
    private readonly HttpClient? _httpClient;

    private readonly IMongoCollection<ClassResultDTO>? _classesResultsCollection;
    private readonly IMongoCollection<CrowdResultDTO>? _crowdResultsCollection;
    

    public AnalyticsRepository(IMongoDatabase database)
    {
        _classesResultsCollection = database.GetCollection<ClassResultDTO>("ClassResults");
        _crowdResultsCollection = database.GetCollection<CrowdResultDTO>("CrowdResults");
    }

    public AnalyticsRepository(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

   

    public Task<ClassResultDTO> GetClassesAnalytics(string classId, string userId, double totalcaloriesBurned, string category, int durationMin, DateTime date)
    {
        ClassResultDTO classResult = new ClassResultDTO
        {
            ClassId = classId,
            UserId = userId,
            TotalCaloriesBurned = totalcaloriesBurned,
            Category = category,
            DurationMin = durationMin,
            Date = date
        };

        _classesResultsCollection.InsertOne(classResult);

        return Task.FromResult(classResult);
    }

    public Task<string> PostEnteredUser(string userId, DateTime entryTime, DateTime exitTime)
    {
        if (userId == null)
        {
            throw new ArgumentNullException(nameof(userId));
        }
        if (exitTime < entryTime && exitTime != DateTime.MinValue)
        {
            throw new ArgumentException("Exit time cannot be earlier than entry time.");
        }
        
        CrowdResultDTO crowdResult = new CrowdResultDTO
        {
            UserId = userId,
            EntryTime = entryTime,
            ExitTime = exitTime,
            Status = CrowdResultDTO.timestatus.Entered
        };
        // Assuming you have a MongoDB collection for CrowdResults similar to ClassResults
        _crowdResultsCollection.InsertOne(crowdResult);

        return Task.FromResult("User entered crowd data posted successfully");
    }

    public Task<string> UpdateUserExitTime(string userId, DateTime exitTime)
    {
        if (userId == null)
        {
            throw new ArgumentNullException(nameof(userId));
        }
        if (exitTime == DateTime.MinValue)
        {
            throw new ArgumentException("Exit time cannot be DateTime.MinValue.");
        }
        var filter = Builders<CrowdResultDTO>.Filter.Eq("UserId", userId) & Builders<CrowdResultDTO>.Filter.Eq("ExitTime", DateTime.MinValue);
        var update = Builders<CrowdResultDTO>.Update.Combine(
        Builders<CrowdResultDTO>.Update.Set(x => x.ExitTime, exitTime),
        Builders<CrowdResultDTO>.Update.Set(x => x.Status, CrowdResultDTO.timestatus.Exited)
        );
        var options = new FindOneAndUpdateOptions<CrowdResultDTO>
        {
            ReturnDocument = ReturnDocument.After
        };

        var updatedCrowdResult = _crowdResultsCollection.FindOneAndUpdate(filter, update, options);

        return Task.FromResult("User exit time updated successfully");
    }
}