using Moq;
using MongoDB.Driver;
using AnalyticsService.Models;
using System;
using System.Collections.Generic;
using System.Threading;
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

            _mockDatabase
                .Setup(db => db.GetCollection<ClassResultDTO>("ClassResults", null))
                .Returns(_mockClassCollection.Object);
            _mockDatabase
                .Setup(db => db.GetCollection<CrowdResultDTO>("CrowdResults", null))
                .Returns(_mockCrowdCollection.Object);
            _mockDatabase
                .Setup(db => db.GetCollection<SoloTrainingResultsDTO>("SoloTrainingResults", null))
                .Returns(_mockSoloCollection.Object);

            // default async setups
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
            _mockClassCollection
                .Setup(c => c.CountDocumentsAsync(It.IsAny<FilterDefinition<ClassResultDTO>>(), null, default))
                .ReturnsAsync(0);

            var httpClient = new HttpClient();
            _repository = new AnalyticsRepository(_mockDatabase.Object, httpClient);
        }

        // -----------------------------
        // Happy-path tests (minimal)
        // -----------------------------
        [TestMethod]
        public async Task PostClassesAnalytics_ShouldInsertAndReturnClassResult()
        {
            var classId = "class123";
            var userId = "user123";
            var calories = 500.0;
            var category = "Yoga";
            var duration = 60;
            var date = DateTime.UtcNow;

            var result = await _repository.PostClassesAnalytics(classId, userId, calories, category, duration, date);

            Assert.IsNotNull(result);
            Assert.AreEqual(classId, result.ClassId);
            _mockClassCollection.Verify(c => c.InsertOneAsync(It.IsAny<ClassResultDTO>(), null, default), Times.Once);
        }

        [TestMethod]
        public async Task PostEnteredUser_ShouldInsertCrowdResult()
        {
            var userId = "user123";
            var entryTime = DateTime.UtcNow;
            var exitTime = DateTime.MinValue;

            var result = await _repository.PostEnteredUser(userId, entryTime, exitTime);

            StringAssert.Contains(result, "User entered");
            _mockCrowdCollection.Verify(c => c.InsertOneAsync(It.IsAny<CrowdResultDTO>(), null, default), Times.Once);
        }

        [TestMethod]
        public async Task UpdateUserExitTime_ShouldThrowWhenNoMatch()
        {
            // default setup returns null for FindOneAndUpdateAsync -> repository should throw InvalidOperationException
            await Assert.ThrowsExceptionAsync<InvalidOperationException>(
                async () => await _repository.UpdateUserExitTime("nonexistent", DateTime.UtcNow)
            );
        }

        // -----------------------------
        // Edge-case tests
        // -----------------------------

        [TestMethod]
        public async Task PostClassesAnalytics_ShouldThrow_OnEmptyClassId()
        {
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                await _repository.PostClassesAnalytics("", "user1", 100, "cat", 30, DateTime.UtcNow));
        }

        [TestMethod]
        public async Task PostClassesAnalytics_ShouldThrow_OnEmptyUserId()
        {
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                await _repository.PostClassesAnalytics("c1", "", 100, "cat", 30, DateTime.UtcNow));
        }

        [TestMethod]
        public async Task PostClassesAnalytics_ShouldThrow_OnNegativeDuration()
        {
            await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(async () =>
                await _repository.PostClassesAnalytics("c1", "u1", 100, "cat", -1, DateTime.UtcNow));
        }

        [TestMethod]
        public async Task PostEnteredUser_ShouldThrow_OnMissingUserId()
        {
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                await _repository.PostEnteredUser(null!, DateTime.UtcNow, DateTime.MinValue));
        }

        // Change calls that passed string to pass enum:
        [TestMethod]
        public async Task PostSoloTrainingResult_ShouldThrow_OnMissingUserId()
        {
            var exercises = new List<Exercise>();
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                await _repository.PostSoloTrainingResult(null!, DateTime.UtcNow, exercises, TrainingTypes.UpperBody, 30));
        }

        [TestMethod]
        public async Task PostSoloTrainingResult_ShouldThrow_OnNullExercises()
        {
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
                await _repository.PostSoloTrainingResult("u1", DateTime.UtcNow, null!, TrainingTypes.UpperBody, 30));
        }

        // Previously tested "missing trainingType" — now test invalid enum value:
        [TestMethod]
        public async Task PostSoloTrainingResult_ShouldThrow_OnInvalidTrainingType()
        {
            var exercises = new List<Exercise>();
            // use an out-of-range enum value to simulate invalid input
            var invalidType = (TrainingTypes)(-1);

            await Assert.ThrowsExceptionAsync<ArgumentException>(async () =>
                await _repository.PostSoloTrainingResult("u1", DateTime.UtcNow, exercises, invalidType, 30));
        }

        [TestMethod]
        public async Task PostSoloTrainingResult_ShouldThrow_OnNegativeDuration()
        {
            var exercises = new List<Exercise>();
            await Assert.ThrowsExceptionAsync<ArgumentOutOfRangeException>(async () =>
                await _repository.PostSoloTrainingResult("u1", DateTime.UtcNow, exercises, TrainingTypes.UpperBody, -5));
        }


        [TestMethod]
        public async Task GetCrowdCount_WhenNoEntries_ReturnsZero()
        {
            // CountDocumentsAsync was setup to return 0 in Init
            var count = await _repository.GetCrowdCount();
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public async Task UpdateUserExitTime_ShouldReturnId_WhenUpdated()
        {
            // arrange: make FindOneAndUpdateAsync return a populated DTO
            var updated = new CrowdResultDTO { UserId = "user123", ExitTime = DateTime.UtcNow, Id = "id-1" };

            _mockCrowdCollection
                .Setup(c => c.FindOneAndUpdateAsync(
                    It.IsAny<FilterDefinition<CrowdResultDTO>>(),
                    It.IsAny<UpdateDefinition<CrowdResultDTO>>(),
                    It.IsAny<FindOneAndUpdateOptions<CrowdResultDTO>>(),
                    default))
                .ReturnsAsync(updated);

            var result = await _repository.UpdateUserExitTime("user123", updated.ExitTime);

            Assert.IsNotNull(result);
            StringAssert.Contains(result, "id-1");
            _mockCrowdCollection.Verify(c => c.FindOneAndUpdateAsync(
                It.IsAny<FilterDefinition<CrowdResultDTO>>(),
                It.IsAny<UpdateDefinition<CrowdResultDTO>>(),
                It.IsAny<FindOneAndUpdateOptions<CrowdResultDTO>>(),
                default), Times.Once);
        }
    }
}
