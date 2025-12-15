using FitnessApp.Shared.Models;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Threading.Tasks;

public class SoloTrainingRepository : ISoloTrainingRepository
{
    private readonly IMongoCollection<SoloTrainingSession> _SolotrainingCollection;
    private readonly HttpClient _httpClient;

    public SoloTrainingRepository(IMongoDatabase database, HttpClient httpClient)
    {
        _SolotrainingCollection = database.GetCollection<SoloTrainingSession>("SoloTrainingSessions");
        _httpClient = httpClient;
    }

    public async Task<SoloTrainingSession> CreateSoloTraining(string userId, SoloTrainingSession soloTraining)
    {
        soloTraining.UserId = userId;

        await _SolotrainingCollection.InsertOneAsync(soloTraining);

        try
        {
            // soloTraining indeholder allerede: UserId, Date, Exercises, TrainingType, DurationMinutes
            var response = await _httpClient.PostAsJsonAsync(
                "http://analyticsservice:8080/api/Analytics/solotraining",
                soloTraining
            );

            Console.WriteLine($"Analytics response: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calling analytics service: {ex.Message}");
        }

        return soloTraining;
    }

    public async Task DeleteSoloTraining(string trainingId)
    {
        var filter = Builders<SoloTrainingSession>.Filter.Eq(s => s.Id, trainingId);

        var sessionToDelete = await _SolotrainingCollection
            .Find(filter)
            .FirstOrDefaultAsync();

        if (sessionToDelete == null)
        {
            throw new Exception("Solo training session not found.");
        }

        await _SolotrainingCollection.DeleteOneAsync(filter);
    }

    public async Task<List<SoloTrainingSession>> GetAllSoloTrainingsForUser(string userId)
    {
        var filter = Builders<SoloTrainingSession>.Filter.Eq(s => s.UserId, userId);

        var sessions = await _SolotrainingCollection.Find(filter).ToListAsync();

        return sessions ?? new List<SoloTrainingSession>();
    }

    public async Task<SoloTrainingSession> GetMostRecentSoloTrainingForUser(string userId)
    {
        return await _SolotrainingCollection.Find(s => s.UserId == userId)
            .SortByDescending(s => s.Date)
            .FirstOrDefaultAsync();
    }
}
