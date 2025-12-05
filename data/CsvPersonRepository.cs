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
            while ((line = await reader.ReadLineAsync()) != null)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var p = ParseLine(line);
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
            while ((line = await reader.ReadLineAsync()) != null)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var p = ParseLine(line);
                if (p != null && p.Id == id) return p;
            }

            return null;
        }

        private Person? ParseLine(string line)
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

            // Expecting at least lastname, firstname, address, id-ish (group/id). We'll be tolerant.
            if (parts.Length < 4)
            {
                // try to salvage lines with 3 columns where last column might be id
                if (parts.Length == 3)
                {
                    // last column might contain both address and id; attempt to split by last space
                    var last = parts[2];
                    var idx = last.LastIndexOf(' ');
                    if (idx > 0)
                    {
                        var addr = last.Substring(0, idx).Trim();
                        var idPart = last.Substring(idx + 1).Trim();
                        var id = TryParseInt(idPart);
                        return new Person
                        {
                            LastName = parts[0],
                            FirstName = parts[1],
                            Address = addr,
                            Id = id ?? 0,
                            Group = null
                        };
                    }
                }
                return null;
            }

            var lastName = parts[0];
            var firstName = parts[1];
            var address = parts[2];
            var idOrGroup = parts[3];

            var idVal = TryParseInt(idOrGroup) ?? 0;
            int? group = null;
            int? color = null;

            // If there's a 4th column, it's the color (ID 1-7)
            color = TryParseInt(parts[3]);
            
            // If there's a 5th column, treat the 4th as color and 5th as group
            if (parts.Length >= 5)
            {
                color = TryParseInt(parts[3]);
                group = TryParseInt(parts[4]);
            }
            else if (parts.Length == 4)
            {
                // 4 columns: lastname, firstname, address, color
                idVal = 0; // No explicit ID provided
            }

            return new Person
            {
                Id = idVal,
                LastName = lastName,
                FirstName = firstName,
                Address = address,
                Group = group,
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
