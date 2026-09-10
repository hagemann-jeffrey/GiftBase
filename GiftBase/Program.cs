using MudBlazor.Services;
using Microsoft.EntityFrameworkCore;
using GiftBase.Data;
using GiftBase.Core.Interfaces;
using GiftBase.Features.User;
using Microsoft.AspNetCore.Identity;
using GiftBase.Core.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using GiftBase.Features.User.Login;
using GiftBase;
using GiftBase.Features.Gifts;
using GiftBase.Features.Occasions;
using GiftBase.Features.Persons;
using GiftBase.Features.Sharing;
using GiftBase.Shared;
using GiftBase.Shared.Services;
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

builder.Services.AddDbContextFactory<GiftBaseDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("GiftBase"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 6,
            maxRetryDelay: TimeSpan.FromSeconds(15),
            errorNumbersToAdd: null)));

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IPersonService, PersonService>();
builder.Services.AddScoped<IGiftService, GiftService>();
builder.Services.AddScoped<IOccasionService, OccasionService>();
builder.Services.AddScoped<IShareLinkService, ShareLinkService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<UserActionHelper>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "GiftBase.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.IsEssential = true;
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/login";
        options.ExpireTimeSpan = TimeSpan.FromDays(30);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

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

app.Run();
