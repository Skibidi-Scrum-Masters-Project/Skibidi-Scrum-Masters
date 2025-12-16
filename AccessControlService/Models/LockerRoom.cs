namespace AccessControlService.Models;

public class LockerRoom
{
    public int LockerRoomId { get; set; }
    public int CenterId { get; set; }
    public int Capacity { get; set; }
    public List<Locker> Lockers { get; set; }
}