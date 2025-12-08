using assecor_assesment_api.Models;

namespace assecor_assesment_api.Data
{
    public interface IPersonRepository
    {
        Task<IEnumerable<Person>> GetAllPersonsAsync(CancellationToken cancellationToken = default);
        Task<Person?> GetPersonByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<IEnumerable<Person>> GetPersonsByColorAsync(int color, CancellationToken cancellationToken = default);
        Task<Person> AddPersonAsync(Person person, CancellationToken cancellationToken = default);
    }
}
