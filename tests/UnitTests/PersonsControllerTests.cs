using Xunit;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using assecor_assesment_api.Data;
using assecor_assesment_api.Models;
using assecor_assesment_api.Exceptions;
using System.Threading.Tasks;
using assecor_assesment_api.Controllers;

#nullable enable

namespace assecor_assesment_api.UnitTests
{
    public class PersonsControllerTests
    {
        private class FakeRepo : IPersonRepository
        {
            private readonly List<Person> _items = new()
            {
                new Person { Id = 1, FirstName = "Hans", LastName = "Müller", Address = "A", Color = 1 },
                new Person { Id = 3, FirstName = "Johnny", LastName = "Johnson", Address = "B", Color = 2 },
                new Person { Id = 5, FirstName = "Jane", LastName = "Doe", Address = "C", Color = 1 },
                new Person { Id = 7, FirstName = "Bob", LastName = "Smith", Address = "D", Color = 3 }
            };

            public Task<IEnumerable<Person>> GetAllPersonsAsync(CancellationToken cancellationToken = default)
                => Task.FromResult<IEnumerable<Person>>(_items);

            public Task<Person?> GetPersonByIdAsync(int id, CancellationToken cancellationToken = default)
                => Task.FromResult(_items.FirstOrDefault(p => p.Id == id));

            public Task<Person> AddPersonAsync(Person person, CancellationToken cancellationToken = default)
            {
                // Assign next ID
                var nextId = _items.Any() ? _items.Max(p => p.Id) + 1 : 1;
                person.Id = nextId;
                _items.Add(person);
                return Task.FromResult(person);
            }
        }

        [Fact]
        public async Task GetAllPersons_ReturnsAllPersons()
        {
            var controller = new PersonsController(new FakeRepo());
            var result = await controller.GetAllPersons(CancellationToken.None) as OkObjectResult;
            Assert.NotNull(result);
            var people = Assert.IsAssignableFrom<IEnumerable<Person>>(result.Value);
            Assert.Equal(4, people.Count());
        }

        [Fact]
        public async Task GetPersonById_ReturnsPerson_WhenExists()
        {
            var controller = new PersonsController(new FakeRepo());

            var ok = await controller.GetPersonById(3, CancellationToken.None) as OkObjectResult;
            Assert.NotNull(ok);
            var p = Assert.IsType<Person>(ok.Value);
            Assert.Equal(3, p.Id);
        }

        [Fact]
        public void GetPersonById_ThrowsPersonNotFoundException_WhenMissing()
        {
            var controller = new PersonsController(new FakeRepo());

            var exception = Assert.Throws<PersonNotFoundException>(() =>
            {
                controller.GetPersonById(999, CancellationToken.None).GetAwaiter().GetResult();
            });

            Assert.NotNull(exception);
            Assert.Equal(999, exception.RequestedId);
            Assert.Contains("999", exception.Message);
        }

        [Fact]
        public async Task GetByColorName_ReturnsPersonsWithThatColor()
        {
            var controller = new PersonsController(new FakeRepo());

            // Color "Blau" (1) should return 2 persons (Hans and Jane)
            var result = await controller.GetPersonsByColorName("Blau", CancellationToken.None) as OkObjectResult;
            Assert.NotNull(result);
            var people = Assert.IsAssignableFrom<IEnumerable<Person>>(result.Value);
            Assert.Equal(2, people.Count());
            Assert.All(people, p => Assert.Equal(1, p.Color));

            // Color "Grün" (2) should return 1 person (Johnny)
            result = await controller.GetPersonsByColorName("Grün", CancellationToken.None) as OkObjectResult;
            Assert.NotNull(result);
            people = Assert.IsAssignableFrom<IEnumerable<Person>>(result.Value);
            Assert.Single(people);
            Assert.Equal(2, people.First().Color);
        }

