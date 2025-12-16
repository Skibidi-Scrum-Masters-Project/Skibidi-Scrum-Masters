namespace FitLifeFitness.Models;

public class LockerRoom
{
    
    public string? Id { get; set; }
   
    public int Capacity { get; set; }
    public List<Locker>? Lockers { get; set; }
}



