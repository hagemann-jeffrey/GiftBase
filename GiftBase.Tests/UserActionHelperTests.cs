using GiftBase.Core.Exceptions;
using GiftBase.Core.Interfaces;
using GiftBase.Shared.Services;
using MudBlazor;
using NSubstitute;

namespace GiftBase.Tests;

public class UserActionHelperTests
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ISnackbar _snackbar;
    private readonly UserActionHelper _userActionHelper;

    public UserActionHelperTests()
    {
        _currentUserService = Substitute.For<ICurrentUserService>();
        _snackbar = Substitute.For<ISnackbar>();
        _userActionHelper = new UserActionHelper(_currentUserService, _snackbar);
    }

    [Fact]
    public async Task ExecuteIfLoggedInAsync_UserNotLoggedIn_ShouldShowWarningSnackbar()
    {
        // Arrange
        _currentUserService.GetCurrentUserIdAsync().Returns(Task.FromResult<int?>(null));

        // Act
        await _userActionHelper.ExecuteIfLoggedInAsync(async (userId) => { /* This should not be called */ });

        // Assert
        _snackbar.Received(1).Add("Sie müssen angemeldet sein, um diese Aktion auszuführen.", Severity.Warning);
    }

    [Fact]
    public async Task ExecuteIfLoggedInAsync_UserLoggedIn_ShouldExecuteAction()
    {
        // Arrange
        _currentUserService.GetCurrentUserIdAsync().Returns(Task.FromResult<int?>(1));

        // Act
        await _userActionHelper.ExecuteIfLoggedInAsync(async (userId) => { /* This should be called */ });

        // Assert
        await _currentUserService.Received(1).GetCurrentUserIdAsync();
        _snackbar.DidNotReceive().Add(Arg.Any<string>(), Arg.Any<Severity>());
    }

    [Fact]
    public async Task ExecuteIfLoggedInAsync_ActionThrowsNotFoundException_ShouldShowWarningSnackbar()
    {
        // Arrange
        _currentUserService.GetCurrentUserIdAsync().Returns(Task.FromResult<int?>(1));

        // Act
        await _userActionHelper.ExecuteIfLoggedInAsync(async (userId) =>
        {
            throw new NotFoundException("Person konnte nicht gefunden werden: 999");
        });

        // Assert
        _snackbar.Received(1).Add("Nicht gefunden: Person konnte nicht gefunden werden: 999", Severity.Warning);
    }

    [Fact]
    public async Task ExecuteIfLoggedInAsync_ActionThrowsException_ShouldShowErrorSnackbarWithMessage()
    {
        // Arrange
        _currentUserService.GetCurrentUserIdAsync().Returns(Task.FromResult<int?>(1));

        // Act
        await _userActionHelper.ExecuteIfLoggedInAsync(async (userId) =>
        {
            throw new Exception("Test exception");
        });

        // Assert
        _snackbar.Received(1).Add("Ein Fehler ist aufgetreten: Test exception", Severity.Error);
    }
}