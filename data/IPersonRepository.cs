using assecor_assesment_api.Models;

namespace assecor_assesment_api.Data
{
    public interface IPersonRepository
    {
        Task<IEnumerable<Person>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<Person?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Person> AddPersonAsync(Person person, CancellationToken cancellationToken = default);
    }
}
