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

            _mockDatabase
                .Setup(db => db.GetCollection<ClassResultDTO>("ClassResults", null))
                .Returns(_mockClassCollection.Object);

            _mockDatabase
                .Setup(db => db.GetCollection<CrowdResultDTO>("CrowdResults", null))
                .Returns(_mockCrowdCollection.Object);

            _mockDatabase
                .Setup(db => db.GetCollection<SoloTrainingResultsDTO>("SoloTrainingResults", null))
                .Returns(_mockSoloCollection.Object);

            _mockClassCollection
                .Setup(c => c.InsertOneAsync(It.IsAny<ClassResultDTO>(), null, default))
                .Returns(Task.CompletedTask);

            _mockCrowdCollection
                .Setup(c => c.InsertOneAsync(It.IsAny<CrowdResultDTO>(), null, default))
                .Returns(Task.CompletedTask);

            _mockSoloCollection
                .Setup(c => c.InsertOneAsync(It.IsAny<SoloTrainingResultsDTO>(), null, default))
                .Returns(Task.CompletedTask);

            _mockCrowdCollection
                .Setup(c => c.CountDocumentsAsync(It.IsAny<FilterDefinition<CrowdResultDTO>>(), null, default))
                .ReturnsAsync(0);

            _mockCrowdCollection
                .Setup(c => c.FindOneAndUpdateAsync(
                    It.IsAny<FilterDefinition<CrowdResultDTO>>(),
                    It.IsAny<UpdateDefinition<CrowdResultDTO>>(),
                    It.IsAny<FindOneAndUpdateOptions<CrowdResultDTO>>(),
                    default))
                .ReturnsAsync((CrowdResultDTO?)null);

            _repository = new AnalyticsRepository(_mockDatabase.Object, new HttpClient());
        }

        // -----------------------------
        // Class analytics
        // -----------------------------

        [TestMethod]
        public async Task PostClassesAnalytics_ShouldInsertAndReturnClassResult()
        {
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

            var result = await _repository.PostClassesAnalytics(dto);

            Assert.IsNotNull(result);
            Assert.AreEqual("class123", result.ClassId);

            _mockClassCollection.Verify(
                c => c.InsertOneAsync(It.IsAny<ClassResultDTO>(), null, default),
                Times.Once);
        }

        [TestMethod]
        public async Task PostClassesAnalytics_ShouldThrow_OnNullDto()
        {
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
                _repository.PostClassesAnalytics(null!));
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
        public async Task PostSoloTrainingResult_ShouldInsertSoloTraining()
        {
            var dto = new SoloTrainingResultsDTO
            {
                UserId = "user123",
                Date = DateTime.UtcNow,
                TrainingType = TrainingTypes.UpperBody,
                DurationMinutes = 30,
                Exercises = new List<Exercise>
                {
                    new Exercise
                    {
                        ExerciseType = ExerciseType.BenchPress,
                        Volume = 100,
                        Sets = new List<Set>
                        {
                            new Set { Repetitions = 10, Weight = 50 }
                        }
                    }
                }
            };

            var result = await _repository.PostSoloTrainingResult(dto);

            Assert.IsNotNull(result);

            _mockSoloCollection.Verify(
                c => c.InsertOneAsync(It.IsAny<SoloTrainingResultsDTO>(), null, default),
                Times.Once);
        }


        [TestMethod]
        public async Task PostSoloTrainingResult_ShouldThrow_OnNullDto()
        {
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(() =>
                _repository.PostSoloTrainingResult(null!));
        }

        [TestMethod]
        public async Task GetCrowdCount_WhenNoEntries_ReturnsZero()
        {
            var count = await _repository.GetCrowdCount();
            Assert.AreEqual(0, count);
        }
    }
}
