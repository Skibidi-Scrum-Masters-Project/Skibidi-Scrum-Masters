namespace SocialService.Models;

public sealed record ClassResultEventDto(
    string EventId,
    string ClassId,
    string UserName,
    string UserId,
    double CaloriesBurned,
    double Watt,
    int DurationMin,
    DateTime Date
);
