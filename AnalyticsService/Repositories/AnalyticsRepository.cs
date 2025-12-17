using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;
using AnalyticsService.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

public class AnalyticsRepository : IAnalyticsRepository
{
    
    private readonly IMongoCollection<ClassResultDTO> _classesResultsCollection;
    private readonly IMongoCollection<CrowdResultDTO> _crowdResultsCollection;
    private readonly IMongoCollection<SoloTrainingResultsDTO> _soloTrainingResultsCollection;

    public AnalyticsRepository(IMongoDatabase database, HttpClient httpClient)
    {
        if (database == null) throw new ArgumentNullException(nameof(database));
        _classesResultsCollection = database.GetCollection<ClassResultDTO>("ClassResults");
        _crowdResultsCollection = database.GetCollection<CrowdResultDTO>("CrowdResults");
        _soloTrainingResultsCollection = database.GetCollection<SoloTrainingResultsDTO>("SoloTrainingResults");
    }

    
    public async Task<ClassResultDTO> PostClassesAnalytics(ClassResultDTO dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        var entity = new ClassResultDTO
        {
            ClassId = dto.ClassId,
            UserId = dto.UserId,
            CaloriesBurned = dto.CaloriesBurned,
            Category = dto.Category,
            Watt = dto.Watt,
            DurationMin = dto.DurationMin,
            Date = dto.Date
        };

        await _classesResultsCollection.InsertOneAsync(entity);

        return entity;
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

        var updated = await _crowdResultsCollection.FindOneAndUpdateAsync(filter, update, options)
            .ConfigureAwait(false);

        if (updated == null)
            throw new InvalidOperationException("No matching crowd entry found to update.");

        return updated.Id ?? "User exit time updated successfully";
    }

    public async Task<string> PostSoloTrainingResult(SoloTrainingResultsDTO dto)
    {
        if (dto == null)
            throw new ArgumentNullException(nameof(dto));

        if (string.IsNullOrWhiteSpace(dto.UserId))
            throw new ArgumentNullException(nameof(dto.UserId));

        if (dto.Exercises == null || dto.Exercises.Count == 0)
            throw new ArgumentNullException(nameof(dto.Exercises));

        if (dto.DurationMinutes < 0)
            throw new ArgumentOutOfRangeException(nameof(dto.DurationMinutes));

        var entity = new SoloTrainingResultsDTO
        {
            UserId = dto.UserId,
            Date = dto.Date,
            TrainingType = dto.TrainingType,
            DurationMinutes = dto.DurationMinutes,
            Exercises = dto.Exercises
        };

        await _soloTrainingResultsCollection.InsertOneAsync(entity)
            .ConfigureAwait(false);

        return entity.Id ?? "Solo training result added successfully";
    }

