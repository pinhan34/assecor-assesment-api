namespace assecor_assesment_api.Models
{
    public class PersonResponseDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? LastName { get; set; }
        public string? ZipCode { get; set; }
        public string? City { get; set; }
        public string? Color { get; set; }

        public static PersonResponseDto FromPerson(Person person)
        {
            // Extract zipcode and city from address (e.g., "67742 Lauterecken")
            string? zipCode = null;
            string? city = null;

            if (!string.IsNullOrWhiteSpace(person.Address))
            {
                var addressParts = person.Address.Split(' ', 2, System.StringSplitOptions.RemoveEmptyEntries);
                if (addressParts.Length > 0)
                {
                    zipCode = addressParts[0];
                    if (addressParts.Length > 1)
                    {
                        city = addressParts[1];
                    }
                }
            }

            // Convert color number to color name
            string? colorName = person.Color.HasValue ? GetColorName(person.Color.Value) : null;

            return new PersonResponseDto
            {
                Id = person.Id,
                Name = person.FirstName,
                LastName = person.LastName,
                ZipCode = zipCode,
                City = city,
                Color = colorName
            };
        }

        private static string GetColorName(int colorId)
        {
            return colorId switch
            {
                1 => "blau",
                2 => "grün",
                3 => "violett",
                4 => "rot",
                5 => "gelb",
                6 => "türkis",
                7 => "weiß",
                _ => "unbekannt"
            };
        }
    }
}
