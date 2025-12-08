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

        [TestMethod]
        public void CreateSession_ValidSession_InsertsIntoCollection()
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
            var result = _repository.CreateSession(session);

            // Assert
            _mockCollection.Verify(c => c.InsertOne(session, null, default), Times.Once);
            Assert.AreEqual(session, result);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CreateSession_NullSession_ThrowsArgumentNullException()
        {
            // Act
            _repository.CreateSession(null!);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateSession_EndTimeBeforeStartTime_ThrowsArgumentException()
        {
            // Arrange
            var invalidSession = new Session
            {
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddMinutes(-30)
            };

            // Act
            _repository.CreateSession(invalidSession);
        }

        [TestMethod]
        public void CreateSession_BookingFormWithoutCreatedAt_SetsCreatedAt()
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
            var result = _repository.CreateSession(session);

            // Assert
            Assert.AreNotEqual(default(DateTime), result.BookingForm.CreatedAt);
        }

        [TestMethod]
        public void CreateSession_DefaultStatus_SetsPlanned()
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
            var result = _repository.CreateSession(session);

            // Assert
            Assert.AreEqual(Session.Status.Planned, result.CurrentStatus);
        }
    }
}