    public async Task<List<SoloTrainingResultsDTO>> GetSoloTrainingResult(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId));

        var filter = Builders<SoloTrainingResultsDTO>
            .Filter.Eq(x => x.UserId, userId);

        return await _soloTrainingResultsCollection
            .Find(filter)
            .ToListAsync();
    }


    public async Task<List<ClassResultDTO>> GetClassResult(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId));

        var filter = Builders<ClassResultDTO>
            .Filter.Eq(x => x.UserId, userId);

        return await _classesResultsCollection
            .Find(filter)
            .ToListAsync();
    }
    public async Task<AnalyticsDashboardDTO> GetDashboardResult(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentNullException(nameof(userId));

        var classTask = GetClassResult(userId);
        var soloTask = GetSoloTrainingResult(userId);
        var crowdTask = GetCrowdCount();

        await Task.WhenAll(classTask, soloTask, crowdTask).ConfigureAwait(false);

        return new AnalyticsDashboardDTO
        {
            UserId = userId,
            CrowdCount = crowdTask.Result,
            ClassResults = classTask.Result ?? new(),
            SoloTrainingResults = soloTask.Result ?? new(),
            ServerTimeUtc = DateTime.UtcNow
        };
    }
    
    public async Task<AnalyticsCompareDTO> GetCompareResultForCurrentMonth(string userId)
{
    if (string.IsNullOrWhiteSpace(userId))
        throw new ArgumentNullException(nameof(userId));

    var now = DateTime.UtcNow;
    var periodStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
    var periodEnd = periodStart.AddMonths(1);

    // 1) dine egne metrics for måned
    var yourClassesFilter = Builders<ClassResultDTO>.Filter.Eq(x => x.UserId, userId) &
                            Builders<ClassResultDTO>.Filter.Gte(x => x.Date, periodStart) &
                            Builders<ClassResultDTO>.Filter.Lt(x => x.Date, periodEnd);

    var yourSoloFilter = Builders<SoloTrainingResultsDTO>.Filter.Eq(x => x.UserId, userId) &
                         Builders<SoloTrainingResultsDTO>.Filter.Gte(x => x.Date, periodStart) &
                         Builders<SoloTrainingResultsDTO>.Filter.Lt(x => x.Date, periodEnd);

    var yourClassesTask = _classesResultsCollection.Find(yourClassesFilter).ToListAsync();
    var yourSoloTask = _soloTrainingResultsCollection.Find(yourSoloFilter).ToListAsync();

    // 2) app averages (alle brugere) for måned, via aggregation
    var classAggTask = _classesResultsCollection.Aggregate()
        .Match(Builders<ClassResultDTO>.Filter.Gte(x => x.Date, periodStart) &
               Builders<ClassResultDTO>.Filter.Lt(x => x.Date, periodEnd))
        .Group(
            x => x.UserId,
            g => new
            {
                UserId = g.Key,
                Workouts = g.Count(),
                Minutes = g.Sum(x => x.DurationMin),
                Calories = g.Sum(x => x.CaloriesBurned)
            }
        )
        .ToListAsync();

    var soloAggTask = _soloTrainingResultsCollection.Aggregate()
        .Match(Builders<SoloTrainingResultsDTO>.Filter.Gte(x => x.Date, periodStart) &
               Builders<SoloTrainingResultsDTO>.Filter.Lt(x => x.Date, periodEnd))
        .Group(
            x => x.UserId,
            g => new
            {
                UserId = g.Key,
                Workouts = g.Count(),
                Minutes = g.Sum(x => x.DurationMinutes)
            }
        )
        .ToListAsync();

    await Task.WhenAll(yourClassesTask, yourSoloTask, classAggTask, soloAggTask).ConfigureAwait(false);

    var yourClasses = yourClassesTask.Result ?? new();
    var yourSolo = yourSoloTask.Result ?? new();

    var classAgg = classAggTask.Result ?? new();
    var soloAgg = soloAggTask.Result ?? new();

    // merge aggregates pr userId
    var merged = new Dictionary<string, (int Workouts, double Minutes, double Calories)>(StringComparer.Ordinal);

    foreach (var c in classAgg)
    {
        merged[c.UserId] = (c.Workouts, c.Minutes, c.Calories);
    }

    foreach (var s in soloAgg)
    {
        if (!merged.TryGetValue(s.UserId, out var cur))
            cur = (0, 0, 0);

        merged[s.UserId] = (cur.Workouts + s.Workouts, cur.Minutes + s.Minutes, cur.Calories);
    }

    // active members = brugere som har mindst 1 workout i perioden
    var activeMembers = merged.Count;

    // leaderboard score: minutter (du kan ændre til calories eller workouts)
    var leaderboard = merged
        .Select(kv => new { UserId = kv.Key, Workouts = kv.Value.Workouts, Minutes = kv.Value.Minutes, Calories = kv.Value.Calories })
        .OrderByDescending(x => x.Minutes)
        .Take(5)
        .ToList();

    // rank: din placering i samme sortering
    var sortedAll = merged
        .Select(kv => new { UserId = kv.Key, Workouts = kv.Value.Workouts, Minutes = kv.Value.Minutes, Calories = kv.Value.Calories })
        .OrderByDescending(x => x.Minutes)
        .ToList();

    var yourIndex = sortedAll.FindIndex(x => x.UserId == userId);
    var rank = yourIndex >= 0 ? yourIndex + 1 : 0;

    // averages på tværs af aktive brugere
    double avgWorkouts = 0;
    double avgMinutes = 0;
    double avgCalories = 0;

    if (activeMembers > 0)
    {
        avgWorkouts = sortedAll.Average(x => (double)x.Workouts);
        avgMinutes = sortedAll.Average(x => x.Minutes);
        avgCalories = sortedAll.Average(x => x.Calories);
    }

    // dine egne values
    var yourWorkouts = yourClasses.Count + yourSolo.Count;
    var yourMinutes = yourClasses.Sum(x => (double)x.DurationMin) + yourSolo.Sum(x => x.DurationMinutes);
    var yourCalories = yourClasses.Sum(x => x.CaloriesBurned);

    var dto = new AnalyticsCompareDTO
    {
        UserId = userId,
        Rank = rank,
        ActiveMembers = activeMembers,
        PeriodStartUtc = periodStart,
        PeriodEndUtc = periodEnd,
        Metrics = new List<CompareMetricDTO>
        {
            new CompareMetricDTO { Name = "Træninger/måned", You = yourWorkouts, Avg = avgWorkouts, Unit = "" },
            new CompareMetricDTO { Name = "Minutter/måned", You = yourMinutes, Avg = avgMinutes, Unit = "" },
            new CompareMetricDTO { Name = "Kalorier/måned", You = yourCalories, Avg = avgCalories, Unit = "" },
        },
        Leaderboard = leaderboard.Select((x, i) => new LeaderboardRowDTO
        {
            Rank = i + 1,
            UserId = x.UserId,
            Score = x.Minutes,
            StatsText = $"{x.Workouts} træninger, {Math.Round(x.Minutes, 0)} min"
        }).ToList()
    };

    return dto;
}

}



