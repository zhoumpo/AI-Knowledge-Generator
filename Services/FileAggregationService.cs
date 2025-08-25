using AI_Knowledge_Generator.Exceptions;
using AI_Knowledge_Generator.Models;
using AI_Knowledge_Generator.Utils;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace AI_Knowledge_Generator.Services
{
    public interface IFileAggregationService
    {
        Task AggregateFilesAsync(FileAggregationSettings settings, IProgress<string> progress);
    }

    public class FileAggregationService : IFileAggregationService
    {
        private const int MaxFileSize = 10 * 1024 * 1024; // 10MB

        public async Task AggregateFilesAsync(FileAggregationSettings settings, IProgress<string> progress)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(settings.InputDirectory))
                throw new FileAggregationException("Input directory cannot be empty.");

            if (!Directory.Exists(settings.InputDirectory))
                throw new FileAggregationException($"Input directory does not exist: {settings.InputDirectory}");

            if (string.IsNullOrWhiteSpace(settings.OutputFile))
                throw new FileAggregationException("Output file path cannot be empty.");

            try
            {
                var ignorePatterns = new HashSet<string>();

                if (settings.UseDefaultIgnores)
                {
                    foreach (var pattern in FileUtils.DefaultIgnores)
                    {
                        ignorePatterns.Add(pattern);
                    }
                    progress.Report("Using default ignore patterns.");
                }

                foreach (var pattern in settings.IgnorePatterns)
                {
                    ignorePatterns.Add(pattern);
                }

                var allFiles = Directory.GetFiles(settings.InputDirectory, "*.*", SearchOption.AllDirectories)
                                     .Select(f => Path.GetRelativePath(settings.InputDirectory, f))
                                     .ToList();

                progress.Report($"Found {allFiles.Count} files in {settings.InputDirectory}. Applying filters...");

                var output = new StringBuilder();
                int includedCount = 0;
                int defaultIgnoredCount = 0;
                int binaryAndSvgFileCount = 0;
                var includedFiles = new List<string>();
                var errorFiles = new List<string>();

                foreach (var file in allFiles.OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
                {
                    var fullPath = Path.Combine(settings.InputDirectory, file);

                    try
                    {
                        if (ShouldIgnoreFile(file, ignorePatterns, settings.OutputFile))
                        {
                            defaultIgnoredCount++;
                            continue;
                        }

                        if (!FileUtils.IsBinaryFile(fullPath) && !FileUtils.ShouldTreatAsBinary(fullPath))
                        {
                            var content = await File.ReadAllTextAsync(fullPath);
                            var extension = Path.GetExtension(file);

                            content = FileUtils.EscapeTripleBackticks(content);

                            if (settings.EnableWhitespaceRemoval && !FileUtils.WhitespaceDependentExtensions.Contains(extension))
                            {
                                content = FileUtils.RemoveWhitespace(content);
                            }

                            output.AppendLine($"# {file}\n");
                            output.AppendLine($"```{extension.TrimStart('.')}\n{content}\n```\n");

                            includedCount++;
                            includedFiles.Add(file);
                        }
                        else
                        {
                            var fileType = FileUtils.GetFileType(fullPath);
                            output.AppendLine($"# {file}\n");
                            output.AppendLine($"This is a {(fileType == "SVG Image" ? "file" : "binary file")} of the type: {fileType}\n");

                            binaryAndSvgFileCount++;
                            includedCount++;
                            includedFiles.Add(file);
                        }
                    }
                    catch (Exception ex)
                    {
                        errorFiles.Add(file);
                        progress.Report($"Warning: Could not process file '{file}': {ex.Message}");
                        // Continue processing other files
                    }
                }

                // Ensure output directory exists
                var outputDirectory = Path.GetDirectoryName(settings.OutputFile);
                if (!string.IsNullOrEmpty(outputDirectory) && !Directory.Exists(outputDirectory))
                {
                    Directory.CreateDirectory(outputDirectory);
                }

                await File.WriteAllTextAsync(settings.OutputFile, output.ToString());

                var fileInfo = new FileInfo(settings.OutputFile);

                progress.Report($"Files aggregated successfully into {settings.OutputFile}");
                progress.Report($"Total files found: {allFiles.Count}");
                progress.Report($"Files included in output: {includedCount}");
                progress.Report($"Binary and SVG files included: {binaryAndSvgFileCount}");

                if (errorFiles.Count > 0)
                {
                    progress.Report($"Files with errors (skipped): {errorFiles.Count}");
                }

                if (fileInfo.Length > MaxFileSize)
                {
                    progress.Report($"Warning: Output file size ({fileInfo.Length / 1024.0 / 1024.0:F2} MB) exceeds 10 MB.");
                    progress.Report("Consider adding more files to .aidigestignore to reduce the output size.");
                }

                progress.Report($"Done! Wrote code base to {settings.OutputFile}");
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new FileAggregationException($"Access denied: {ex.Message}", ex);
            }
            catch (DirectoryNotFoundException ex)
            {
                throw new FileAggregationException($"Directory not found: {ex.Message}", ex);
            }
            catch (IOException ex)
            {
                throw new FileAggregationException($"IO error: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new FileAggregationException($"Unexpected error during file aggregation: {ex.Message}", ex);
            }
        }

        private static bool ShouldIgnoreFile(string filePath, HashSet<string> ignorePatterns, string outputFile)
        {
            if (Path.GetFileName(outputFile).Equals(Path.GetFileName(filePath), StringComparison.OrdinalIgnoreCase))
                return true;

            return ignorePatterns.Any(pattern =>
            {
                if (pattern.Contains('*'))
                {
                    // Convert the glob pattern to a regex pattern
                    string regexPattern = "^" + Regex.Escape(pattern)
                                                  .Replace("\\*", ".*")
                                                  .Replace("\\?", ".")
                                             + "$";

                    return Regex.IsMatch(filePath, regexPattern, RegexOptions.IgnoreCase);
                }
                return filePath.Contains(pattern, StringComparison.OrdinalIgnoreCase);
            });
        }
    }
}