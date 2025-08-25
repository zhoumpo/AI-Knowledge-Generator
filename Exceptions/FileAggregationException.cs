using System;

namespace AI_Knowledge_Generator.Exceptions
{
    public class FileAggregationException : Exception
    {
        public string? FilePath { get; }

        public FileAggregationException(string message) : base(message)
        {
        }

        public FileAggregationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public FileAggregationException(string message, string filePath) : base(message)
        {
            FilePath = filePath;
        }

        public FileAggregationException(string message, string filePath, Exception innerException) : base(message, innerException)
        {
            FilePath = filePath;
        }
    }
}