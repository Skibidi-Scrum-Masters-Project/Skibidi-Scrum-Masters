using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using MongoDB.Driver;
using CoachingService.Models;
using CoachingService;
using System;

namespace CoachingService.Tests
{
    [TestClass]
    public class CoachingRepositoryTests
    {
        private Mock<IMongoCollection<Session>> _mockCollection = null!;
        private Mock<IMongoDatabase> _mockDatabase = null!;
        private CoachingRepository _repository = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockCollection = new Mock<IMongoCollection<Session>>();
            _mockDatabase = new Mock<IMongoDatabase>();

            _mockDatabase.Setup(db => db.GetCollection<Session>("Sessions", null))
                .Returns(_mockCollection.Object);

            _repository = new CoachingRepository(_mockDatabase.Object);
        }


        // BookSession Tests
        [TestMethod]
        public void BookSession_ValidSession_InsertsIntoCollection()
        {
            // Arrange
            var session = new Session
            {
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1),
                BookingForm = new BookingForm
                {
                    CreatedAt = DateTime.UtcNow,
                    Goals = "Improve stamina",
                    Notes = "Focus on breathing",
                    Experience = Experience.Øvet
                },
                CurrentStatus = Session.Status.Planned
            };

            // Act
            var result = _repository.BookSession(session);

            // Assert
            _mockCollection.Verify(c => c.InsertOne(session, null, default), Times.Once);
            Assert.AreEqual(session, result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void BookSession_NullSession_ThrowsArgumentNullException()
        {
            // Act
            _repository.BookSession(null!);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void BookSession_EndTimeBeforeStartTime_ThrowsArgumentException()
        {
            // Arrange
            var invalidSession = new Session
            {
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddMinutes(-30)
            };

            // Act
            _repository.BookSession(invalidSession);
        }

        [TestMethod]
        public void BookSession_BookingFormWithoutCreatedAt_SetsCreatedAt()
        {
            // Arrange
            var session = new Session
            {
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1),
                BookingForm = new BookingForm
                {
                    Goals = "Improve stamina",
                    Notes = "Focus on breathing",
                    Experience = Experience.Begynder,
                    CreatedAt = default // not set
                }
            };

            // Act
            var result = _repository.BookSession(session);

            // Assert
            Assert.AreNotEqual(default(DateTime), result.BookingForm.CreatedAt);
        }

        [TestMethod]
        public void BookSession_DefaultStatus_SetsPlanned()
        {
            // Arrange
            var session = new Session
            {
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1),
                BookingForm = new BookingForm
                {
                    CreatedAt = DateTime.UtcNow,
                    Goals = "Improve stamina",
                    Notes = "Focus on breathing",
                    Experience = Experience.Ekspert
                },
                CurrentStatus = default // not set
            };

            // Act
            var result = _repository.BookSession(session);

            // Assert
            Assert.AreEqual(Session.Status.Planned, result.CurrentStatus);
        }


        // GetAllSessions Tests

        [TestMethod]
        public void GetAllSessions_ReturnsListOfSessions()
        {
            // Arrange
            var sessions = new List<Session>
            {
                new Session { StartTime = DateTime.UtcNow, EndTime = DateTime.UtcNow.AddHours(1) },
                new Session { StartTime = DateTime.UtcNow.AddHours(2), EndTime = DateTime.UtcNow.AddHours(3) }
            };

            // Mock cursor to simulate MongoDB results
            var mockCursor = new Mock<IAsyncCursor<Session>>();
            mockCursor.SetupSequence(c => c.MoveNext(It.IsAny<System.Threading.CancellationToken>()))
                .Returns(true)
                .Returns(false);
            mockCursor.SetupGet(c => c.Current).Returns(sessions);

            _mockCollection.Setup(c => c.FindSync(
                    It.IsAny<FilterDefinition<Session>>(),
                    It.IsAny<FindOptions<Session, Session>>(),
                    default))
                .Returns(mockCursor.Object);

            // Act
            var result = _repository.GetAllSessions();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(2, result.Count());
        }

        [TestMethod]
        public void GetAllSessions_EmptyCollection_ReturnsEmptyList()
        {
            // Arrange
            var emptySessions = new List<Session>();

            var mockCursor = new Mock<IAsyncCursor<Session>>();
            mockCursor.SetupSequence(c => c.MoveNext(It.IsAny<System.Threading.CancellationToken>()))
                .Returns(false);
            mockCursor.SetupGet(c => c.Current).Returns(emptySessions);

            _mockCollection.Setup(c => c.FindSync(
                    It.IsAny<FilterDefinition<Session>>(),
                    It.IsAny<FindOptions<Session, Session>>(),
                    default))
                .Returns(mockCursor.Object);

            // Act
            var result = _repository.GetAllSessions();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count());
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void GetAllSessions_RepositoryThrowsException_PropagatesException()
        {
            // Arrange
            _mockCollection.Setup(c => c.FindSync(
                    It.IsAny<FilterDefinition<Session>>(),
                    It.IsAny<FindOptions<Session, Session>>(),
                    default))
                .Throws(new Exception("Database error"));

            // Act
            _repository.GetAllSessions(); // should throw }

        }
    }
}
