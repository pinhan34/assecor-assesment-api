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
                new Person { Id = 1, FirstName = "Hans", LastName = "Müller", Address = "A" },
                new Person { Id = 3, FirstName = "Johnny", LastName = "Johnson", Address = "B" }
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
            Assert.Equal(2, people.Count());
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
    }
}
