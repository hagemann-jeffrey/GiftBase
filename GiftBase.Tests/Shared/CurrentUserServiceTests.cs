using System.Security.Claims;
using GiftBase.Shared.Services;
using Microsoft.AspNetCore.Components.Authorization;
using NSubstitute;
using Shouldly;

namespace GiftBase.Tests.Shared;

public class CurrentUserServiceTests
{
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly CurrentUserService _sut;

    public CurrentUserServiceTests()
    {
        _authStateProvider = Substitute.For<AuthenticationStateProvider>();
        _sut = new CurrentUserService(_authStateProvider);
    }

    [Fact]
    public async Task GetCurrentUserIdAsync_AuthenticatedWithValidClaim_ReturnsUserId()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "42") };
        var identity = new ClaimsIdentity(claims, "test");
        var user = new ClaimsPrincipal(identity);
        _authStateProvider.GetAuthenticationStateAsync()
            .Returns(new AuthenticationState(user));

        // Act
        var result = await _sut.GetCurrentUserIdAsync();

        // Assert
        result.ShouldBe(42);
    }

    [Fact]
    public async Task GetCurrentUserIdAsync_NotAuthenticated_ReturnsNull()
    {
        // Arrange
        var identity = new ClaimsIdentity(); // IsAuthenticated = false
        var user = new ClaimsPrincipal(identity);
        _authStateProvider.GetAuthenticationStateAsync()
            .Returns(new AuthenticationState(user));

        // Act
        var result = await _sut.GetCurrentUserIdAsync();

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetCurrentUserIdAsync_AuthenticatedButNoNameIdentifierClaim_ReturnsNull()
    {
        // Arrange
        var identity = new ClaimsIdentity([], "test"); // authenticated, but no NameIdentifier
        var user = new ClaimsPrincipal(identity);
        _authStateProvider.GetAuthenticationStateAsync()
            .Returns(new AuthenticationState(user));

        // Act
        var result = await _sut.GetCurrentUserIdAsync();

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetCurrentUserIdAsync_AuthenticatedButInvalidClaimValue_ReturnsNull()
    {
        // Arrange
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "not-a-number") };
        var identity = new ClaimsIdentity(claims, "test");
        var user = new ClaimsPrincipal(identity);
        _authStateProvider.GetAuthenticationStateAsync()
            .Returns(new AuthenticationState(user));

        // Act
        var result = await _sut.GetCurrentUserIdAsync();

        // Assert
        result.ShouldBeNull();
    }
}