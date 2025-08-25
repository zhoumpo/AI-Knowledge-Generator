namespace AI_Knowledge_Generator.Models
{
    public class FileAggregationSettings
    {
        public string InputDirectory { get; set; } = string.Empty;
        public string OutputFile { get; set; } = string.Empty;
        public List<string> IgnorePatterns { get; set; } = new();
        public bool UseDefaultIgnores { get; set; } = true;
        public bool EnableWhitespaceRemoval { get; set; } = false;
    }
}