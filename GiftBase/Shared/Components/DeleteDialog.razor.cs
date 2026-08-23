using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GiftBase.Shared.Components;

public partial class DeleteDialog
{
    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = null!;
    [Parameter]
    public string Message { get; set; } = "Sind Sie sicher, dass Sie löschen möchten?";

    private void Cancel() => MudDialog.Cancel();

    private void ConfirmDelete() => MudDialog.Close(DialogResult.Ok(true));
}
