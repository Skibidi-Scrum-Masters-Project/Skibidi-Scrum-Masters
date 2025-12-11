using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mongo2Go;
using MongoDB.Driver;
using CoachingService.Models;
using System;
using System.Linq;
using System.Collections.Generic;

namespace CoachingService.Tests
{
    [TestClass]
    public class CoachingRepositoryTests
    {
        private MongoDbRunner _runner = null!;
        private IMongoDatabase _database = null!;
        private CoachingRepository _repository = null!;
        private IMongoCollection<Session> _sessionsCollection = null!; // Added for direct setup

        [TestInitialize]
        public void Setup()
        {
            _runner = MongoDbRunner.Start();

            var client = new MongoClient(_runner.ConnectionString);
            _database = client.GetDatabase("test_coachingservice_db");
            _sessionsCollection = _database.GetCollection<Session>("Sessions"); // Initialize collection

            _repository = new CoachingRepository(_database);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _runner?.Dispose();
        }


        // --- BookSession Tests 

        [TestMethod]
        public void BookSession_ValidSession_UpdatesStatusAndUserId()
        {
            // Arrange
            const string userIdToBook = "user-abc";
            
            // 1. Create an available session first, as required by the repo logic
            var initialSession = new Session
            {
                CoachId = "coach-1",
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1),
                CurrentStatus = Session.Status.Available // Must be available
            };
            _sessionsCollection.InsertOne(initialSession);

            // 2. Create the input session object used for the PUT request
            var sessionToBook = new Session
            {
                Id = initialSession.Id, // Use the existing ID
                UserId = userIdToBook    // Provide the UserId
            };

            // Act
            var result = _repository.BookSession(sessionToBook);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(Session.Status.Planned, result.CurrentStatus);
            Assert.AreEqual(userIdToBook, result.UserId);

            // Verify update in database
            var stored = _sessionsCollection.Find(s => s.Id == initialSession.Id).FirstOrDefault();
            Assert.IsNotNull(stored);
            Assert.AreEqual(Session.Status.Planned, stored.CurrentStatus);
            Assert.AreEqual(userIdToBook, stored.UserId);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void BookSession_SessionNotFound_Throws()
        {
            // Arrange
            var nonExistentSession = new Session
            {
                Id = "000000000000000000000000",
                UserId = "user-id"
            };

            // Act
            _repository.BookSession(nonExistentSession);
            
            // Assert: Throws InvalidOperationException("Session not found.")
        }
        
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void BookSession_SessionNotAvailable_Throws()
        {
            // Arrange
            // Create a session that is already Planned (or any status other than Available)
            var plannedSession = new Session
            {
                CoachId = "coach-1",
                CurrentStatus = Session.Status.Planned
            };
            _sessionsCollection.InsertOne(plannedSession);
            
            // Create the input session object
            var sessionToBook = new Session
            {
                Id = plannedSession.Id, 
                UserId = "user-id"    
            };

            // Act
            _repository.BookSession(sessionToBook);
            
            // Assert: Throws InvalidOperationException("Session is not available to be booked.")
        }


        // --- GetAllSessions Tests 


        [TestMethod]
        public void GetAllSessions_ReturnsSessions()
        {
            // Arrange
            _sessionsCollection.InsertOne(new Session { CurrentStatus = Session.Status.Planned, CoachId = "C1" });
            _sessionsCollection.InsertOne(new Session { CurrentStatus = Session.Status.Available, CoachId = "C2" });
            
            // Act
            var result = _repository.GetAllSessions();

            // Assert
            Assert.AreEqual(2, result.Count());
        }

        [TestMethod]
        public void GetAllSessions_EmptyDatabase_ReturnsEmptyList()
        {
            // Arrange (Database is empty)
            
            // Act
            var result = _repository.GetAllSessions();
            
            // Assert
            Assert.AreEqual(0, result.Count());
        }


        // --- GetSessionById Tests 


        [TestMethod]
        public void GetSessionById_ValidId_ReturnsSession()
        {
            // Arrange
            var session = new Session
            {
                CurrentStatus = Session.Status.Available
            };
            _sessionsCollection.InsertOne(session);

            // Act
            var fetched = _repository.GetSessionById(session.Id!);

            // Assert
            Assert.IsNotNull(fetched);
            Assert.AreEqual(session.Id, fetched.Id);
        }

        [TestMethod]
        public void GetSessionById_NotFound_ReturnsNull()
        {
            // Arrange (Non-existent ID)
            
            // Act
            var result = _repository.GetSessionById("000000000000000000000000"); 
            
            // Assert
            Assert.IsNull(result);
        }


        // --- CancelSession Tests 

