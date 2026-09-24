using MudBlazor;

namespace GiftBase.Shared.Common;

public static class AppDialogOptions
{
    public static DialogOptions Default => new()
    {
        CloseButton = true,
        MaxWidth = MaxWidth.Small,
        FullWidth = true,
        CloseOnEscapeKey = true
    };

    public static DialogOptions Medium => new()
    {
        CloseButton = true,
        MaxWidth = MaxWidth.Medium,
        FullWidth = true,
        CloseOnEscapeKey = true
    };
}
