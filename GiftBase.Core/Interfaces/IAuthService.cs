namespace GiftBase.Core.Interfaces;

public interface IAuthService
{
    Task<bool> RegisterUserAsync(string email, string password);
    Task<bool> ConfirmEmailAsync(string token);
    Task<int?> LoginUserAsync(string email, string password);
}
