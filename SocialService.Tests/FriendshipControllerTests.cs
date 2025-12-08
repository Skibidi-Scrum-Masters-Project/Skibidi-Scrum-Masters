using Microsoft.AspNetCore.Mvc;
using FitnessApp.Shared.Models;
using SocialService.Controllers;
using Moq;
using SocialService.Repositories;

namespace SocialService.Tests;

[TestClass]
public class FriendshipControllerTests
{
    private FriendshipController _controller = null!;
    private Mock<IFriendshipRepository> _mockRepository = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new Mock<IFriendshipRepository>();
        _controller = new FriendshipController(_mockRepository.Object);
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