using assecor_assesment_api.Models;
using assecor_assesment_api.Exceptions;
using System.Globalization;

namespace assecor_assesment_api.Data
{
    public class CsvPersonRepository : IPersonRepository
    {
        private readonly string _filePath;

        public CsvPersonRepository(IConfiguration configuration)
        {
            // Allow override via configuration, fallback to the data/sample-input.csv in the app folder
            var configured = configuration["PersonsCsvPath"];
            _filePath = string.IsNullOrWhiteSpace(configured)
                ? Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "data", "sample-input.csv")
                : configured!;
        }

        public async Task<IEnumerable<Person>> GetAllPersonsAsync(CancellationToken cancellationToken = default)
        {
            var list = new List<Person>();
            
            if (!File.Exists(_filePath))
            {
                throw new CsvFileException($"CSV file not found at path: {_filePath}", _filePath, CsvFileOperation.Access);
            }

            try
            {
                using var stream = File.OpenRead(_filePath);
                using var reader = new StreamReader(stream);

                string? line;
                int lineNumber = 0;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    lineNumber++;
                    cancellationToken.ThrowIfCancellationRequested();
                    var p = ParseLine(line, lineNumber);
                    if (p != null) list.Add(p);
                }
            }
            catch (IOException ex)
            {
                throw new CsvFileException($"Error reading CSV file: {ex.Message}", _filePath, CsvFileOperation.Read, ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new CsvFileException($"Access denied to CSV file: {ex.Message}", _filePath, CsvFileOperation.Access, ex);
            }

            return list;
        }

        public async Task<Person?> GetPersonByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(_filePath))
            {
                throw new CsvFileException($"CSV file not found at path: {_filePath}", _filePath, CsvFileOperation.Access);
            }

            try
            {
                using var stream = File.OpenRead(_filePath);
                using var reader = new StreamReader(stream);

                string? line;
                int lineNumber = 0;
                while ((line = await reader.ReadLineAsync()) != null)
                {
                    lineNumber++;
                    cancellationToken.ThrowIfCancellationRequested();
                    var p = ParseLine(line, lineNumber);
                    if (p != null && p.Id == id) return p;
                }
            }
            catch (IOException ex)
            {
                throw new CsvFileException($"Error reading CSV file: {ex.Message}", _filePath, CsvFileOperation.Read, ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new CsvFileException($"Access denied to CSV file: {ex.Message}", _filePath, CsvFileOperation.Access, ex);
            }

            return null;
        }

        public async Task<Person> AddPersonAsync(Person person, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Create CSV line: LastName, FirstName, Address, Color
            var lastName = person.LastName ?? string.Empty;
            var firstName = person.FirstName ?? string.Empty;
            var address = person.Address ?? string.Empty;
            var colorStr = person.Color?.ToString() ?? string.Empty;

            var csvLine = $"{lastName}, {firstName}, {address}, {colorStr}";

            // Append to CSV file on a background thread (with lock for thread safety)
            try
            {
                await Task.Run(() =>
                {
                    lock (_filePath)
                    {
                        File.AppendAllText(_filePath, csvLine + Environment.NewLine);
                    }
                }, cancellationToken);

                // Return the created person with the new ID (next line number)
                // Count lines to get the next ID
                var lineCount = File.ReadAllLines(_filePath).Length;
                person.Id = lineCount;
            }
            catch (IOException ex)
            {
                throw new CsvFileException($"Error writing to CSV file: {ex.Message}", _filePath, CsvFileOperation.Write, ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new CsvFileException($"Access denied when writing to CSV file: {ex.Message}", _filePath, CsvFileOperation.Write, ex);
            }

            return person;
        }

        private Person? ParseLine(string line, int lineNumber)
        {
            if (string.IsNullOrWhiteSpace(line)) return null;

            // Simple CSV split on comma, preserving content; trim whitespace
            var parts = line.Split(',').Select(x => x.Trim()).ToArray();

            // Skip rows where both lastname and firstname are missing
            if (string.IsNullOrWhiteSpace(parts.ElementAtOrDefault(0)) && 
                string.IsNullOrWhiteSpace(parts.ElementAtOrDefault(1)))
            {
                return null;
            }

            // Need at least lastname, firstname, address (3 columns minimum)
            if (parts.Length < 3)
            {
                return null;
            }

            var lastName = parts[0];
            var firstName = parts[1];
            
            // Treat empty strings as null
            string? address = parts.Length > 2 && !string.IsNullOrWhiteSpace(parts[2]) ? parts[2] : null;
            int? color = parts.Length >= 4 ? TryParseInt(parts[3]) : null;

            return new Person
            {
                Id = lineNumber,  // Use line number as ID
                LastName = lastName,
                FirstName = firstName,
                Address = address,
                Color = color
            };
        }

        private int? TryParseInt(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            // Remove non-digit characters
            var trimmed = new string(s.Where(c => char.IsDigit(c) || c == '-').ToArray());
            if (int.TryParse(trimmed, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v)) return v;
            return null;
        }
    }
}
