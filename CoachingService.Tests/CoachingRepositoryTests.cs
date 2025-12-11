using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mongo2Go;
using MongoDB.Driver;
using CoachingService.Models;
using CoachingService;
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

        [TestInitialize]
        public void Setup()
        {
            _runner = MongoDbRunner.Start();

            var client = new MongoClient(_runner.ConnectionString);
            _database = client.GetDatabase("test_coachingservice_db");

            _repository = new CoachingRepository(_database);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _runner?.Dispose();
        }


        //       BookSession Tests


        [TestMethod]
        public void BookSession_ValidSession_InsertsIntoDatabase()
        {
            var session = new Session
            {
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1),
                BookingForm = new BookingForm
                {
                    CreatedAt = DateTime.UtcNow,
                    Goals = "Improve stamina",
                    Notes = "Focus on breathing",
                    Experience = Experience.Begynder
                },
                CurrentStatus = Session.Status.Planned
            };

            var result = _repository.BookSession(session);

            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Id);

            var stored = _repository.GetSessionById(result.Id!);
            Assert.IsNotNull(stored);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BookSession_NullSession_Throws()
        {
            _repository.BookSession(null!);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BookSession_EndBeforeStart_Throws()
        {
            var session = new Session
            {
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(-1)
            };

            _repository.BookSession(session);
        }

        [TestMethod]
        public void BookSession_NoCreatedAt_SetsCreatedAt()
        {
            var session = new Session
            {
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1),
                BookingForm = new BookingForm
                {
                    Goals = "Test",
                    Notes = "Test notes",
                    Experience = Experience.Begynder
                }
            };

            var result = _repository.BookSession(session);

            Assert.AreNotEqual(default(DateTime), result.BookingForm.CreatedAt);
        }


        //     GetAllSessions Tests


        [TestMethod]
        public void GetAllSessions_ReturnsSessions()
        {
            _repository.BookSession(new Session
            {
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1),
                BookingForm = new BookingForm
                {
                    CreatedAt = DateTime.UtcNow,
                    Goals = "A",
                    Notes = "B",
                    Experience = Experience.Begynder
                }
            });

            _repository.BookSession(new Session
            {
                StartTime = DateTime.UtcNow.AddHours(2),
                EndTime = DateTime.UtcNow.AddHours(3),
                BookingForm = new BookingForm
                {
                    CreatedAt = DateTime.UtcNow,
                    Goals = "C",
                    Notes = "D",
                    Experience = Experience.Ekspert
                }
            });

            var result = _repository.GetAllSessions();

            Assert.AreEqual(2, result.Count());
        }

        [TestMethod]
        public void GetAllSessions_EmptyDatabase_ReturnsEmptyList()
        {
            var result = _repository.GetAllSessions();
            Assert.AreEqual(0, result.Count());
        }


        //      GetSessionById Tests


        [TestMethod]
        public void GetSessionById_ValidId_ReturnsSession()
        {
            var session = new Session
            {
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1),
                BookingForm = new BookingForm
                {
                    CreatedAt = DateTime.UtcNow,
                    Goals = "Test",
                    Notes = "Test",
                    Experience = Experience.Begynder
                }
            };

            var created = _repository.BookSession(session);

            var fetched = _repository.GetSessionById(created.Id!);

            Assert.IsNotNull(fetched);
            Assert.AreEqual(created.Id, fetched.Id);
        }

        [TestMethod]
        public void GetSessionById_NotFound_ReturnsNull()
        {
            // Use a syntactically valid, but non-existent, ObjectId string (24-character hex)
            var result = _repository.GetSessionById("000000000000000000000000"); 
            Assert.IsNull(result);
        }


        //      CancelSession Tests

        [TestMethod]
        public void CancelSession_ValidId_StatusUpdated()
        {
            var session = new Session
            {
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1),
                BookingForm = new BookingForm
                {
                    CreatedAt = DateTime.UtcNow,
                    Goals = "Test",
                    Notes = "Test",
                    Experience = Experience.Begynder
                },
                CurrentStatus = Session.Status.Planned
            };

            var created = _repository.BookSession(session);

            var updated = _repository.CancelSession(created.Id!);

            Assert.AreEqual(Session.Status.Cancelled, updated.CurrentStatus);

            var fetched = _repository.GetSessionById(created.Id!);
            Assert.AreEqual(Session.Status.Cancelled, fetched!.CurrentStatus);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CancelSession_NotFound_Throws()
        {
            // Use a syntactically valid, but non-existent, ObjectId string (24-character hex)
            _repository.CancelSession("000000000000000000000001");
        }


        // CompleteSession

        [TestMethod]
        public void CompleteSession_ValidId_UpdatesStatusToCompleted()
        {
            // Arrange
            var session = new Session
            {
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1),
                BookingForm = new BookingForm
                {
                    CreatedAt = DateTime.UtcNow,
                    Goals = "Test",
                    Notes = "Test notes",
                    Experience = Experience.Begynder
                },
                CurrentStatus = Session.Status.Planned
            };

            var created = _repository.BookSession(session);

            // Act
            var completed = _repository.CompleteSession(created.Id!);

            // Assert
            Assert.IsNotNull(completed);
            Assert.AreEqual(Session.Status.Completed, completed.CurrentStatus);

            // Also verify it was updated in the database
            var fetched = _repository.GetSessionById(created.Id!);
            Assert.IsNotNull(fetched);
            Assert.AreEqual(Session.Status.Completed, fetched.CurrentStatus);
        }
    }
}

