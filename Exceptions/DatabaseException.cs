namespace assecor_assesment_api.Exceptions
{
    /// <summary>
    /// Exception thrown when there are issues with database operations.
    /// </summary>
    public class DatabaseException : Exception
    {
        public DatabaseOperation Operation { get; }

        public DatabaseException(string message, DatabaseOperation operation)
            : base(message)
        {
            Operation = operation;
        }

        public DatabaseException(string message, DatabaseOperation operation, Exception innerException)
            : base(message, innerException)
        {
            Operation = operation;
        }
    }

    public enum DatabaseOperation
    {
        Read,
        Write,
        Delete,
        Connection
    }
}
