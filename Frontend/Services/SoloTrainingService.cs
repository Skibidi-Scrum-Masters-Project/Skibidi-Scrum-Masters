namespace FitLifeFitness.Services;

public class SoloTrainingService
{
    private readonly HttpClient _httpClient;

    public SoloTrainingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://localhost:4000");
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
}