        [TestMethod]
        public void CancelSession_ValidId_StatusUpdated()
        {
            // Arrange
            var session = new Session
            {
                CurrentStatus = Session.Status.Planned
            };
            _sessionsCollection.InsertOne(session);

            // Act
            var updated = _repository.CancelSession(session.Id!);

            // Assert
            Assert.AreEqual(Session.Status.Cancelled, updated.CurrentStatus);

            // Verify in database
            var fetched = _repository.GetSessionById(session.Id!);
            Assert.AreEqual(Session.Status.Cancelled, fetched!.CurrentStatus);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CancelSession_NotFound_Throws()
        {
            // Act
            _repository.CancelSession("000000000000000000000001");
            
            // Assert: Throws ArgumentNullException("Session not found")
        }


        // --- CompleteSession Tests 

        [TestMethod]
        public void CompleteSession_ValidId_UpdatesStatusToCompleted()
        {
            // Arrange
            var session = new Session
            {
                CurrentStatus = Session.Status.Planned
            };
            _sessionsCollection.InsertOne(session);

            // Act
            var completed = _repository.CompleteSession(session.Id!);

            // Assert
            Assert.IsNotNull(completed);
            Assert.AreEqual(Session.Status.Completed, completed.CurrentStatus);

            // Verify in database
            var fetched = _repository.GetSessionById(session.Id!);
            Assert.IsNotNull(fetched);
            Assert.AreEqual(Session.Status.Completed, fetched.CurrentStatus);
        }
        
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CompleteSession_NotFound_Throws()
        {
            // Act
            _repository.CompleteSession("000000000000000000000002");
            
            // Assert: Throws ArgumentNullException("Session not found")
        }
        
        
        // --- CreateSession Tests

        [TestMethod]
        public void CreateSession_ValidSession_InsertsIntoDatabaseAndReturnsSessionWithId()
        {
            // Arrange
            var session = new Session { CoachId = "C1", CurrentStatus = Session.Status.Available };

            // Act
            var result = _repository.CreateSession(session);

            // Assert
            Assert.IsNotNull(result.Id);
            
            // Verify in database
            var stored = _sessionsCollection.Find(s => s.Id == result.Id).FirstOrDefault();
            Assert.IsNotNull(stored);
            Assert.AreEqual("C1", stored.CoachId);
        }
        
        
        // --- DeleteSession Tests 
        
        [TestMethod]
        public void DeleteSession_ValidId_DeletesFromDatabaseAndReturnsDeletedSession()
        {
            // Arrange
            var session = new Session { CurrentStatus = Session.Status.Planned };
            _sessionsCollection.InsertOne(session);

            // Act
            var deleted = _repository.DeleteSession(session.Id!);

            // Assert
            Assert.IsNotNull(deleted);
            
            // Verify deletion in database
            var stored = _sessionsCollection.Find(s => s.Id == session.Id).FirstOrDefault();
            Assert.IsNull(stored);
        }

        [TestMethod]
        public void DeleteSession_NotFound_ReturnsNull()
        {
            // Act
            var deleted = _repository.DeleteSession("000000000000000000000003");

            // Assert
            Assert.IsNull(deleted);
        }
        
        
        // --- GetAllAvaliableCoachSessions Tests 
        
        [TestMethod]
        public void GetAllAvaliableCoachSessions_ReturnsOnlyAvailableSessions()
        {
            // Arrange
            _sessionsCollection.InsertOne(new Session { CurrentStatus = Session.Status.Available }); // Should be returned
            _sessionsCollection.InsertOne(new Session { CurrentStatus = Session.Status.Planned });   // Should NOT be returned
            _sessionsCollection.InsertOne(new Session { CurrentStatus = Session.Status.Available }); // Should be returned
            
            // Act
            var result = _repository.GetAllAvaliableCoachSessions();

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(s => s.CurrentStatus == Session.Status.Available));
        }

        
        // --- GetAllAvailableCoachSessionsForCoachId Tests
        
        [TestMethod]
        public void GetAllAvailableCoachSessionsForCoachId_ReturnsFilteredAvailableSessions()
        {
            // Arrange
            const string targetCoachId = "target-coach";
            
            _sessionsCollection.InsertOne(new Session { CoachId = targetCoachId, CurrentStatus = Session.Status.Available }); // Return
            _sessionsCollection.InsertOne(new Session { CoachId = "other-coach", CurrentStatus = Session.Status.Available });  // Ignore (wrong coach)
            _sessionsCollection.InsertOne(new Session { CoachId = targetCoachId, CurrentStatus = Session.Status.Planned });    // Ignore (wrong status)
            
            // Act
            var result = _repository.GetAllAvailableCoachSessionsForCoachId(targetCoachId);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.IsTrue(result.All(s => s.CoachId == targetCoachId && s.CurrentStatus == Session.Status.Available));
        }
        
        
        // --- GetAllSessionsByCoachId Tests
        
        [TestMethod]
        public void GetAllSessionsByCoachId_ReturnsAllSessionsForSpecifiedCoach()
        {
            // Arrange
            const string targetCoachId = "coach-A";
            
            _sessionsCollection.InsertOne(new Session { CoachId = targetCoachId, CurrentStatus = Session.Status.Planned });    // Return
            _sessionsCollection.InsertOne(new Session { CoachId = "coach-B", CurrentStatus = Session.Status.Available });       // Ignore (wrong coach)
            _sessionsCollection.InsertOne(new Session { CoachId = targetCoachId, CurrentStatus = Session.Status.Completed });  // Return

            // Act
            var result = _repository.GetAllSessionsByCoachId(targetCoachId);

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.All(s => s.CoachId == targetCoachId));
        }
    }
}