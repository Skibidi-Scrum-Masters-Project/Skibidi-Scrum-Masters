using CoachingService.Models;
using MongoDB.Driver;

public class CoachingRepository : ICoachingRepository
{
    private readonly IMongoCollection<Session> _sessionsCollection;
    
    public CoachingRepository(IMongoDatabase database)
    {
        _sessionsCollection= database.GetCollection<Session>("Sessions");
    }


    public Session CreateSession(Session session)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));

        // enforce simple invariants
        if (session.EndTime <= session.StartTime)
            throw new ArgumentException("EndTime must be after StartTime");

        if (session.BookingForm != null && session.BookingForm.CreatedAt == default)
            session.BookingForm.CreatedAt = DateTime.UtcNow;

        if (session.CurrentStatus == default)
            session.CurrentStatus = Session.Status.Planned;

        _sessionsCollection.InsertOne(session);

        return session;
    }
    
}