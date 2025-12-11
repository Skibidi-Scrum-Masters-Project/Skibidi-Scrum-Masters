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
        // Find existing session by id
        var existingSession = _sessionsCollection
            .Find(s => s.Id == session.Id)
            .FirstOrDefault();

        if (existingSession == null)
        {
            // Decide what you want here:
            // throw, return null, or create a new session.
            throw new InvalidOperationException("Session not found.");
        }

        // Only allow booking when session is Available
        if (existingSession.CurrentStatus != Session.Status.Available)
        {
            throw new InvalidOperationException("Session is not available to be booked.");
        }

        // Build the update: set status to Planned, and copy user id from input
        var update = Builders<Session>.Update
            .Set(s => s.CurrentStatus, Session.Status.Planned)
            .Set(s => s.UserId, session.UserId);

        // Apply the update in MongoDB
        _sessionsCollection.UpdateOne(
            s => s.Id == existingSession.Id,
            update
        );

        // Update the in memory object so the caller gets the new state
        existingSession.CurrentStatus = Session.Status.Planned;
        existingSession.UserId = session.UserId;

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
        session.CoachId = session.UserId;
        
        _sessionsCollection.InsertOne(session);
        
        return  session;
    }



    public Session  DeleteSession(string id)
    {
        var deletedSession = _sessionsCollection
            .FindOneAndDelete(s => s.Id == id);

        return deletedSession;
    }
}