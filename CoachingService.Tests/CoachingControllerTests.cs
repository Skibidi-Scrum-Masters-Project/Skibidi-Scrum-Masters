using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using CoachingService.Controllers;
using CoachingService.Models;

namespace CoachingService.Tests
{
    [TestClass]
    public class CoachingControllerTests
    {
        private CoachingController _controller = null!;
        private Mock<ICoachingRepository> _mockRepository = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockRepository = new Mock<ICoachingRepository>();
            _controller = new CoachingController(_mockRepository.Object);
        }

        [TestMethod]
        public void CreateSession_ValidSession_ReturnsOkWithSession()
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
                    Experience = Experience.Øvet
                },
                CurrentStatus = Session.Status.Planned
            };

            _mockRepository.Setup(r => r.CreateSession(It.IsAny<Session>()))
                           .Returns(session);

            var result = _controller.CreateSession(session);

            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var returnedSession = okResult.Value as Session;
            Assert.IsNotNull(returnedSession);
            Assert.AreEqual(session.BookingForm.Goals, returnedSession.BookingForm.Goals);
        }

        [TestMethod]
        public void CreateSession_NullSession_ReturnsBadRequest()
        {
            _mockRepository.Setup(r => r.CreateSession(null!))
                           .Throws(new ArgumentNullException());

            var result = _controller.CreateSession(null!);

            var badRequestResult = result.Result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
        }

        [TestMethod]
        public void CreateSession_InvalidTimes_ReturnsBadRequest()
        {
            var invalidSession = new Session
            {
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddMinutes(-30)
            };

            _mockRepository.Setup(r => r.CreateSession(invalidSession))
                           .Throws(new ArgumentException("EndTime must be after StartTime"));

            var result = _controller.CreateSession(invalidSession);

            var badRequestResult = result.Result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
        }

        [TestMethod]
        public void CreateSession_UnexpectedException_ReturnsInternalServerError()
        {
            var session = new Session
            {
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1)
            };

            _mockRepository.Setup(r => r.CreateSession(It.IsAny<Session>()))
                           .Throws(new Exception("Database error"));

            var result = _controller.CreateSession(session);

            var objectResult = result.Result as ObjectResult;
            Assert.IsNotNull(objectResult);
            Assert.AreEqual(500, objectResult.StatusCode);
        }
    }
}

