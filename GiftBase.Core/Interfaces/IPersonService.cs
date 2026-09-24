using GiftBase.Core.Dtos.Persons;
using GiftBase.Core.Entities;

namespace GiftBase.Core.Interfaces;

public interface IPersonService
{
    Task<List<Person>> GetPersonsAsync(int userId);
    Task<Person> GetPersonAsync(int personId, int currentUserId);
    Task<Person> AddPersonAsync(PersonAddDto personAddDto);
    Task<Person> UpdatePersonAsync(int personId, int currentUserId, PersonUpdateDto personUpdateDto);
    Task DeletePersonAsync(int personId, int currentUserId);
}
