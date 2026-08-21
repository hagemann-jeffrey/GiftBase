namespace GiftBase.Core.Interfaces;

public interface ICurrentUserService
{
    Task<int?> GetCurrentUserIdAsync();
}
