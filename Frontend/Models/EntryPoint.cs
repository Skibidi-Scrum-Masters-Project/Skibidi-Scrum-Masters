namespace FitLifeFitness.Models;

public class EntryPoint
{
   
    public string? Id { get; set; }
    public string? UserId { get; set; }
    public DateTime EnteredAt { get; set; }
    public DateTime ExitedAt { get; set; }
}