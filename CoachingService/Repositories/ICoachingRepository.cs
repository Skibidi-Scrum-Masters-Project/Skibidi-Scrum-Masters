using CoachingService.Models;
using FitnessApp.Shared.Models;

public interface ICoachingRepository
{
        Session BookSession(Session session);

        IEnumerable<Session> GetAllSessions();
        
        Session? GetSessionById(string id);
        
        Session CancelSession(string id);
        
        Session CompleteSession(string id);
        
        Session CreateSession(Session session);
        
        Session DeleteSession(string id);

        List<Session>GetAllAvaliableCoachSessions();
        
        List<Session> GetAllAvailableCoachSessionsForCoachId(string userId);
        
        List<Session> GetAllSessionsByCoachId(string coachId);

}