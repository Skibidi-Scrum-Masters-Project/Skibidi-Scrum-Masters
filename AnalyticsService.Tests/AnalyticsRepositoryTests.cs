using Moq;
using MongoDB.Driver;
using AnalyticsService.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AnalyticsService.Tests
{
    [TestClass]
    public class AnalyticsRepositoryTests
    {
        private readonly Mock<IMongoDatabase> _mockDatabase;
        private readonly Mock<IMongoCollection<ClassResultDTO>> _mockClassCollection;
        private readonly Mock<IMongoCollection<CrowdResultDTO>> _mockCrowdCollection;
        private readonly AnalyticsRepository _repository;

        public AnalyticsRepositoryTests()
        {
            _mockDatabase = new Mock<IMongoDatabase>();
            _mockClassCollection = new Mock<IMongoCollection<ClassResultDTO>>();
            _mockCrowdCollection = new Mock<IMongoCollection<CrowdResultDTO>>();

            _mockDatabase.Setup(db => db.GetCollection<ClassResultDTO>("ClassResults", null))
                .Returns(_mockClassCollection.Object);
            _mockDatabase.Setup(db => db.GetCollection<CrowdResultDTO>("CrowdResults", null))
                .Returns(_mockCrowdCollection.Object);

            _repository = new AnalyticsRepository(_mockDatabase.Object);
        }

        [TestMethod]
        public async Task GetClassesAnalytics_ShouldInsertAndReturnClassResult()
        {
            // Arrange
            var classId = "class123";
            var userId = "user123";
            var calories = 500.0;
            var category = "Yoga";
            var duration = 60;
            var date = DateTime.Now;

            // Act
            var result = await _repository.GetClassesAnalytics(classId, userId, calories, category, duration, date);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(classId, result.ClassId);
            Assert.AreEqual(userId, result.UserId);
            Assert.AreEqual(calories, result.TotalCaloriesBurned);
            Assert.AreEqual(category, result.Category);
            Assert.AreEqual(duration, result.DurationMin);
            Assert.AreEqual(date, result.Date);

            _mockClassCollection.Verify(c => c.InsertOne(
                It.IsAny<ClassResultDTO>(),
                null,
                default(CancellationToken)), Times.Once);
        }

        [TestMethod]
        public async Task PostEnteredUser_ShouldInsertCrowdResult()
        {
            // Arrange
            var userId = "user123";
            var entryTime = DateTime.Now;
            var exitTime = DateTime.MinValue;

            // Act
            var result = await _repository.PostEnteredUser(userId, entryTime, exitTime);

            // Assert
            Assert.AreEqual("User entered crowd data posted successfully", result);

            _mockCrowdCollection.Verify(c => c.InsertOne(
                It.Is<CrowdResultDTO>(cr => 
                    cr.UserId == userId && 
                    cr.EntryTime == entryTime && 
                    cr.ExitTime == exitTime),
                null,
                default(CancellationToken)), Times.Once);
        }

        [TestMethod]
        public async Task UpdateUserExitTime_ShouldUpdateExitTime()
        {
            // Arrange
            var userId = "user123";
            var exitTime = DateTime.Now;
            var mockCursor = new Mock<IAsyncCursor<CrowdResultDTO>>();

            _mockCrowdCollection.Setup(c => c.FindOneAndUpdate(
                It.IsAny<FilterDefinition<CrowdResultDTO>>(),
                It.IsAny<UpdateDefinition<CrowdResultDTO>>(),
                It.IsAny<FindOneAndUpdateOptions<CrowdResultDTO>>(),
                default(CancellationToken)))
                .Returns(new CrowdResultDTO { UserId = userId, ExitTime = exitTime });

            // Act
            var result = await _repository.UpdateUserExitTime(userId, exitTime);

            // Assert
            Assert.AreEqual("User exit time updated successfully", result);

            _mockCrowdCollection.Verify(c => c.FindOneAndUpdate(
                It.IsAny<FilterDefinition<CrowdResultDTO>>(),
                It.IsAny<UpdateDefinition<CrowdResultDTO>>(),
                It.IsAny<FindOneAndUpdateOptions<CrowdResultDTO>>(),
                default(CancellationToken)), Times.Once);
        }
    }
}