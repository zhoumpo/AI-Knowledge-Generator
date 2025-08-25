using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
namespace AI_Knowledge_Generator.Services
{
    public class DetectedLanguage : INotifyPropertyChanged
    {
        private bool _isSelected = true;

        public string Name { get; set; } = string.Empty;
        public string[] Extensions { get; set; } = [];
        public string[] CommonIgnorePatterns { get; set; } = [];
        public int FileCount { get; set; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public interface ILanguageDetectionService
    {
        Task<List<DetectedLanguage>> DetectLanguagesAsync(string directoryPath);
    }

    public class LanguageDetectionService : ILanguageDetectionService
    {
        private static readonly Dictionary<string, DetectedLanguage> LanguageDefinitions = new()
        {
            ["JavaScript"] = new DetectedLanguage
            {
                Name = "JavaScript/Node.js",
                Extensions = [".js", ".mjs", ".cjs", ".jsx"],
                CommonIgnorePatterns =
                [
                    "node_modules/",
                    "package-lock.json",
                    "npm-debug.log*",
                    "yarn.lock",
                    "yarn-error.log",
                    ".npm/",
                    "dist/",
                    "build/"
                ]
            },
            ["TypeScript"] = new DetectedLanguage
            {
                Name = "TypeScript",
                Extensions = [".ts", ".tsx"],
                CommonIgnorePatterns =
                [
                    "node_modules/",
                    "dist/",
                    "build/",
                    "*.d.ts",
                    "tsconfig.tsbuildinfo"
                ]
            },
            ["Python"] = new DetectedLanguage
            {
                Name = "Python",
                Extensions = [".py", ".pyw", ".pyx", ".pyi"],
                CommonIgnorePatterns =
                [
                    "__pycache__/",
                    "*.pyc",
                    "*.pyo",
                    "*.pyd",
                    ".venv/",
                    "venv/",
                    "env/",
                    ".pytest_cache/",
                    "*.egg-info/",
                    ".mypy_cache/",
                    "poetry.lock"
                ]
            },
            ["C#"] = new DetectedLanguage
            {
                Name = "C# / .NET",
                Extensions = [".cs", ".csx", ".csproj", ".sln", ".vb", ".fs"],
                CommonIgnorePatterns =
                [
                    "bin/",
                    "obj/",
                    "packages/",
                    "*.suo",
                    "*.user",
                    "*.nupkg",
                    ".vs/"
                ]
            },
            ["Java"] = new DetectedLanguage
            {
                Name = "Java",
                Extensions = [".java", ".jar", ".class"],
                CommonIgnorePatterns =
                [
                    "target/",
                    "*.class",
                    ".gradle/",
                    "build/",
                    "gradle-wrapper.jar",
                    ".mvn/"
                ]
            },
            ["C/C++"] = new DetectedLanguage
            {
                Name = "C/C++",
                Extensions = [".c", ".cpp", ".cxx", ".cc", ".h", ".hpp", ".hxx"],
                CommonIgnorePatterns =
                [
                    "*.o",
                    "*.obj",
                    "*.exe",
                    "*.dll",
                    "*.so",
                    "*.a",
                    "*.lib",
                    "build/",
                    "cmake-build-*/",
                    "CMakeCache.txt",
                    "CMakeFiles/"
                ]
            },
            ["PHP"] = new DetectedLanguage
            {
                Name = "PHP",
                Extensions = [".php", ".phtml", ".php3", ".php4", ".php5"],
                CommonIgnorePatterns =
                [
                    "vendor/",
                    "composer.lock",
                    "*.log"
                ]
            },
            ["Ruby"] = new DetectedLanguage
            {
                Name = "Ruby",
                Extensions = [".rb", ".ruby", ".rake", ".gemspec"],
                CommonIgnorePatterns =
                [
                    "Gemfile.lock",
                    ".bundle/",
                    "vendor/bundle/"
                ]
            },
            ["Go"] = new DetectedLanguage
            {
                Name = "Go",
                Extensions = [".go"],
                CommonIgnorePatterns =
                [
                    "go.sum",
                    "vendor/",
                    "*.exe"
                ]
            },
            ["Rust"] = new DetectedLanguage
            {
                Name = "Rust",
                Extensions = [".rs"],
                CommonIgnorePatterns =
                [
                    "target/",
                    "Cargo.lock",
                    "*.exe"
                ]
            },
            ["Web"] = new DetectedLanguage
            {
                Name = "Web (HTML/CSS)",
                Extensions = [".html", ".htm", ".css", ".scss", ".sass", ".less"],
                CommonIgnorePatterns =
                [
                    "*.min.css",
                    "*.min.js",
                    ".sass-cache/",
                    "node_modules/"
                ]
            },
            ["React"] = new DetectedLanguage
            {
                Name = "React",
                Extensions = [".jsx", ".tsx"],
                CommonIgnorePatterns =
                [
                    "node_modules/",
                    "build/",
                    ".next/",
                    "dist/"
                ]
            },
            ["Vue"] = new DetectedLanguage
            {
                Name = "Vue.js",
                Extensions = [".vue"],
                CommonIgnorePatterns =
                [
                    "node_modules/",
                    "dist/",
                    ".nuxt/"
                ]
            },
            ["Docker"] = new DetectedLanguage
            {
                Name = "Docker",
                Extensions = [".dockerfile"],
                CommonIgnorePatterns =
                [
                    ".dockerignore",
                    "docker-compose.override.yml"
                ]
            },
            ["Database"] = new DetectedLanguage
            {
                Name = "Database",
                Extensions = [".sql", ".db", ".sqlite", ".sqlite3"],
                CommonIgnorePatterns =
                [
                    "*.db",
                    "*.sqlite",
                    "*.sqlite3",
                    "migrations/"
                ]
            }
        };

        // Special file detection (files without extensions or special names)
        private static readonly Dictionary<string, string> SpecialFiles = new()
        {
            ["Dockerfile"] = "Docker",
            ["docker-compose.yml"] = "Docker",
            ["docker-compose.yaml"] = "Docker",
            ["Makefile"] = "C/C++",
            ["CMakeLists.txt"] = "C/C++",
            ["package.json"] = "JavaScript",
            ["tsconfig.json"] = "TypeScript",
            ["requirements.txt"] = "Python",
            ["setup.py"] = "Python",
            ["Pipfile"] = "Python",
            ["pyproject.toml"] = "Python",
            ["Gemfile"] = "Ruby",
            ["Cargo.toml"] = "Rust",
            ["go.mod"] = "Go",
            ["composer.json"] = "PHP",
            ["pom.xml"] = "Java",
            ["build.gradle"] = "Java"
        };

        public async Task<List<DetectedLanguage>> DetectLanguagesAsync(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                return [];

            var languageCounts = new Dictionary<string, int>();

            await Task.Run(() =>
            {
                try
                {
                    var allFiles = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories)
                                           .Select(f => Path.GetRelativePath(directoryPath, f))
                                           .ToList();

                    foreach (var file in allFiles)
                    {
                        var fileName = Path.GetFileName(file);
                        var extension = Path.GetExtension(file).ToLower();

                        // Check special files first
                        if (SpecialFiles.TryGetValue(fileName, out string? language))
                        {
                            languageCounts[language] = languageCounts.GetValueOrDefault(language, 0) + 1;
                            continue;
                        }

                        // Check by extension
                        foreach (var (langKey, langDef) in LanguageDefinitions)
                        {
                            if (langDef.Extensions.Contains(extension))
                            {
                                languageCounts[langKey] = languageCounts.GetValueOrDefault(langKey, 0) + 1;
                                break;
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // Handle any file access issues silently
                }
            });

            // Convert to DetectedLanguage objects with file counts
            var detectedLanguages = languageCounts
                .Where(kvp => kvp.Value > 0)
                .Select(kvp =>
                {
                    var langDef = LanguageDefinitions[kvp.Key];
                    return new DetectedLanguage
                    {
                        Name = langDef.Name,
                        Extensions = langDef.Extensions,
                        CommonIgnorePatterns = langDef.CommonIgnorePatterns,
                        FileCount = kvp.Value,
                        IsSelected = true
                    };
                })
                .OrderByDescending(l => l.FileCount)
                .ToList();

            return detectedLanguages;
        }
    }
}