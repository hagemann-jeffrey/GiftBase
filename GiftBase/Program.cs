using MudBlazor.Services;
using Microsoft.EntityFrameworkCore;
using GiftBase.Data;
using GiftBase.Core.Interfaces;
using GiftBase.Features.Users.Login;
using GiftBase;
using GiftBase.Features.Gifts;
using GiftBase.Features.Notifications;
using GiftBase.Shared.Common;
using MudBlazor;
using System.Globalization;

CultureInfo.DefaultThreadCurrentCulture = AppCulture.German;
CultureInfo.DefaultThreadCurrentUICulture = AppCulture.German;

var builder = WebApplication.CreateBuilder(args);

// Add MudBlazor services
builder.Services.AddMudServices(opt =>
{
    opt.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopCenter;
    opt.SnackbarConfiguration.ClearAfterNavigation = false;
    opt.SnackbarConfiguration.PreventDuplicates = false;
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddGiftBaseOptions(builder.Configuration);
builder.Services.AddGiftBaseData(builder.Configuration);
builder.Services.AddGiftBaseServices();
builder.Services.AddGiftBaseAuthentication();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<GiftBaseDbContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ein Fehler ist beim Migrieren der Datenbank aufgetreten.");
        throw;
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseRequestLocalization(new RequestLocalizationOptions()
    .SetDefaultCulture(AppCulture.Name)
    .AddSupportedCultures(AppCulture.Name)
    .AddSupportedUICultures(AppCulture.Name));

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapLoginEndpoints();
app.MapGiftImageEndpoints();
app.MapNotificationEndpoints();

app.Run();
