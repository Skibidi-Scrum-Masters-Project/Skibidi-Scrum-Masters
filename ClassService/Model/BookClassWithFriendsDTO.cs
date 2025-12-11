namespace ClassService.Model;

public class BookClassWithFriendsRequestDTO
{
    public List<string> Friends { get; set; } = new();
    public List<int> Seats { get; set; } = new();
}