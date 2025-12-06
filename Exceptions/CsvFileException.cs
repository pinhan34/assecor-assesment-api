namespace assecor_assesment_api.Exceptions
{
    /// <summary>
    /// Exception thrown when there are issues accessing or modifying the CSV file.
    /// </summary>
    public class CsvFileException : Exception
    {
        public string FilePath { get; }
        public CsvFileOperation Operation { get; }

        public CsvFileException(string message, string filePath, CsvFileOperation operation)
            : base(message)
        {
            FilePath = filePath;
            Operation = operation;
        }

        public CsvFileException(string message, string filePath, CsvFileOperation operation, Exception innerException)
            : base(message, innerException)
        {
            FilePath = filePath;
            Operation = operation;
        }
    }

    public enum CsvFileOperation
    {
        Read,
        Write,
        Access
    }
}
