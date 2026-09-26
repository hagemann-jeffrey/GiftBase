namespace GiftBase.Core.Interfaces;

public interface IAuthService
{
    Task RegisterUserAsync(string email, string password, string baseUrl);
    Task ConfirmEmailAsync(string token);
    Task<int?> LoginUserAsync(string email, string password);
}
