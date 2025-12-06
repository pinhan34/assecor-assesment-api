namespace assecor_assesment_api.Exceptions
{
    /// <summary>
    /// Exception thrown when person data validation fails.
    /// </summary>
    public class InvalidPersonDataException : Exception
    {
        public List<string> ValidationErrors { get; }

        public InvalidPersonDataException(string error)
            : base(error)
        {
            ValidationErrors = new List<string> { error };
        }

        public InvalidPersonDataException(List<string> errors)
            : base("Invalid person data: " + string.Join("; ", errors))
        {
            ValidationErrors = errors;
        }
    }
}
