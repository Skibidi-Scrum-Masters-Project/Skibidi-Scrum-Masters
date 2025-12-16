namespace AccessControlService.Models;

public class EntryPoint
{
    public int UserId { get; set; }
    public DateTime EnteredAt { get; set; }
    public DateTime ExitedAt { get; set; }
}