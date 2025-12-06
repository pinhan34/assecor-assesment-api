namespace assecor_assesment_api.Exceptions
{
    /// <summary>
    /// Exception thrown when a requested person is not found by ID.
    /// </summary>
    public class PersonNotFoundException : Exception
    {
        public int RequestedId { get; }

        public PersonNotFoundException(int id)
            : base($"Person with ID {id} was not found.")
        {
            RequestedId = id;
        }
    }
}
