using Xunit;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using assecor_assesment_api.Data;
using assecor_assesment_api.Models;
using Microsoft.Extensions.Configuration;

#nullable enable

namespace assecor_assesment_api.UnitTests
{
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

        private static string CreateCsv(params string[] lines) 
            => string.Join(Environment.NewLine, lines);

        [Fact]
        public async Task ParseLine_AssignsLineNumberAsId()
        {
            var csv = CreateCsv(
                "Müller, Hans, 67742 Lauterecken, 1",
                "Petersen, Peter, 18439 Stralsund, 2",
                "Johnson, Johnny, 88888 made up, 3"
            );

            var config = CreateTestConfiguration(csv);
            var repo = new CsvPersonRepository(config);
            var people = (await repo.GetAllPersonsAsync()).ToList();

            Assert.Equal(3, people.Count);
            Assert.Equal(1, people[0].Id);
            Assert.Equal(2, people[1].Id);
            Assert.Equal(3, people[2].Id);
        }

        [Fact]
        public async Task ParseLine_HandlesRowsWithMissingOptionalFields()
        {
            var csv = CreateCsv(
                "Bart, Bertram,",
                "Müller, Hans, 67742 Lauterecken",
                "Müller, Hans, 67742 Lauterecken, 1"
            );

            var config = CreateTestConfiguration(csv);
            var repo = new CsvPersonRepository(config);
            var people = (await repo.GetAllPersonsAsync()).ToList();

            // All rows should be parsed even with missing optional fields
            Assert.Equal(3, people.Count);
            
            // First row: missing address and color
            Assert.Equal("Bart", people[0].LastName);
            Assert.Equal("Bertram", people[0].FirstName);
            Assert.Null(people[0].Address);
            Assert.Null(people[0].Color);
            
            // Second row: missing color only
            Assert.Equal("Müller", people[1].LastName);
            Assert.Equal("Hans", people[1].FirstName);
            Assert.Equal("67742 Lauterecken", people[1].Address);
            Assert.Null(people[1].Color);
            
            // Third row: all fields present
            Assert.Equal("Müller", people[2].LastName);
            Assert.Equal("Hans", people[2].FirstName);
            Assert.Equal("67742 Lauterecken", people[2].Address);
            Assert.Equal(1, people[2].Color);
        }

        [Fact]
        public async Task ParseLine_SkipsRowWithMissingBothNames()
        {
            var csv = CreateCsv(
                "Müller, Hans, 67742 Lauterecken, 1",
                " , , 12313 Wasweißich, 1",
                "Petersen, Peter, 18439 Stralsund, 2"
            );

            var config = CreateTestConfiguration(csv);
            var repo = new CsvPersonRepository(config);
            var people = (await repo.GetAllPersonsAsync()).ToList();

            // Second row has no names - should be skipped
            // IDs should be 1 and 3 (line numbers preserved even when rows are skipped)
            Assert.Equal(2, people.Count);
            Assert.Equal(1, people[0].Id);
            Assert.Equal("Müller", people[0].LastName);
            Assert.Equal(3, people[1].Id);
            Assert.Equal("Petersen", people[1].LastName);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsPersonByLineNumber()
        {
            var csv = CreateCsv(
                "Müller, Hans, 67742 Lauterecken, 1",
                "Petersen, Peter, 18439 Stralsund, 2",
                "Johnson, Johnny, 88888 made up, 3"
            );

            var config = CreateTestConfiguration(csv);
            var repo = new CsvPersonRepository(config);
            
            var person2 = await repo.GetPersonByIdAsync(2);
            Assert.NotNull(person2);
            Assert.Equal(2, person2.Id);
            Assert.Equal("Petersen", person2.LastName);
            Assert.Equal("Peter", person2.FirstName);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNullForNonexistentId()
        {
            var csv = CreateCsv(
                "Müller, Hans, 67742 Lauterecken, 1"
            );

            var config = CreateTestConfiguration(csv);
            var repo = new CsvPersonRepository(config);
            
            var person = await repo.GetPersonByIdAsync(999);
            Assert.Null(person);
        }

        [Fact]
        public async Task ParseLine_HandlesEmptyLines()
        {
            var csv = CreateCsv(
                "Müller, Hans, 67742 Lauterecken, 1",
                "",
                "Petersen, Peter, 18439 Stralsund, 2"
            );

            var config = CreateTestConfiguration(csv);
            var repo = new CsvPersonRepository(config);
            var people = (await repo.GetAllPersonsAsync()).ToList();

            // Empty line should be skipped, but IDs should still be line numbers
            Assert.Equal(2, people.Count);
            Assert.Equal(1, people[0].Id);
            Assert.Equal(3, people[1].Id); // Line 3 (line 2 was empty)
        }
    }
}
