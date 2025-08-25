// Updated Models/UserSettings.cs

using System.Collections.Generic;

namespace AI_Knowledge_Generator.Models
{
    public class UserSettings
    {
        public string LastInputDirectory { get; set; } = string.Empty;
        public string CustomIgnorePatterns { get; set; } = "# Enter custom patterns to ignore, one per line\n";
        public bool UseDefaultIgnores { get; set; } = true;
        public bool EnableWhitespaceRemoval { get; set; } = false;
        public int WindowWidth { get; set; } = 900;
        public int WindowHeight { get; set; } = 800;

        // Language preferences
        public Dictionary<string, bool> LanguagePreferences { get; set; } = new();
    }
}