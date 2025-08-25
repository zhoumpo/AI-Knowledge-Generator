using System.IO;
using System.Text.RegularExpressions;


namespace AI_Knowledge_Generator.Utils
{
    public static class FileUtils
    {
        public static readonly string[] WhitespaceDependentExtensions =
        [
            ".py",   // Python
            ".yaml", // YAML
            ".yml",  // YAML
            ".jade", // Jade/Pug
            ".haml", // Haml
            ".slim", // Slim
            ".coffee", // CoffeeScript
            ".pug",  // Pug
            ".styl", // Stylus
            ".gd",   // Godot
        ];

        public static readonly string[] DefaultIgnores =
        [
            ".aidigestignore",
            // Node.js
            "node_modules",
            "package-lock.json",
            "npm-debug.log",
            // Yarn
            "yarn.lock",
            "yarn-error.log",
            // pnpm
            "pnpm-lock.yaml",
            // Bun
            "bun.lockb",
            // Deno
            "deno.lock",
            // PHP (Composer)
            "vendor",
            "composer.lock",
            // Python
            "__pycache__",
            "*.pyc",
            "*.pyo",
            "*.pyd",
            ".Python",
            "pip-log.txt",
            "pip-delete-this-directory.txt",
            ".venv",
            "venv",
            "ENV",
            "env",
            ".pytest_cache",
            "migrations",
            // Godot
            ".godot",
            "*.import",
            // Ruby
            "Gemfile.lock",
            ".bundle",
            // Java
            "target",
            "*.class",
            // Gradle
            ".gradle",
            "build",
            // Maven
            "pom.xml.tag",
            "pom.xml.releaseBackup",
            "pom.xml.versionsBackup",
            "pom.xml.next",
            // .NET
            "bin",
            "obj",
            "*.suo",
            "*.user",
            // Go
            "go.sum",
            // Rust
            "Cargo.lock",
            "target",
            // General
            ".git",
            ".svn",
            ".hg",
            ".DS_Store",
            "Thumbs.db",
            // Environment variables
            ".env",
            ".env.local",
            ".env.development.local",
            ".env.test.local",
            ".env.production.local",
            "*.env",
            "*.env.*",
            // Common framework directories
            ".svelte-kit",
            ".next",
            ".nuxt",
            ".vuepress",
            ".cache",
            "dist",
            "tmp",
            // Our output file
            "codebase.md",
            // Turborepo cache folder
            ".turbo",
            ".vercel",
            ".netlify",
            "LICENSE",
            // Certificates
            ".pem",
            ".cer",
            ".crt",
            ".key",
            ".p12",
            ".pfx",
            // Images
            ".jpg",
            ".jpeg",
            ".png",
            ".gif",
            ".bmp",
            ".webp",
            ".svg",
            // Videos
            ".mp4",
            ".webm",
            ".ogg",
            ".ogv",
            ".avi",
            ".mov",
            ".flv",
            ".mkv",
            // Azure
            "azure-pipelines.yml",
            //
            "static",
            "statics",
            "staticfiles",
            ".mypy_cache",
            "poetry.lock"
        ];

        public static string RemoveWhitespace(string text)
        {
            return Regex.Replace(text, @"\s+", " ").Trim();
        }

        public static string EscapeTripleBackticks(string content)
        {
            return content.Replace("```", "\\`\\`\\`");
        }

        public static bool IsBinaryFile(string filePath)
        {
            try
            {
                using var stream = File.OpenRead(filePath);
                byte[] buffer = new byte[Math.Min(stream.Length, 8192)];
                int bytesRead = stream.Read(buffer, 0, buffer.Length);

                for (int i = 0; i < bytesRead; i++)
                {
                    if (buffer[i] == 0)
                        return true;
                }
                return false;
            }
            catch
            {
                return true;
            }
        }

        public static string GetFileType(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            return extension switch
            {
                ".jpg" or ".jpeg" or ".png" or ".gif" or ".bmp" or ".webp" => "Image",
                ".svg" => "SVG Image",
                ".wasm" => "WebAssembly",
                ".pdf" => "PDF",
                ".doc" or ".docx" => "Word Document",
                ".xls" or ".xlsx" => "Excel Spreadsheet",
                ".ppt" or ".pptx" => "PowerPoint Presentation",
                ".zip" or ".rar" or ".7z" => "Compressed Archive",
                ".exe" => "Executable",
                ".dll" => "Dynamic-link Library",
                ".so" => "Shared Object",
                ".dylib" => "Dynamic Library",
                ".pem" or ".cer" or ".crt" or ".key" or ".p12" or ".pfx" => "Certificate",
                _ => "Binary"
            };
        }

        public static bool ShouldTreatAsBinary(string filePath)
        {
            return Path.GetExtension(filePath).Equals(".svg", StringComparison.CurrentCultureIgnoreCase) ||
                   GetFileType(filePath) != "Binary";
        }
    }
}