        [Fact]
        public async Task GetByColorName_IsCaseInsensitive()
        {
            var controller = new PersonsController(new FakeRepo());

            // Test lowercase
            var result = await controller.GetPersonsByColorName("blau", CancellationToken.None) as OkObjectResult;
            Assert.NotNull(result);
            var people = Assert.IsAssignableFrom<IEnumerable<Person>>(result.Value);
            Assert.Equal(2, people.Count());

            // Test uppercase
            result = await controller.GetPersonsByColorName("VIOLETT", CancellationToken.None) as OkObjectResult;
            Assert.NotNull(result);
            people = Assert.IsAssignableFrom<IEnumerable<Person>>(result.Value);
            Assert.Single(people);
            Assert.Equal(3, people.First().Color);
        }

        [Fact]
        public void GetByColorName_ReturnsBadRequestForInvalidColorName()
        {
            var controller = new PersonsController(new FakeRepo());

            var exception = Assert.Throws<ColorNotFoundInTheListException>(() =>
            {
                controller.GetPersonsByColorName("Purple", CancellationToken.None).GetAwaiter().GetResult();
            });

            Assert.NotNull(exception);
            Assert.Equal("Purple", exception.RequestedColor);
            Assert.Contains("Purple", exception.Message);
            Assert.Contains("Blau", exception.Message);
        }

        [Fact]
        public async Task CreatePerson_CreatesNewPerson()
        {
            var controller = new PersonsController(new FakeRepo());
            var request = new CreatePersonRequest 
            { 
                FirstName = "New", 
                LastName = "Person", 
                Address = "123 Main St", 
                Color = 3 
            };

            var result = await controller.CreatePerson(request, CancellationToken.None) as CreatedAtActionResult;
            Assert.NotNull(result);
            Assert.Equal(nameof(PersonsController.GetPersonById), result.ActionName);
            
            var person = Assert.IsType<Person>(result.Value);
            Assert.Equal("New", person.FirstName);
            Assert.Equal("Person", person.LastName);
            Assert.Equal("123 Main St", person.Address);
            Assert.Equal(3, person.Color);
            Assert.True(person.Id > 0);
        }

        [Fact]
        public void CreatePerson_ThrowsInvalidPersonDataException_ForMissingNames()
        {
            var controller = new PersonsController(new FakeRepo());
            var request = new CreatePersonRequest 
            { 
                FirstName = null, 
                LastName = null, 
                Address = "123 Main St" 
            };

            var exception = Assert.Throws<InvalidPersonDataException>(() =>
            {
                controller.CreatePerson(request, CancellationToken.None).GetAwaiter().GetResult();
            });

            Assert.NotNull(exception);
            Assert.Contains(exception.ValidationErrors, e => e.Contains("FirstName or LastName"));
        }

        [Fact]
        public void CreatePerson_ThrowsInvalidPersonDataException_ForInvalidColor()
        {
            var controller = new PersonsController(new FakeRepo());
            var request = new CreatePersonRequest 
            { 
                FirstName = "Test", 
                LastName = "User", 
                Color = 10 // Invalid: must be 1-7
            };

            var exception = Assert.Throws<InvalidPersonDataException>(() =>
            {
                controller.CreatePerson(request, CancellationToken.None).GetAwaiter().GetResult();
            });

            Assert.NotNull(exception);
            Assert.Contains(exception.ValidationErrors, e => e.Contains("Color must be between 1 and 7"));
        }

        [Fact]
        public async Task CreatePerson_AllowsNullOptionalFields()
        {
            var controller = new PersonsController(new FakeRepo());
            var request = new CreatePersonRequest 
            { 
                FirstName = "John",
                LastName = null,
                Address = null,
                Color = null
            };

            var result = await controller.CreatePerson(request, CancellationToken.None) as CreatedAtActionResult;
            Assert.NotNull(result);
            
            var person = Assert.IsType<Person>(result.Value);
            Assert.Equal("John", person.FirstName);
            Assert.Null(person.LastName);
            Assert.Null(person.Address);
            Assert.Null(person.Color);
        }
    }
}
