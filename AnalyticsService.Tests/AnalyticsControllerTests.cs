using Moq;
using Microsoft.AspNetCore.Mvc;
using AnalyticsService.Controllers;
using AnalyticsService.Models;
using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AnalyticsService.Tests
{
    [TestClass]
    public class AnalyticsControllerTests
    {
        private readonly Mock<IAnalyticsRepository> _mockRepository;
        private readonly AnalyticsController _controller;

        public AnalyticsControllerTests()
        {
            _mockRepository = new Mock<IAnalyticsRepository>();
            _controller = new AnalyticsController(_mockRepository.Object);
        }

        [TestMethod]
        public async Task GetClassesAnalytics_ShouldReturnOkWithClassResult()
        {
            // Arrange
            var classId = "class123";
            var userId = "user123";
            var calories = 500.0;
            var category = "Yoga";
            var duration = 60;
            var date = DateTime.UtcNow;

            var expectedResult = new ClassResultDTO
            {
                ClassId = classId,
                UserId = userId,
                TotalCaloriesBurned = calories,
                Category = category,
                DurationMin = duration,
                Date = date
            };

            _mockRepository
                .Setup(r => r.PostClassesAnalytics(classId, userId, calories, category, duration, date))
                .ReturnsAsync(expectedResult);

            // Act
            var actionResult = await _controller.PostClassesAnalytics(classId, userId, calories, category, duration, date);

            // Assert
            var okResult = actionResult as OkObjectResult;
            Assert.IsNotNull(okResult);
            var returnValue = okResult.Value as ClassResultDTO;
            Assert.IsNotNull(returnValue);
            Assert.AreEqual(classId, returnValue.ClassId);
            Assert.AreEqual(userId, returnValue.UserId);
        }

        [TestMethod]
        public async Task PostEnteredUser_ShouldReturnOkWithSuccessMessage()
        {
            // Arrange
            var userId = "user123";
            var entryTime = DateTime.UtcNow;
            var expected = "User entered crowd data posted successfully";

            _mockRepository
                .Setup(r => r.PostEnteredUser(userId, entryTime, DateTime.MinValue))
                .ReturnsAsync(expected);

            // Act
            var actionResult = await _controller.AddUserToCrowd(userId, entryTime);

            // Assert
            var okResult = actionResult as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(expected, okResult.Value);
        }

        [TestMethod]
        public async Task UpdateUserExitTime_ShouldReturnOkWithSuccessMessage()
        {
            // Arrange
            var userId = "user123";
            var exitTime = DateTime.UtcNow;
            var expected = "User exit time updated successfully";

            _mockRepository
                .Setup(r => r.UpdateUserExitTime(userId, exitTime))
                .ReturnsAsync(expected);

            // Act
            var actionResult = await _controller.UpdateUserExitTime(userId, exitTime);

            // Assert
            var okResult = actionResult as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(expected, okResult.Value);
        }
    }
}
