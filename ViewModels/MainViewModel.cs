// Enhanced MainViewModel.cs with language detection

using AI_Knowledge_Generator.Commands;
using AI_Knowledge_Generator.Exceptions;
using AI_Knowledge_Generator.Models;
using AI_Knowledge_Generator.Services;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace AI_Knowledge_Generator.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly IFileAggregationService _fileAggregationService;
        private readonly ISettingsService _settingsService;
        private readonly ILanguageDetectionService _languageDetectionService;

        private string _inputDirectory = "";
        private string _customIgnorePatterns = "# Enter custom patterns to ignore, one per line\n";
        private bool _useDefaultIgnores = true;
        private bool _enableWhitespaceRemoval;
        private string _logOutput = "";
        private bool _isGenerating;
        private bool _isDetectingLanguages;
        private readonly StringBuilder _logBuilder = new();

        public event PropertyChangedEventHandler PropertyChanged;

        // Language detection properties
        public ObservableCollection<DetectedLanguage> DetectedLanguages { get; } = new();

        public string InputDirectory
        {
            get => _inputDirectory;
            set
            {
                if (SetProperty(ref _inputDirectory, value))
                {
                    OnPropertyChanged(nameof(OutputFile));
                    ((RelayCommand)GenerateCommand).RaiseCanExecuteChanged();

                    // Auto-detect languages when directory changes
                    if (!string.IsNullOrWhiteSpace(value) && Directory.Exists(value))
                    {
                        _ = DetectLanguagesAsync();
                    }
                    else
                    {
                        DetectedLanguages.Clear();
                    }
                }
            }
        }

        public string OutputFile
        {
            get => !string.IsNullOrWhiteSpace(InputDirectory)
                   ? Path.Combine(InputDirectory, "codebase.md")
                   : "codebase.md";
        }

        public string CustomIgnorePatterns
        {
            get => _customIgnorePatterns;
            set => SetProperty(ref _customIgnorePatterns, value);
        }

        public bool UseDefaultIgnores
        {
            get => _useDefaultIgnores;
            set
            {
                if (SetProperty(ref _useDefaultIgnores, value))
                {
                    _ = SaveSettingsAsync();
                }
            }
        }

        public bool EnableWhitespaceRemoval
        {
            get => _enableWhitespaceRemoval;
            set
            {
                if (SetProperty(ref _enableWhitespaceRemoval, value))
                {
                    _ = SaveSettingsAsync();
                }
            }
        }

        public string LogOutput
        {
            get => _logOutput;
            private set => SetProperty(ref _logOutput, value);
        }

        public bool IsDetectingLanguages
        {
            get => _isDetectingLanguages;
            private set => SetProperty(ref _isDetectingLanguages, value);
        }

        public ICommand BrowseInputCommand { get; }
        public ICommand GenerateCommand { get; }
        public ICommand RefreshLanguagesCommand { get; }

        public MainViewModel() : this(new FileAggregationService(), new SettingsService(), new LanguageDetectionService())
        {
        }

        public MainViewModel(IFileAggregationService fileAggregationService,
                           ISettingsService settingsService,
                           ILanguageDetectionService languageDetectionService)
        {
            _fileAggregationService = fileAggregationService ?? throw new ArgumentNullException(nameof(fileAggregationService));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
            _languageDetectionService = languageDetectionService ?? throw new ArgumentNullException(nameof(languageDetectionService));

            BrowseInputCommand = new RelayCommand(BrowseInput);
            GenerateCommand = new RelayCommand(
                executeAction: async () => await GenerateMarkdown(),
                canExecute: () => !string.IsNullOrWhiteSpace(InputDirectory) && !_isGenerating
            );
            RefreshLanguagesCommand = new RelayCommand(
                executeAction: async () => await DetectLanguagesAsync(),
                canExecute: () => !string.IsNullOrWhiteSpace(InputDirectory) && !_isDetectingLanguages
            );

            // Load settings on startup
            LoadSettingsAsync();
        }

        private async Task DetectLanguagesAsync()
        {
            if (string.IsNullOrWhiteSpace(InputDirectory) || !Directory.Exists(InputDirectory))
                return;

            try
            {
                IsDetectingLanguages = true;
                AppendLog("Detecting languages in the directory...");

                var languages = await _languageDetectionService.DetectLanguagesAsync(InputDirectory);

                DetectedLanguages.Clear();
                foreach (var language in languages)
                {
                    DetectedLanguages.Add(language);
                }

                if (DetectedLanguages.Any())
                {
                    AppendLog($"Detected {DetectedLanguages.Count} language(s): {string.Join(", ", DetectedLanguages.Select(l => $"{l.Name} ({l.FileCount} files)"))}");
                }
                else
                {
                    AppendLog("No specific languages detected in this directory.");
                }

                ((RelayCommand)RefreshLanguagesCommand).RaiseCanExecuteChanged();
            }
            catch (Exception ex)
            {
                AppendLog($"Error detecting languages: {ex.Message}");
            }
            finally
            {
                IsDetectingLanguages = false;
                ((RelayCommand)RefreshLanguagesCommand).RaiseCanExecuteChanged();
            }
        }

        private async void LoadSettingsAsync()
        {
            try
            {
                var settings = await _settingsService.LoadSettingsAsync();

                if (!string.IsNullOrEmpty(settings.LastInputDirectory) && Directory.Exists(settings.LastInputDirectory))
                {
                    InputDirectory = settings.LastInputDirectory;
                }

                CustomIgnorePatterns = settings.CustomIgnorePatterns;
                UseDefaultIgnores = settings.UseDefaultIgnores;
                EnableWhitespaceRemoval = settings.EnableWhitespaceRemoval;

                AppendLog("Settings loaded successfully.");
            }
            catch (Exception ex)
            {
                AppendLog($"Failed to load settings: {ex.Message}");
            }
        }

        public async Task SaveSettingsAsync()
        {
            try
            {
                var settings = new UserSettings
                {
                    LastInputDirectory = InputDirectory,
                    CustomIgnorePatterns = CustomIgnorePatterns,
                    UseDefaultIgnores = UseDefaultIgnores,
                    EnableWhitespaceRemoval = EnableWhitespaceRemoval
                };

                await _settingsService.SaveSettingsAsync(settings);
            }
            catch (Exception ex)
            {
                AppendLog($"Failed to save settings: {ex.Message}");
            }
        }

        private void BrowseInput()
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog
            {
                Title = "Select Input Directory"
            };

            if (!string.IsNullOrEmpty(InputDirectory) && Directory.Exists(InputDirectory))
            {
                dialog.InitialDirectory = InputDirectory;
            }

            if (dialog.ShowDialog() == true)
            {
                InputDirectory = dialog.FolderName;
                _ = SaveSettingsAsync();
            }
        }

        private async Task GenerateMarkdown()
        {
            try
            {
                _isGenerating = true;
                ((RelayCommand)GenerateCommand).RaiseCanExecuteChanged();

                _logBuilder.Clear();
                LogOutput = "";
                AppendLog("Starting file aggregation...");

                await SaveSettingsAsync();

                var progress = new Progress<string>(message => AppendLog(message));

                // Combine all ignore patterns
                var allIgnorePatterns = GetCombinedIgnorePatterns();
                AppendLog($"Using {allIgnorePatterns.Count} ignore patterns total.");

                var settings = new FileAggregationSettings
                {
                    InputDirectory = InputDirectory,
                    OutputFile = OutputFile,
                    IgnorePatterns = allIgnorePatterns,
                    UseDefaultIgnores = UseDefaultIgnores,
                    EnableWhitespaceRemoval = EnableWhitespaceRemoval
                };

                await _fileAggregationService.AggregateFilesAsync(settings, progress);
                AppendLog("Process completed successfully!");
            }
            catch (FileAggregationException ex)
            {
                AppendLog($"Aggregation Error: {ex.Message}");
                if (!string.IsNullOrEmpty(ex.FilePath))
                {
                    AppendLog($"Problem file: {ex.FilePath}");
                }

                MessageBox.Show($"File aggregation failed: {ex.Message}", "Aggregation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                AppendLog($"Unexpected Error: {ex.Message}");
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _isGenerating = false;
                ((RelayCommand)GenerateCommand).RaiseCanExecuteChanged();
            }
        }

        private System.Collections.Generic.List<string> GetCombinedIgnorePatterns()
        {
            var patterns = new System.Collections.Generic.List<string>();

            // Add custom patterns from text box
            var customPatterns = CustomIgnorePatterns
                .Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries)
                .Where(line => !string.IsNullOrWhiteSpace(line) && !line.TrimStart().StartsWith('#'))
                .Select(line => line.Trim())
                .ToList();

            patterns.AddRange(customPatterns);

            // Add patterns from selected languages
            foreach (var language in DetectedLanguages.Where(l => l.IsSelected))
            {
                patterns.AddRange(language.CommonIgnorePatterns);
                AppendLog($"Including {language.CommonIgnorePatterns.Length} ignore patterns for {language.Name}");
            }

            return patterns.Distinct().ToList();
        }

        private void AppendLog(string message)
        {
            var logMessage = $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}";
            _logBuilder.Append(logMessage);
            LogOutput = _logBuilder.ToString();
        }
    }
}