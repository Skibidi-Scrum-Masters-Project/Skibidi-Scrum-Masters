using Moq;
using MongoDB.Driver;
using AnalyticsService.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;

namespace AnalyticsService.Tests
{
    [TestClass]
    public class AnalyticsRepositoryTests
    {
        private Mock<IMongoDatabase> _mockDatabase = null!;
        private Mock<IMongoCollection<ClassResultDTO>> _mockClassCollection = null!;
        private Mock<IMongoCollection<CrowdResultDTO>> _mockCrowdCollection = null!;
        private Mock<IMongoCollection<SoloTrainingResultsDTO>> _mockSoloCollection = null!;
        private AnalyticsRepository _repository = null!;

        [TestInitialize]
        public void Init()
        {
            _mockDatabase = new Mock<IMongoDatabase>();
            _mockClassCollection = new Mock<IMongoCollection<ClassResultDTO>>();
            _mockCrowdCollection = new Mock<IMongoCollection<CrowdResultDTO>>();
            _mockSoloCollection = new Mock<IMongoCollection<SoloTrainingResultsDTO>>();

            _mockDatabase.Setup(db => db.GetCollection<ClassResultDTO>("ClassResults", null))
                .Returns(_mockClassCollection.Object);
            _mockDatabase.Setup(db => db.GetCollection<CrowdResultDTO>("CrowdResults", null))
                .Returns(_mockCrowdCollection.Object);
            _mockDatabase.Setup(db => db.GetCollection<SoloTrainingResultsDTO>("SoloTrainingResults", null))
                .Returns(_mockSoloCollection.Object);

            _mockClassCollection
                .Setup(c => c.InsertOneAsync(It.IsAny<ClassResultDTO>(), null, default))
                .Returns(Task.CompletedTask);

            _mockCrowdCollection
                .Setup(c => c.InsertOneAsync(It.IsAny<CrowdResultDTO>(), null, default))
                .Returns(Task.CompletedTask);

            _mockCrowdCollection
                .Setup(c => c.FindOneAndUpdateAsync(
                    It.IsAny<FilterDefinition<CrowdResultDTO>>(),
                    It.IsAny<UpdateDefinition<CrowdResultDTO>>(),
                    It.IsAny<FindOneAndUpdateOptions<CrowdResultDTO>>(),
                    default))
                .ReturnsAsync((CrowdResultDTO?)null);

            _mockCrowdCollection
                .Setup(c => c.CountDocumentsAsync(It.IsAny<FilterDefinition<CrowdResultDTO>>(), null, default))
                .ReturnsAsync(0);

            _repository = new AnalyticsRepository(_mockDatabase.Object, new HttpClient());
        }

        // -----------------------------
        // Class analytics
        // -----------------------------

        [TestMethod]
        public async Task PostClassesAnalytics_ShouldInsertAndReturnClassResult()
        {
            var result = await _repository.PostClassesAnalytics(
                "class123",
                "user123",
                500,
                200,
                Category.Yoga,
                60,
                DateTime.UtcNow);

            Assert.IsNotNull(result);
            Assert.AreEqual("class123", result.ClassId);

            _mockClassCollection.Verify(
                c => c.InsertOneAsync(It.IsAny<ClassResultDTO>(), null, default),
                Times.Once);
        }

        [TestMethod]
        public async Task PostClassesAnalytics_ShouldThrow_OnEmptyClassId()
        {
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
                _repository.PostClassesAnalytics(
                    "",
                    "user",
                    100,
                    50,
                    Category.Yoga,
                    30,
                    DateTime.UtcNow));
        }

        [TestMethod]
        public async Task PostClassesAnalytics_ShouldThrow_OnNegativeDuration()
        {
            await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(() =>
                _repository.PostClassesAnalytics(
                    "c1",
                    "u1",
                    100,
                    50,
                    Category.Yoga,
                    -1,
                    DateTime.UtcNow));
        }

        // -----------------------------
        // Crowd
        // -----------------------------

        [TestMethod]
        public async Task PostEnteredUser_ShouldInsertCrowdResult()
        {
            var result = await _repository.PostEnteredUser(
                "user123",
                DateTime.UtcNow,
                DateTime.MinValue);

            StringAssert.Contains(result, "User entered");
        }

        [TestMethod]
        public async Task UpdateUserExitTime_ShouldThrow_WhenNoMatch()
        {
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() =>
                _repository.UpdateUserExitTime("unknown", DateTime.UtcNow));
        }

        [TestMethod]
        public async Task UpdateUserExitTime_ShouldReturnId_WhenUpdated()
        {
            var updated = new CrowdResultDTO
            {
                Id = "id-1",
                UserId = "user123",
                ExitTime = DateTime.UtcNow
            };

            _mockCrowdCollection
                .Setup(c => c.FindOneAndUpdateAsync(
                    It.IsAny<FilterDefinition<CrowdResultDTO>>(),
                    It.IsAny<UpdateDefinition<CrowdResultDTO>>(),
                    It.IsAny<FindOneAndUpdateOptions<CrowdResultDTO>>(),
                    default))
                .ReturnsAsync(updated);

            var result = await _repository.UpdateUserExitTime("user123", updated.ExitTime);

            Assert.AreEqual("id-1", result);
        }

        // -----------------------------
        // Solo training
        // -----------------------------

        [TestMethod]
        public async Task PostSoloTrainingResult_ShouldThrow_OnMissingUserId()
        {
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
                _repository.PostSoloTrainingResult(
                    null!,
                    DateTime.UtcNow,
                    new List<Exercise>(),
                    TrainingTypes.UpperBody,
                    30));
        }

        [TestMethod]
        public async Task PostSoloTrainingResult_ShouldThrow_OnNullExercises()
        {
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
                _repository.PostSoloTrainingResult(
                    "u1",
                    DateTime.UtcNow,
                    null!,
                    TrainingTypes.UpperBody,
                    30));
        }

        [TestMethod]
        public async Task PostSoloTrainingResult_ShouldThrow_OnNegativeDuration()
        {
            await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(() =>
                _repository.PostSoloTrainingResult(
                    "u1",
                    DateTime.UtcNow,
                    new List<Exercise>(),
                    TrainingTypes.UpperBody,
                    -5));
        }

        [TestMethod]
        public async Task GetCrowdCount_WhenNoEntries_ReturnsZero()
        {
            var count = await _repository.GetCrowdCount();
            Assert.AreEqual(0, count);
        }
    }
}
