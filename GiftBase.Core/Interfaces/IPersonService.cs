using GiftBase.Core.Dtos;
using GiftBase.Core.Entities;

namespace GiftBase.Core.Interfaces;

public interface IPersonService
{
    Task<List<Person>> GetPersonsAsync(int userId);
    Task<Person> AddPersonAsync(PersonAddDto personAddDto);
    Task<Person> UpdatePersonAsync(int personId, int currentUserId, PersonUpdateDto personUpdateDto);
    Task DeletePersonAsync(int personId, int currentUserId);
}
