using Xunit;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using assecor_assesment_api.Data;
using assecor_assesment_api.Models;

#nullable enable

namespace assecor_assesment_api.UnitTests
{
    public class DatabasePersonRepositoryTests : IDisposable
    {
        private readonly PersonDbContext _context;
        private readonly DatabasePersonRepository _repository;

        public DatabasePersonRepositoryTests()
        {
            // Use in-memory SQLite database for testing
            var options = new DbContextOptionsBuilder<PersonDbContext>()
                .UseSqlite("DataSource=:memory:")
                .Options;

            _context = new PersonDbContext(options);
            _context.Database.OpenConnection();
            _context.Database.EnsureCreated();

            _repository = new DatabasePersonRepository(_context);
        }

        public void Dispose()
        {
            _context.Database.CloseConnection();
            _context.Dispose();
        }

        [Fact]
        public async Task GetAllPersonsAsync_ReturnsAllPersons_OrderedById()
        {
            // Arrange - seed data is already in the database from PersonDbContext

            // Act
            var result = await _repository.GetAllPersonsAsync();
            var persons = result.ToList();

            // Assert
            Assert.NotEmpty(persons);
            Assert.Equal(3, persons.Count); // Hans, Peter, Johnny from seed data
            Assert.Equal(1, persons[0].Id);
            Assert.Equal(2, persons[1].Id);
            Assert.Equal(3, persons[2].Id);
        }

        [Fact]
        public async Task GetAllPersonsAsync_ReturnsEmptyList_WhenDatabaseIsEmpty()
        {
            // Arrange - remove all persons
            _context.Persons.RemoveRange(_context.Persons);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllPersonsAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetPersonByIdAsync_ReturnsCorrectPerson_WhenPersonExists()
        {
            // Arrange
            var expectedId = 1;

            // Act
            var result = await _repository.GetPersonByIdAsync(expectedId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedId, result.Id);
            Assert.Equal("Hans", result.FirstName);
        }

        [Fact]
        public async Task GetPersonByIdAsync_ReturnsNull_WhenPersonDoesNotExist()
        {
            // Arrange
            var nonExistentId = 999;

            // Act
            var result = await _repository.GetPersonByIdAsync(nonExistentId);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task AddPersonAsync_AddsPersonToDatabase_AndReturnsPersonWithId()
        {
            // Arrange
            var newPerson = new Person
            {
                FirstName = "Test",
                LastName = "User",
                Address = "12345 Test Street",
                Color = 4
            };

            // Act
            var result = await _repository.AddPersonAsync(newPerson);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0); // ID should be auto-generated
            Assert.Equal("Test", result.FirstName);
            Assert.Equal("User", result.LastName);

            // Verify it's actually in the database
            var personFromDb = await _context.Persons.FindAsync(result.Id);
            Assert.NotNull(personFromDb);
            Assert.Equal("Test", personFromDb.FirstName);
        }

        [Fact]
        public async Task AddPersonAsync_WithMinimalData_SavesSuccessfully()
        {
            // Arrange
            var newPerson = new Person
            {
                FirstName = "OnlyFirstName",
                LastName = null,
                Address = null,
                Color = null
            };

            // Act
            var result = await _repository.AddPersonAsync(newPerson);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Id > 0);
            Assert.Equal("OnlyFirstName", result.FirstName);
            Assert.Null(result.LastName);
            Assert.Null(result.Address);
            Assert.Null(result.Color);
        }

        [Fact]
        public async Task AddPersonAsync_IncreasesTotalCount()
        {
            // Arrange
            var initialCount = await _context.Persons.CountAsync();
            var newPerson = new Person
            {
                FirstName = "Another",
                LastName = "Person",
                Color = 2
            };

            // Act
            await _repository.AddPersonAsync(newPerson);
            var newCount = await _context.Persons.CountAsync();

            // Assert
            Assert.Equal(initialCount + 1, newCount);
        }
    }
}
