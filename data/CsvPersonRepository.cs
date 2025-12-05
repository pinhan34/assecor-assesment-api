using assecor_assesment_api.Models;
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

        public async Task<IEnumerable<Person>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            var list = new List<Person>();
            if (!File.Exists(_filePath)) return list;

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

            return list;
        }

        public async Task<Person?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(_filePath)) return null;

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

            return null;
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
                Group = null,
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
