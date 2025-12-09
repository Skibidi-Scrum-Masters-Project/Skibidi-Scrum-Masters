using CoachingService.Models;
using FitnessApp.Shared.Models;

public interface ICoachingRepository
{
        Session BookSession(Session session);

        IEnumerable<Session> GetAllSessions();
}