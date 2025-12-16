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

        // BookSession Tests 
        [TestMethod]
        public void BookSession_ValidSession_ReturnsOkWithSession()
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

            _mockRepository.Setup(r => r.BookSession(It.IsAny<Session>()))
                           .Returns(session);

            var result = _controller.BookSession(session);

            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var returnedSession = okResult.Value as Session;
            Assert.IsNotNull(returnedSession);
            Assert.AreEqual(session.BookingForm.Goals, returnedSession.BookingForm.Goals);
        }

        [TestMethod]
        public void BookSession_NullSession_ReturnsBadRequest()
        {
            _mockRepository.Setup(r => r.BookSession(null!))
                           .Throws(new ArgumentNullException());

            var result = _controller.BookSession(null!);

            var badRequestResult = result.Result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
        }

        [TestMethod]
        public void BookSession_InvalidTimes_ReturnsBadRequest()
        {
            var invalidSession = new Session
            {
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddMinutes(-30)
            };

            _mockRepository.Setup(r => r.BookSession(invalidSession))
                           .Throws(new ArgumentException("EndTime must be after StartTime"));

            var result = _controller.BookSession(invalidSession);

            var badRequestResult = result.Result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
        }

        [TestMethod]
        public void BookSession_UnexpectedException_ReturnsInternalServerError()
        {
            var session = new Session
            {
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1)
            };

            _mockRepository.Setup(r => r.BookSession(It.IsAny<Session>()))
                           .Throws(new Exception("Database error"));

            var result = _controller.BookSession(session);

            var objectResult = result.Result as ObjectResult;
            Assert.IsNotNull(objectResult);
            Assert.AreEqual(500, objectResult.StatusCode);
        }
        
        //GetAllSessions Tests
        
        
          [TestMethod]
    public void GetAllSessions_ReturnsOkWithSessions()
    {
        // Arrange
        var sessions = new List<Session>
        {
            new Session
            {
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1),
                CurrentStatus = Session.Status.Planned
            },
            new Session
            {
                StartTime = DateTime.UtcNow.AddHours(2),
                EndTime = DateTime.UtcNow.AddHours(3),
                CurrentStatus = Session.Status.Booked
            }
        };

        _mockRepository.Setup(r => r.GetAllSessions())
                       .Returns(sessions);

        // Act
        var result = _controller.GetAllSessions();

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);

        var returnedSessions = okResult.Value as IEnumerable<Session>;
        Assert.IsNotNull(returnedSessions);
        Assert.AreEqual(2, returnedSessions.Count());
    }

    [TestMethod]
    public void GetAllSessions_EmptyList_ReturnsOkWithEmptyCollection()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAllSessions())
                       .Returns(new List<Session>());

        // Act
        var result = _controller.GetAllSessions();

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);

        var returnedSessions = okResult.Value as IEnumerable<Session>;
        Assert.IsNotNull(returnedSessions);
        Assert.AreEqual(0, returnedSessions.Count());
    }

    [TestMethod]
    public void GetAllSessions_RepositoryThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        _mockRepository.Setup(r => r.GetAllSessions())
                       .Throws(new Exception("Database error"));

        // Act
        var result = _controller.GetAllSessions();

        // Assert
        var objectResult = result.Result as ObjectResult;
        Assert.IsNotNull(objectResult);
        Assert.AreEqual(500, objectResult.StatusCode);
    }
        
        //GetSessionById Tests
        
        
        
        [TestMethod]
        public void GetSessionById_ValidId_ReturnsOkWithSession()
        {
            var session = new Session
            {
                Id = "123",
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1),
                CurrentStatus = Session.Status.Planned
            };

            _mockRepository.Setup(r => r.GetSessionById("123"))
                .Returns(session);

            var result = _controller.GetSessionById("123");

            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var returnedSession = okResult.Value as Session;
            Assert.IsNotNull(returnedSession);
            Assert.AreEqual("123", returnedSession.Id);
        }

        [TestMethod]
        public void GetSessionById_InvalidId_ReturnsNotFound()
        {
            _mockRepository.Setup(r => r.GetSessionById("999"))
                .Returns((Session?)null);

            var result = _controller.GetSessionById("999");

            var notFoundResult = result.Result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [TestMethod]
        public void GetSessionById_RepositoryThrowsException_ReturnsInternalServerError()
        {
            _mockRepository.Setup(r => r.GetSessionById("123"))
                .Throws(new Exception("Database error"));

            var result = _controller.GetSessionById("123");

            var objectResult = result.Result as ObjectResult;
            Assert.IsNotNull(objectResult);
            Assert.AreEqual(500, objectResult.StatusCode);
        }
        
        
        //CancelSession Tests
        
        
        [TestMethod]
        public void CancelSession_ValidId_ReturnsOkWithCancelledSession()
        {
            var session = new Session
            {
                Id = "123",
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1),
                CurrentStatus = Session.Status.Cancelled
            };

            _mockRepository.Setup(r => r.CancelSession("123"))
                .Returns(session);

            var result = _controller.CancelSession("123");

            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var returnedSession = okResult.Value as Session;
            Assert.IsNotNull(returnedSession);
            Assert.AreEqual(Session.Status.Cancelled, returnedSession.CurrentStatus);
        }

        [TestMethod]
        public void CancelSession_InvalidId_ReturnsNotFound()
        {
            _mockRepository.Setup(r => r.CancelSession("999"))
                .Throws(new ArgumentNullException("Session not found"));

            var result = _controller.CancelSession("999");

            var notFoundResult = result.Result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [TestMethod]
        public void CancelSession_RepositoryThrowsException_ReturnsInternalServerError()
        {
            _mockRepository.Setup(r => r.CancelSession("123"))
                .Throws(new Exception("Database error"));

            var result = _controller.CancelSession("123");

            var objectResult = result.Result as ObjectResult;
            Assert.IsNotNull(objectResult);
            Assert.AreEqual(500, objectResult.StatusCode);
        }
        
    }
}

