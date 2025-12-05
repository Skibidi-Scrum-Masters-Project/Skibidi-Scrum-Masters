using Microsoft.AspNetCore.Mvc;
using FitnessApp.Shared.Models;
using SocialService.Controllers;
using Moq;

namespace SocialService.Tests;

[TestClass]
public class FriendsControllerTests
{
    private FriendsController _controller = null!;
    private Mock<IFriendRepository> _mockRepository = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new Mock<IFriendRepository>();
        _controller = new FriendsController(_mockRepository.Object);
    }

    [TestMethod]
    public void GetUserFriends_ShouldReturnUsersFriends()
    {
        // TBA: Implement test for getting user's friends
        Assert.Inconclusive("Test not implemented yet");
    }

    [TestMethod]
    public void GetUserFriends_WhenNoFriends_ShouldReturnEmptyList()
    {
        // TBA: Implement test for empty friends list
        Assert.Inconclusive("Test not implemented yet");
    }

    [TestMethod]
    public void GetUserFriends_WithValidUserId_ShouldReturnOkResult()
    {
        // TBA: Implement test for OK status code
        Assert.Inconclusive("Test not implemented yet");
    }
}