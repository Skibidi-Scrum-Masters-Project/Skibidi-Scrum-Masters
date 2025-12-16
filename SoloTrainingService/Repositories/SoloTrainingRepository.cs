using FitnessApp.Shared.Models;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Threading.Tasks;
using SoloTrainingService.Models;
using FitnessApp.SoloTrainingService.Models;


public class SoloTrainingRepository : ISoloTrainingRepository
{
    private readonly IMongoCollection<SoloTrainingSession> _SolotrainingCollection;
    private readonly IMongoCollection<WorkoutProgram> _workoutProgramCollection;
    private readonly HttpClient _httpClient;

    public SoloTrainingRepository(IMongoDatabase database, HttpClient httpClient)
    {
        _SolotrainingCollection = database.GetCollection<SoloTrainingSession>("SoloTrainingSessions");
        _httpClient = httpClient;
        _workoutProgramCollection = database.GetCollection<WorkoutProgram>("WorkoutPrograms");
    }

    public async Task<SoloTrainingSession> CreateSoloTraining(string userId, SoloTrainingSession soloTraining, string programId)
    {
        soloTraining.UserId = userId;
        soloTraining.WorkoutProgramId = programId;

        await _SolotrainingCollection.InsertOneAsync(soloTraining);

        try
        {
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

        try
        {
            var evt = new SoloTrainingCompletedEvent
            {
                UserId = userId,
                SoloTrainingSessionId = soloTraining.Id!,
                Date = soloTraining.Date,
                WorkoutProgramName = soloTraining.WorkoutProgramName,
                DurationMinutes = soloTraining.DurationMinutes,
                ExerciseCount = soloTraining.Exercises?.Count ?? 0
            };

            var response = await _httpClient.PostAsJsonAsync(
                "http://socialservice:8080/internal/events/solo-training-completed",
                evt
            );

            Console.WriteLine($"Social response: {response.StatusCode}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calling social service: {ex.Message}");
        }

        return soloTraining;
    }

    public async Task<WorkoutProgram> CreateWorkoutProgram(WorkoutProgram workoutProgram)
    {
        if (workoutProgram.ExerciseTypes == null)
        {
         throw new ArgumentException("ExerciseTypes cannot be null.");
        }
        await _workoutProgramCollection.InsertOneAsync(workoutProgram);
        return workoutProgram;
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

    public async Task<List<WorkoutProgram>> GetAllWorkoutPrograms()
    {
        return await _workoutProgramCollection.Find(_ => true).ToListAsync();
    }

    public async Task<SoloTrainingSession> GetMostRecentSoloTrainingForUser(string userId)
    {
        return await _SolotrainingCollection.Find(s => s.UserId == userId)
            .SortByDescending(s => s.Date)
            .FirstOrDefaultAsync();
    }

    public async Task<SoloTrainingSession?> GetMostRecentSoloTrainingForUserAndProgram(string userId, string programId)
    {
        var response = await _SolotrainingCollection.Find(s => s.UserId == userId && s.WorkoutProgramId == programId)
            .SortByDescending(s => s.Date)
            .FirstOrDefaultAsync();
            if (response == null)
            {
                return new SoloTrainingSession();
            }
            return response;
    }

    public async Task<WorkoutProgram?> GetWorkoutProgramById(string programId)
    {
        var filter = Builders<WorkoutProgram>.Filter.Eq(p => p.Id, programId);
        var response = await _workoutProgramCollection.Find(filter).FirstOrDefaultAsync();
        return response;
    }
}