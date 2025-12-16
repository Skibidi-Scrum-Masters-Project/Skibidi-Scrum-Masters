using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace FitLifeFitness.Services;

public class SoloTrainingService
{
    private readonly HttpClient _httpClient;

    public SoloTrainingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    private void AddJwtHeader(string jwt)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jwt);
    }

    public async Task<HttpResponseMessage> GetWorkoutProgramsAsync(string jwt)
    {
        AddJwtHeader(jwt);
        return await _httpClient.GetAsync("/api/solotraining/programs");
    }

    public async Task<HttpResponseMessage> GetWorkoutProgramByIdAsync(string programId, string jwt)
    {
        AddJwtHeader(jwt);
        return await _httpClient.GetAsync($"/api/solotraining/programs/{programId}");
    }

    public async Task<HttpResponseMessage> GetWorkoutsAsync(string userId, string jwt)
    {
        AddJwtHeader(jwt);
        return await _httpClient.GetAsync($"/api/solotraining/workouts/{userId}");
    }

    public async Task<HttpResponseMessage> CreateWorkoutAsync(object workoutData, string jwt)
    {
        AddJwtHeader(jwt);
        return await _httpClient.PostAsJsonAsync("/api/solotraining/workouts", workoutData);
    }

    public async Task<HttpResponseMessage> GetExercisesAsync(string jwt)
    {
        AddJwtHeader(jwt);
        return await _httpClient.GetAsync("/api/solotraining/exercises");
    }

    public async Task<HttpResponseMessage> GetMostRecentSoloTrainingForUserAndProgramAsync(string userId, string programId, string jwt)
    {
        AddJwtHeader(jwt);
        return await _httpClient.GetAsync($"/api/solotraining/recent/{userId}/{programId}");
    }

    public async Task<HttpResponseMessage> CreateWorkoutProgramAsync(object programData, string jwt)
    {
        AddJwtHeader(jwt);
        return await _httpClient.PostAsJsonAsync("/api/solotraining/create/program", programData);
    }

    public async Task<HttpResponseMessage> CreateSoloTrainingSessionAsync(string userId, string programId, object soloTrainingData, string jwt)
    {
        AddJwtHeader(jwt);
        return await _httpClient.PostAsJsonAsync($"/api/solotraining/{userId}/{programId}", soloTrainingData);
    }
}
