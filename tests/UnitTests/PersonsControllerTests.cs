using Xunit;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using assecor_assesment_api.Data;
using assecor_assesment_api.Models;
using System.Threading.Tasks;
using assecor_assesment_api.Controllers;
using Microsoft.Extensions.Configuration;

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

    public class CsvPersonRepositoryTests
    {
        private IConfiguration CreateTestConfiguration(string csvContent)
        {
            // Create a temp CSV file for testing
            var tempFile = Path.GetTempFileName();
            File.WriteAllText(tempFile, csvContent);

            var configData = new Dictionary<string, string?>
            {
                { "PersonsCsvPath", tempFile }
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(configData!)
                .Build();
        }

        [Fact]
        public async Task ParseLine_AssignsLineNumberAsId()
        {
            var csv = @"Müller, Hans, 67742 Lauterecken, 1
Petersen, Peter, 18439 Stralsund, 2
Johnson, Johnny, 88888 made up, 3";

            var config = CreateTestConfiguration(csv);
            var repo = new CsvPersonRepository(config);
            var people = (await repo.GetAllAsync()).ToList();

            Assert.Equal(3, people.Count);
            Assert.Equal(1, people[0].Id);
            Assert.Equal(2, people[1].Id);
            Assert.Equal(3, people[2].Id);
        }

        [Fact]
        public async Task ParseLine_HandlesRowWithMissingAddress()
        {
            var csv = @"Bart, Bertram, 
Müller, Hans, 67742 Lauterecken, 1";

            var config = CreateTestConfiguration(csv);
            var repo = new CsvPersonRepository(config);
            var people = (await repo.GetAllAsync()).ToList();

            // First row has missing address - should still be parsed but with null address
            Assert.Equal(2, people.Count);
            Assert.Equal("Bart", people[0].LastName);
            Assert.Equal("Bertram", people[0].FirstName);
            Assert.Null(people[0].Address);
            Assert.Null(people[0].Color);
        }

        [Fact]
        public async Task ParseLine_SkipsRowWithMissingBothNames()
        {
            var csv = @"Müller, Hans, 67742 Lauterecken, 1
, , 12313 Wasweißich, 1
Petersen, Peter, 18439 Stralsund, 2";

            var config = CreateTestConfiguration(csv);
            var repo = new CsvPersonRepository(config);
            var people = (await repo.GetAllAsync()).ToList();

            // Second row has no names - should be skipped
            // IDs should be 1 and 3 (line numbers preserved even when rows are skipped)
            Assert.Equal(2, people.Count);
            Assert.Equal(1, people[0].Id);
            Assert.Equal("Müller", people[0].LastName);
            Assert.Equal(3, people[1].Id);
            Assert.Equal("Petersen", people[1].LastName);
        }

        [Fact]
        public async Task ParseLine_HandlesColorWithNonNumericCharacters()
        {
            var csv = @"Andersson, Anders, 32132 Schweden - ☀, 2
Müller, Hans, 67742 Lauterecken, 1";

            var config = CreateTestConfiguration(csv);
            var repo = new CsvPersonRepository(config);
            var people = (await repo.GetAllAsync()).ToList();

            Assert.Equal(2, people.Count);
            
            // First row has sun icon (☀) and "2" - TryParseInt should extract the 2
            Assert.Equal("Andersson", people[0].LastName);
            Assert.Equal(2, people[0].Color);
            
            // Address should preserve the full string including sun icon
            Assert.Contains("☀", people[0].Address);
        }

        [Fact]
        public async Task ParseLine_HandlesRowWithMissingColor()
        {
            var csv = @"Müller, Hans, 67742 Lauterecken
Petersen, Peter, 18439 Stralsund, 2";

            var config = CreateTestConfiguration(csv);
            var repo = new CsvPersonRepository(config);
            var people = (await repo.GetAllAsync()).ToList();

            Assert.Equal(2, people.Count);
            
            // First row has no color column
            Assert.Equal("Müller", people[0].LastName);
            Assert.Null(people[0].Color);
            
            // Second row has color
            Assert.Equal("Petersen", people[1].LastName);
            Assert.Equal(2, people[1].Color);
        }

        [Fact]
        public async Task ParseLine_ExtractsNumericColorFromMixedContent()
        {
            var csv = @"Test, User, Some Address, 5abc
Another, Person, Place, x7y";

            var config = CreateTestConfiguration(csv);
            var repo = new CsvPersonRepository(config);
            var people = (await repo.GetAllAsync()).ToList();

            Assert.Equal(2, people.Count);
            
            // TryParseInt should extract digits from mixed content
            Assert.Equal(5, people[0].Color);
            Assert.Equal(7, people[1].Color);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsPersonByLineNumber()
        {
            var csv = @"Müller, Hans, 67742 Lauterecken, 1
Petersen, Peter, 18439 Stralsund, 2
Johnson, Johnny, 88888 made up, 3";

            var config = CreateTestConfiguration(csv);
            var repo = new CsvPersonRepository(config);
            
            var person2 = await repo.GetByIdAsync(2);
            Assert.NotNull(person2);
            Assert.Equal(2, person2.Id);
            Assert.Equal("Petersen", person2.LastName);
            Assert.Equal("Peter", person2.FirstName);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNullForNonexistentId()
        {
            var csv = @"Müller, Hans, 67742 Lauterecken, 1";

            var config = CreateTestConfiguration(csv);
            var repo = new CsvPersonRepository(config);
            
            var person = await repo.GetByIdAsync(999);
            Assert.Null(person);
        }

        [Fact]
        public async Task ParseLine_HandlesEmptyLines()
        {
            var csv = @"Müller, Hans, 67742 Lauterecken, 1

Petersen, Peter, 18439 Stralsund, 2";

            var config = CreateTestConfiguration(csv);
            var repo = new CsvPersonRepository(config);
            var people = (await repo.GetAllAsync()).ToList();

            // Empty line should be skipped, but IDs should still be line numbers
            Assert.Equal(2, people.Count);
            Assert.Equal(1, people[0].Id);
            Assert.Equal(3, people[1].Id); // Line 3 (line 2 was empty)
        }
    }
}
