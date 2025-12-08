using Microsoft.EntityFrameworkCore;
using assecor_assesment_api.Models;
using assecor_assesment_api.Exceptions;

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
            try
            {
                return await _context.Persons
                    .OrderBy(p => p.Id)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new DatabaseException($"Error retrieving persons from database: {ex.Message}", DatabaseOperation.Read, ex);
            }
        }

        public async Task<Person?> GetPersonByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Persons
                    .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new DatabaseException($"Error retrieving person with ID {id} from database: {ex.Message}", DatabaseOperation.Read, ex);
            }
        }

        public async Task<IEnumerable<Person>> GetPersonsByColorAsync(int color, CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.Persons
                    .Where(p => p.Color == color)
                    .OrderBy(p => p.Id)
                    .ToListAsync(cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new DatabaseException($"Error retrieving persons with color {color} from database: {ex.Message}", DatabaseOperation.Read, ex);
            }
        }

        public async Task<Person> AddPersonAsync(Person person, CancellationToken cancellationToken = default)
        {
            try
            {
                _context.Persons.Add(person);
                await _context.SaveChangesAsync(cancellationToken);
                return person;
            }
            catch (DbUpdateException ex)
            {
                throw new DatabaseException($"Error saving person to database: {ex.Message}", DatabaseOperation.Write, ex);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new DatabaseException($"Unexpected error while adding person: {ex.Message}", DatabaseOperation.Write, ex);
            }
        }
    }
}
