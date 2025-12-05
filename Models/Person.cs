namespace assecor_assesment_api.Models
{
    public class Person
    {
        // Using string properties to match the CSV content and avoid parsing failures for malformed rows
        public int Id { get; set; }
        public string? LastName { get; set; }
        public string? FirstName { get; set; }
        public string? Address { get; set; }
        public int? Group { get; set; }
    }
}
