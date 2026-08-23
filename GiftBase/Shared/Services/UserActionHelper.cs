using GiftBase.Core.Exceptions;
using GiftBase.Core.Interfaces;
using MudBlazor;

namespace GiftBase.Shared.Services;

public class UserActionHelper(ICurrentUserService currentUserService, ISnackbar snackbar)
{
    public async Task ExecuteIfLoggedInAsync(Func<int, Task> action)
    {
        var userId = await currentUserService.GetCurrentUserIdAsync();

        if (!userId.HasValue)
        {
            snackbar.Add("Sie müssen angemeldet sein, um diese Aktion auszuführen.", Severity.Warning);
            return;
        }

        try
        {
            await action(userId.Value);
        }
        catch (NotFoundException ex)
        {
            snackbar.Add($"Nicht gefunden: {ex.Message}", Severity.Warning);
        }
        catch (Exception ex)
        {
            snackbar.Add($"Ein Fehler ist aufgetreten: {ex.Message}", Severity.Error);
        }
    }
}