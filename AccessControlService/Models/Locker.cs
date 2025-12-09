namespace AccessControlService.Models;

public class Locker
{
    public int LockerId { get; set; }
    public int UserId { get; set; }
    public bool IsLocked { get; set; }
}