namespace FitLifeFitness.Services;

public class SoloTrainingService
{
    private readonly HttpClient _httpClient;

    public SoloTrainingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    public async Task<HttpResponseMessage> GetWorkoutPrograms()
    {
        return await _httpClient.GetAsync($"/api/solotraining/programs");
    }
    public async Task<HttpResponseMessage> GetWorkoutProgramByIdAsync(string programId)
    {
        return await _httpClient.GetAsync($"/api/solotraining/programs/{programId}");
    }

    public async Task<HttpResponseMessage> GetWorkoutsAsync(string userId)
    {
        return await _httpClient.GetAsync($"/api/solotraining/workouts/{userId}");
    }

    public async Task<HttpResponseMessage> CreateWorkoutAsync(object workoutData)
    {
        return await _httpClient.PostAsJsonAsync("/api/solotraining/workouts", workoutData);
    }

    public async Task<HttpResponseMessage> GetExercisesAsync()
    {
        return await _httpClient.GetAsync("/api/solotraining/exercises");
    }
    public async Task<HttpResponseMessage> GetMostRecentSoloTrainingForUserAndProgramAsync(string userId, string programId)
    {
        return await _httpClient.GetAsync($"/api/solotraining/recent/{userId}/{programId}");
    }
    public async Task<HttpResponseMessage> CreateWorkoutProgramAsync(object programData)
    {
        return await _httpClient.PostAsJsonAsync("/api/solotraining/create/program", programData);
    }
    public async Task<HttpResponseMessage> CreateSoloTrainingSessionAsync(string userId, string programId, object soloTrainingData)
    {
        return await _httpClient.PostAsJsonAsync($"/api/solotraining/{userId}/{programId}", soloTrainingData);
    }
}
