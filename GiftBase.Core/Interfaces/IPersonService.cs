using GiftBase.Core.Entities;

namespace GiftBase.Core.Interfaces;

public interface IPersonService
{
    Task<List<Person>> GetPersonsAsync(int userId);
}
