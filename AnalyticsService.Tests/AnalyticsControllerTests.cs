using Moq;
using Microsoft.AspNetCore.Mvc;
using AnalyticsService.Controllers;
using AnalyticsService.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AnalyticsService.Tests
{
    [TestClass]
    public class AnalyticsControllerTests
    {
        private Mock<IAnalyticsRepository> _mockRepository = null!;
        private AnalyticsController _controller = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockRepository = new Mock<IAnalyticsRepository>();
            _controller = new AnalyticsController(_mockRepository.Object);
        }

        // ---------- POST /classes ----------

        [TestMethod]
        public async Task PostClassesAnalytics_ShouldReturnOk_WithClassResult()
        {
            // Arrange
            var dto = new ClassResultDTO
            {
                ClassId = "class123",
                UserId = "user123",
                CaloriesBurned = 500,
                Watt = 200,
                Category = Category.Yoga,
                DurationMin = 60,
                Date = DateTime.UtcNow
            };

            _mockRepository
                .Setup(r => r.PostClassesAnalytics(It.IsAny<ClassResultDTO>()))
                .ReturnsAsync(dto);

            // Act
            var result = await _controller.PostClassesAnalytics(dto);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var returnValue = okResult.Value as ClassResultDTO;
            Assert.IsNotNull(returnValue);

            Assert.AreEqual(dto.ClassId, returnValue.ClassId);
            Assert.AreEqual(dto.UserId, returnValue.UserId);
            Assert.AreEqual(dto.Category, returnValue.Category);

            _mockRepository.Verify(
                r => r.PostClassesAnalytics(It.IsAny<ClassResultDTO>()),
                Times.Once);
        }

        // ---------- POST /entered ----------

        [TestMethod]
        public async Task AddUserToCrowd_ShouldReturnOk_WithMessage()
        {
            // Arrange
            var userId = "user123";
            var entryTime = DateTime.UtcNow;
            var expected = "User entered crowd data posted successfully";

            _mockRepository
                .Setup(r => r.PostEnteredUser(userId, entryTime, DateTime.MinValue))
                .ReturnsAsync(expected);

            // Act
            var result = await _controller.AddUserToCrowd(userId, entryTime);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(expected, okResult.Value);
        }

        // ---------- PUT /exited ----------

        [TestMethod]
        public async Task UpdateUserExitTime_ShouldReturnOk_WithMessage()
        {
            // Arrange
            var userId = "user123";
            var exitTime = DateTime.UtcNow;
            var expected = "User exit time updated successfully";

            _mockRepository
                .Setup(r => r.UpdateUserExitTime(userId, exitTime))
                .ReturnsAsync(expected);

            // Act
            var result = await _controller.UpdateUserExitTime(userId, exitTime);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(expected, okResult.Value);
        }

        // ---------- POST /solotraining ----------

        [TestMethod]
        public async Task PostSoloTrainingResult_ShouldReturnOk_WithId()
        {
            // Arrange
            var dto = new SoloTrainingResultsDTO
            {
                UserId = "user123",
                Date = DateTime.UtcNow,
                TrainingType = TrainingTypes.Cardio,
                DurationMinutes = 45,
                Exercises = new List<Exercise>()
            };

            var expected = "solo-id-123";

            _mockRepository
                .Setup(r => r.PostSoloTrainingResult(It.IsAny<SoloTrainingResultsDTO>()))
                .ReturnsAsync(expected);

            // Act
            var result = await _controller.PostSoloTrainingResult(dto);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(expected, okResult.Value);
        }

        // ---------- GET /crowd ----------

        [TestMethod]
        public async Task GetCrowdCount_ShouldReturnOk_WithCount()
        {
            // Arrange
            var expected = 5;

            _mockRepository
                .Setup(r => r.GetCrowdCount())
                .ReturnsAsync(expected);

            // Act
            var result = await _controller.GetCrowdCount();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(expected, okResult.Value);
        }

        // ---------- GET /solotrainingresult/{userId} ----------

        [TestMethod]
        public async Task GetSoloTrainingResult_ShouldReturnOk_WithResults()
        {
            // Arrange
            var userId = "user123";
            var expected = new List<SoloTrainingResultsDTO>();

            _mockRepository
                .Setup(r => r.GetSoloTrainingResult(userId))
                .ReturnsAsync(expected);

            // Act
            var result = await _controller.GetSoloTrainingResult(userId);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(expected, okResult.Value);
        }

        // ---------- GET /classresult/{userId} ----------

        [TestMethod]
        public async Task GetClassResult_ShouldReturnOk_WithResults()
        {
            // Arrange
            var userId = "user123";
            var expected = new List<ClassResultDTO>();

            _mockRepository
                .Setup(r => r.GetClassResult(userId))
                .ReturnsAsync(expected);

            // Act
            var result = await _controller.GetClassResult(userId);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(expected, okResult.Value);
        }

        // ---------- GET /dashboard/{userId} ----------

        [TestMethod]
        public async Task GetDashboard_ShouldReturnOk_WithDashboard()
        {
            // Arrange
            var userId = "user123";
            var dashboard = new AnalyticsDashboardDTO { UserId = userId };

            _mockRepository
                .Setup(r => r.GetDashboardResult(userId))
                .ReturnsAsync(dashboard);

            // Act
            var result = await _controller.GetDashboard(userId);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(dashboard, okResult.Value);
        }

        // ---------- GET /compare/month/{userId} ----------

        [TestMethod]
        public async Task GetCompareForMonth_ShouldReturnOk_WithCompareDto()
        {
            // Arrange
            var userId = "user123";
            var compareDto = new AnalyticsCompareDTO { UserId = userId };

            _mockRepository
                .Setup(r => r.GetCompareResultForCurrentMonth(userId))
                .ReturnsAsync(compareDto);

            // Act
            var result = await _controller.GetCompareForMonth(userId);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(compareDto, okResult.Value);
        }
    }
}
