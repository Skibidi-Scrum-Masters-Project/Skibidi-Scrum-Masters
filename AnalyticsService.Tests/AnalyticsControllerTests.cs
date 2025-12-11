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
            var date = DateTime.Now;

            var expectedResult = new ClassResultDTO
            {
                ClassId = classId,
                UserId = userId,
                TotalCaloriesBurned = calories,
                Category = category,
                DurationMin = duration,
                Date = date
            };

            _mockRepository.Setup(r => r.GetClassesAnalytics(classId, userId, calories, category, duration, date))
                .ReturnsAsync(expectedResult);

            // Act
            var result = await _controller.GetClassesAnalytics(classId, userId, calories, category, duration, date);

            // Assert
            var okResult = result as OkObjectResult;
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
            var entryTime = DateTime.Now;

            _mockRepository.Setup(r => r.PostEnteredUser(userId, entryTime, DateTime.MinValue))
                .ReturnsAsync("User entered crowd data posted successfully");

            // Act
            var result = await _controller.AddUserToCrowd(userId, entryTime);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual("User entered crowd data posted successfully", okResult.Value);
        }

        [TestMethod]
        public async Task UpdateUserExitTime_ShouldReturnOkWithSuccessMessage()
        {
            // Arrange
            var userId = "user123";
            var exitTime = DateTime.Now;

            _mockRepository.Setup(r => r.UpdateUserExitTime(userId, exitTime))
                .ReturnsAsync("User exit time updated successfully");

            // Act
            var result = await _controller.UpdateUserExitTime(userId, exitTime);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual("User exit time updated successfully", okResult.Value);
        }
    }
}