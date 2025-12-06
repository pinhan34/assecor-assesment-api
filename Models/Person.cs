namespace assecor_assesment_api.Models
{
    public class Person
    {
        // Using string properties to match the CSV content and avoid parsing failures for malformed rows
        public int Id { get; set; }
        public string? LastName { get; set; }
        public string? FirstName { get; set; }
        public string? Address { get; set; }
        public int? Color { get; set; }  // Color ID (1-7), nullable if not provided
    }
}
