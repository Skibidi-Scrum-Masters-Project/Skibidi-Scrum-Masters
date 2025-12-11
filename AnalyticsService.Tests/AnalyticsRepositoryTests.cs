using FitnessApp.Shared.Models;
using Moq;
using Moq.Protected;
using System.Net;

namespace AnalyticsService.Tests;

[TestClass]
public class AnalyticsRepositoryTests
{
    private AnalyticsRepository _repository = null!;
    private Mock<HttpMessageHandler> _mockHttpMessageHandler = null!;
    private HttpClient _httpClient = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        _repository = new AnalyticsRepository(_httpClient);
    }

    #region GetCrowd Tests

    [TestMethod]
    public async Task GetCrowd_ValidResponse_ReturnsCrowdCount()
    {
        // Arrange
        var expectedCrowdCount = 42;
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(expectedCrowdCount.ToString())
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _repository.GetCrowd();

        // Assert
        Assert.AreEqual(expectedCrowdCount, result);
        _mockHttpMessageHandler.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [TestMethod]
    public async Task GetCrowd_ZeroCrowd_ReturnsZero()
    {
        // Arrange
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("0")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _repository.GetCrowd();

        // Assert
        Assert.AreEqual(0, result);
    }

    [TestMethod]
    public async Task GetCrowd_LargeCrowdCount_ReturnsValue()
    {
        // Arrange
        var expectedCrowdCount = 500;
        var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(expectedCrowdCount.ToString())
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(responseMessage);

        // Act
        var result = await _repository.GetCrowd();

        // Assert
        Assert.AreEqual(expectedCrowdCount, result);
    }

    #endregion
}
