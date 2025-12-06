namespace assecor_assesment_api.Exceptions
{
    /// <summary>
    /// Exception thrown when data validation for new person insertion fails.
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
