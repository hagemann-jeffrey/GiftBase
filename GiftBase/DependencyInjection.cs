using GiftBase.Core.Entities;
using GiftBase.Core.Interfaces;
using GiftBase.Features.Gifts;
using GiftBase.Features.GiftSuggestions;
using GiftBase.Features.Notifications;
using GiftBase.Features.Occasions;
using GiftBase.Features.Persons;
using GiftBase.Features.Sharing;
using GiftBase.Features.Users;
using GiftBase.Shared.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;

namespace GiftBase;

public static class DependencyInjection
{
    public static IServiceCollection AddGiftBaseServices(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IPersonService, PersonService>();
        services.AddScoped<IGiftService, GiftService>();
        services.AddScoped<IOccasionService, OccasionService>();
        services.AddScoped<IShareLinkService, ShareLinkService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IGeminiClient, GeminiClient>();
        services.AddScoped<IGiftSuggestionService, GiftSuggestionService>();
        services.AddScoped<UserActionHelper>();

        return services;
    }

    public static IServiceCollection AddGiftBaseAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
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

        services.AddAuthorization();
        services.AddCascadingAuthenticationState();

        return services;
    }
}
