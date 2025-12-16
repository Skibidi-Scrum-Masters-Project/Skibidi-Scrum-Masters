namespace AccessControlService.Models;

public class Locker
{
    
    public string? LockerId { get; set; }
    public string? UserId { get; set; }
    public bool IsLocked { get; set; }
}