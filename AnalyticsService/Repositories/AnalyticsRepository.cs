
public class AnalyticsRepository : IAnalyticsRepository
{
    private readonly HttpClient _httpClient;

    public AnalyticsRepository(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<int> GetCrowd()
    {
        var response = await _httpClient.GetAsync("http://accesscontrolservice/api/AccessControl/crowd");
        response.EnsureSuccessStatusCode();
        var crowd = await response.Content.ReadAsStringAsync();
        return int.Parse(crowd);
    }
}