using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using CoachingService.Controllers;
using CoachingService.Models;
using System;
using System.Collections.Generic;
using System.Linq;

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

        // --- BookSession Tests --- 
        
        [TestMethod]
        public void BookSession_ValidSession_ReturnsOkWithSession()
        {
            // Arrange
            var session = new Session
            {
                Id = "existing-id",
                UserId = "user-id",
                CurrentStatus = Session.Status.Planned
            };

            _mockRepository.Setup(r => r.BookSession(It.IsAny<Session>()))
                           .Returns(session);

            // Act
            var result = _controller.BookSession(session);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var returnedSession = okResult.Value as Session;
            Assert.IsNotNull(returnedSession);
            Assert.AreEqual("user-id", returnedSession.UserId);
        }

        [TestMethod]
        public void BookSession_NullSession_ReturnsBadRequest()
        {
            // Arrange
            
            // Act
            var result = _controller.BookSession(null!);

            // Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
        }

        [TestMethod]
        public void BookSession_ArgumentExceptionFromRepo_ReturnsBadRequest()
        {
            // Arrange
            var session = new Session { Id = "test-id" };
            
            _mockRepository.Setup(r => r.BookSession(It.IsAny<Session>()))
                           .Throws(new ArgumentException("Invalid arguments provided."));

            // Act
            var result = _controller.BookSession(session);

            // Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.AreEqual(400, badRequestResult.StatusCode);
        }

        [TestMethod]
        public void BookSession_UnexpectedException_ReturnsInternalServerError()
        {
            // Arrange
            var session = new Session { Id = "test-id" };

            _mockRepository.Setup(r => r.BookSession(It.IsAny<Session>()))
                           .Throws(new Exception("Database error"));

            // Act
            var result = _controller.BookSession(session);

            // Assert
            var objectResult = result.Result as ObjectResult;
            Assert.IsNotNull(objectResult);
            Assert.AreEqual(500, objectResult.StatusCode);
        }
        
        // --- GetAllSessions Tests ---
        
        [TestMethod]
        public void GetAllSessions_ReturnsOkWithSessions()
        {
            // Arrange
            var sessions = new List<Session>
            {
                new Session { CurrentStatus = Session.Status.Planned },
            };

            _mockRepository.Setup(r => r.GetAllSessions()).Returns(sessions);

            // Act
            var result = _controller.GetAllSessions();

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var returnedSessions = okResult.Value as IEnumerable<Session>;
            Assert.IsNotNull(returnedSessions);
            Assert.AreEqual(1, returnedSessions.Count());
        }

        [TestMethod]
        public void GetAllSessions_RepositoryThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetAllSessions()).Throws(new Exception("Database error"));

            // Act
            var result = _controller.GetAllSessions();

            // Assert
            var objectResult = result.Result as ObjectResult;
            Assert.IsNotNull(objectResult);
            Assert.AreEqual(500, objectResult.StatusCode);
        }
        
        // --- GetSessionById Tests ---
        
        [TestMethod]
        public void GetSessionById_ValidId_ReturnsOkWithSession()
        {
            // Arrange
            var session = new Session { Id = "123", CurrentStatus = Session.Status.Planned };

            _mockRepository.Setup(r => r.GetSessionById("123")).Returns(session);

            // Act
            var result = _controller.GetSessionById("123");

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
        }

        [TestMethod]
        public void GetSessionById_InvalidId_ReturnsNotFound()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetSessionById("999")).Returns((Session?)null);

            // Act
            var result = _controller.GetSessionById("999");

            // Assert
            var notFoundResult = result.Result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [TestMethod]
        public void GetSessionById_RepositoryThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            _mockRepository.Setup(r => r.GetSessionById("123")).Throws(new Exception("Database error"));

            // Act
            var result = _controller.GetSessionById("123");

            // Assert
            var objectResult = result.Result as ObjectResult;
            Assert.IsNotNull(objectResult);
            Assert.AreEqual(500, objectResult.StatusCode);
        }
        
        
        // --- CancelSession Tests ---
        
        
        [TestMethod]
        public void CancelSession_ValidId_ReturnsOkWithCancelledSession()
        {
            // Arrange
            var session = new Session { Id = "123", CurrentStatus = Session.Status.Cancelled };

            _mockRepository.Setup(r => r.CancelSession("123")).Returns(session);

            // Act
            var result = _controller.CancelSession("123");

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
        }

        [TestMethod]
        public void CancelSession_InvalidId_ReturnsNotFound()
        {
            // Arrange
            _mockRepository.Setup(r => r.CancelSession("999"))
                .Throws(new ArgumentNullException(paramName: "id", message: "Session not found")); 

            // Act
            var result = _controller.CancelSession("999");

            // Assert
            var notFoundResult = result.Result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        [TestMethod]
        public void CancelSession_RepositoryThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            _mockRepository.Setup(r => r.CancelSession("123"))
                .Throws(new Exception("Database error"));

            // Act
            var result = _controller.CancelSession("123");

            // Assert
            var objectResult = result.Result as ObjectResult;
            Assert.IsNotNull(objectResult);
            Assert.AreEqual(500, objectResult.StatusCode);
        }
        
        
        // --- CompleteSession Tests ---
        
        [TestMethod]
        public void CompleteSession_ValidId_ReturnsOkWithCompletedSession()
        {
            // Arrange
            var completedSession = new Session { Id = "456", CurrentStatus = Session.Status.Completed }; 
            
            _mockRepository.Setup(r => r.CompleteSession("456")).Returns(completedSession);

            // Act
            var result = _controller.CompleteSession("456");

            // Assert
            var okResult = result.Result as OkObjectResult; 
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode); 
        }

        [TestMethod]
        public void CompleteSession_InvalidId_ReturnsNotFound()
        {
            // Arrange
            _mockRepository.Setup(r => r.CompleteSession("999"))
                .Throws(new ArgumentNullException(paramName: "id", message: "Session not found")); 

            // Act
            var result = _controller.CompleteSession("999");

            // Assert
            var notFoundResult = result.Result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }
        
        // --- CreateSession Tests ---
        
        [TestMethod]
        public void CreateSession_ValidSession_ReturnsOkWithCreatedSession()
        {
            // Arrange
            var createdSession = new Session { Id = "new-id", CurrentStatus = Session.Status.Available };
            
            _mockRepository.Setup(r => r.CreateSession(It.IsAny<Session>())).Returns(createdSession);

            // Act
            var result = _controller.CreateSession(createdSession);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
        }
        
        // --- DeleteSession Tests (FIXED LINES 317 and 332) ---

        [TestMethod]
        public void DeleteSession_ValidId_ReturnsOkWithDeletedSession()
        {
            // Arrange
            var deletedSession = new Session { Id = "delete-id" };

            _mockRepository.Setup(r => r.DeleteSession("delete-id")).Returns(deletedSession);

            // Act
            var result = _controller.DeleteSession("delete-id");

            // Assert
            // FIX for line 317: Use .Result
            var okResult = result.Result as OkObjectResult; 
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
        }
        
        [TestMethod]
        public void DeleteSession_NotFound_ReturnsOkWithNull()
        {
            // Arrange
            _mockRepository.Setup(r => r.DeleteSession("non-existent-id")).Returns((Session?)null);

            // Act
            var result = _controller.DeleteSession("non-existent-id");

            // Assert
            // FIX for line 332: Use .Result
            var okResult = result.Result as OkObjectResult; 
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.IsNull(okResult.Value);
        }


        // --- GetAvailableSessions Tests ---

        [TestMethod]
        public void GetAvailableSessions_ReturnsOkWithAvailableSessions()
        {
            // Arrange
            var availableSessions = new List<Session>
            {
                new Session { Id = "a1", CurrentStatus = Session.Status.Available }
            };

            _mockRepository.Setup(r => r.GetAllAvaliableCoachSessions()).Returns(availableSessions);

            // Act
            var result = _controller.GetAvailableSessions();

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
        }


        // --- GetAvailableSessionsForCoachId Tests ---

        [TestMethod]
        public void GetAvailableSessionsForCoachId_ValidId_ReturnsOkWithFilteredSessions()
        {
            // Arrange
            const string coachId = "coach-1";
            var filteredSessions = new List<Session>
            {
                new Session { CoachId = coachId, CurrentStatus = Session.Status.Available }
            };

            _mockRepository.Setup(r => r.GetAllAvailableCoachSessionsForCoachId(coachId))
                           .Returns(filteredSessions);

            // Act
            var result = _controller.GetAvailableSessionsForCoachId(coachId);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
        }


        // --- GetAllSessionsByCoachId Tests ---
        
        [TestMethod]
        public void GetAllSessionsByCoachId_ValidId_ReturnsOkWithAllSessionsForCoach()
        {
            // Arrange
            const string coachId = "coach-2";
            var allCoachSessions = new List<Session>
            {
                new Session { CoachId = coachId, CurrentStatus = Session.Status.Planned }
            };

            _mockRepository.Setup(r => r.GetAllSessionsByCoachId(coachId)).Returns(allCoachSessions);

            // Act
            var result = _controller.GetAllSessionsByCoachId(coachId);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
        }
    }
}