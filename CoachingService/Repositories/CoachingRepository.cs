using MongoDB.Driver;
using CoachingService.Models;
using System;

public class CoachingRepository : ICoachingRepository
{
    private readonly IMongoCollection<Session> _sessionsCollection;

    public CoachingRepository(IMongoDatabase database)
    {
       
        _sessionsCollection = database.GetCollection<Session>("Sessions"); 
    }

    public Session BookSession(Session session)
    {
        var existingSession = _sessionsCollection
            .Find(s => s.Id == session.Id)
            .FirstOrDefault();

        if (existingSession == null)
            throw new InvalidOperationException("Session not found.");

        if (existingSession.CurrentStatus != Session.Status.Available)
            throw new InvalidOperationException("Session is not available to be booked.");

        if (session.BookingForm == null)
            throw new InvalidOperationException("BookingForm is required.");

        var update = Builders<Session>.Update
            .Set(s => s.CurrentStatus, Session.Status.Planned)   // 0 → 1
            .Set(s => s.UserId, session.UserId)
            .Set(s => s.BookingForm, session.BookingForm);

        _sessionsCollection.UpdateOne(
            s => s.Id == existingSession.Id,
            update
        );

        existingSession.CurrentStatus = Session.Status.Planned;
        existingSession.UserId = session.UserId;
        existingSession.BookingForm = session.BookingForm;

        return existingSession;
    }
    public IEnumerable<Session> GetAllSessions()
    {
        return _sessionsCollection.Find(FilterDefinition<Session>.Empty).ToList();
    }
    
    public Session? GetSessionById(string id)
    {
        return _sessionsCollection.Find(s => s.Id == id).FirstOrDefault();
    }
    

    public Session CancelSession(string id)
    {
        var session = GetSessionById(id);
        if (session == null)
            throw new ArgumentNullException(nameof(session), "Session not found");

        session.CurrentStatus = Session.Status.Cancelled;

        var filter = Builders<Session>.Filter.Eq(s => s.Id, id);
        _sessionsCollection.ReplaceOne(filter, session);

        return session;
    }

    public Session CompleteSession(string id)
    {
        var session = GetSessionById(id);
        if (session == null)
            throw new ArgumentNullException(nameof(session), "Session not found");

        session.CurrentStatus = Session.Status.Completed;

        var filter = Builders<Session>.Filter.Eq(s => s.Id, id);
        _sessionsCollection.ReplaceOne(filter, session);

        return session;
    }

    public Session CreateSession(Session session)
    {
        _sessionsCollection.InsertOne(session);
        
        return  session;
    }



    public Session  DeleteSession(string id)
    {
        var deletedSession = _sessionsCollection
            .FindOneAndDelete(s => s.Id == id);

        return deletedSession;
    }


    public List<Session> GetAllAvaliableCoachSessions()
    {
        var allSessionsWithAvalibility = _sessionsCollection
            .Find(s => s.CurrentStatus == Session.Status.Available);

        return allSessionsWithAvalibility.ToList();
    }
    
    public List<Session> GetAllAvailableCoachSessionsForCoachId(string coachId)
    {
        return _sessionsCollection
            .Find(s => s.CurrentStatus == Session.Status.Available && s.CoachId == coachId)
            .ToList();
    }


    public List<Session> GetAllSessionsByCoachId(string coachId)
    {
        return _sessionsCollection
            .Find(s => s.CoachId == coachId)
            .ToList();
    }
}