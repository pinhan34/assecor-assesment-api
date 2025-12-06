using Microsoft.EntityFrameworkCore;
using assecor_assesment_api.Models;

namespace assecor_assesment_api.Data
{
    public class DatabasePersonRepository : IPersonRepository
    {
        private readonly PersonDbContext _context;

        public DatabasePersonRepository(PersonDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Person>> GetAllPersonsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Persons
                .OrderBy(p => p.Id)
                .ToListAsync(cancellationToken);
        }

        public async Task<Person?> GetPersonByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Persons
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }

        public async Task<Person> AddPersonAsync(Person person, CancellationToken cancellationToken = default)
        {
            _context.Persons.Add(person);
            await _context.SaveChangesAsync(cancellationToken);
            return person;
        }
    }
}
