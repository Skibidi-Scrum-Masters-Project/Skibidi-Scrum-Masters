namespace FitLifeFitness.Models;

public class CrowdResultDTO
{
    
    public string? Id { get; set; }
        
    public string UserId { get; set; } = string.Empty;
    public DateTime EntryTime { get; set; }

    public DateTime ExitTime { get; set; }

    public timestatus Status { get; set; }

    public enum timestatus
    {
        Entered,
        Exited
    }


}