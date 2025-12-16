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
        private Mock<IAnalyticsRepository> _mockRepository;
        private AnalyticsController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockRepository = new Mock<IAnalyticsRepository>();
            _controller = new AnalyticsController(_mockRepository.Object);
        }

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
                .Setup(r => r.PostClassesAnalytics(
                    dto.ClassId,
                    dto.UserId,
                    dto.CaloriesBurned,
                    dto.Watt,
                    dto.Category,
                    dto.DurationMin,
                    dto.Date))
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
