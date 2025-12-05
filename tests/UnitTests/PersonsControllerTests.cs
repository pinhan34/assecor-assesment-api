using Xunit;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using assecor_assesment_api.Data;
using assecor_assesment_api.Models;
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

            public Task<IEnumerable<Person>> GetAllAsync(CancellationToken cancellationToken = default)
                => Task.FromResult<IEnumerable<Person>>(_items);

            public Task<Person?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
                => Task.FromResult(_items.FirstOrDefault(p => p.Id == id));
        }

        [Fact]
        public async Task GetAll_ReturnsAllPersons()
        {
            var controller = new PersonsController(new FakeRepo());
            var result = await controller.GetAll(CancellationToken.None) as OkObjectResult;
            Assert.NotNull(result);
            var people = Assert.IsAssignableFrom<IEnumerable<Person>>(result.Value);
            Assert.Equal(4, people.Count());
        }

        [Fact]
        public async Task GetById_ReturnsPerson_WhenExists_And_NotFound_WhenMissing()
        {
            var controller = new PersonsController(new FakeRepo());

            var ok = await controller.GetById(3, CancellationToken.None) as OkObjectResult;
            Assert.NotNull(ok);
            var p = Assert.IsType<Person>(ok.Value);
            Assert.Equal(3, p.Id);

            var notFound = await controller.GetById(999, CancellationToken.None) as NotFoundResult;
            Assert.NotNull(notFound);
        }

        [Fact]
        public async Task GetByColor_ReturnsPersonsWithThatColor()
        {
            var controller = new PersonsController(new FakeRepo());

            // Color 1 should return 2 persons (Hans and Jane)
            var result = await controller.GetByColor(1, CancellationToken.None) as OkObjectResult;
            Assert.NotNull(result);
            var people = Assert.IsAssignableFrom<IEnumerable<Person>>(result.Value);
            Assert.Equal(2, people.Count());
            Assert.All(people, p => Assert.Equal(1, p.Color));

            // Color 2 should return 1 person (Johnny)
            result = await controller.GetByColor(2, CancellationToken.None) as OkObjectResult;
            Assert.NotNull(result);
            people = Assert.IsAssignableFrom<IEnumerable<Person>>(result.Value);
            Assert.Single(people);
            Assert.Equal(2, people.First().Color);
        }

        [Fact]
        public void GetByColor_ThrowsColorNotFoundExceptionForInvalidColor()
        {
            var controller = new PersonsController(new FakeRepo());

            // Color 99 is out of range (1-7)
            var exception = Record.Exception(() => controller.GetByColor(99, CancellationToken.None).GetAwaiter().GetResult());
            Assert.NotNull(exception);
            Assert.IsType<assecor_assesment_api.Exceptions.ColorNotFoundInTheListException>(exception);
        }
    }
}
