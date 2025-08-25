// Updated Views/MainWindow.xaml.cs

using AI_Knowledge_Generator.Models;
using AI_Knowledge_Generator.Services;
using AI_Knowledge_Generator.ViewModels;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace AI_Knowledge_Generator.Views
{
    public partial class MainWindow : Window
    {
        private readonly ISettingsService _settingsService;
        private MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            _settingsService = new SettingsService();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;

            // Subscribe to language detection changes to save preferences
            _viewModel.DetectedLanguages.CollectionChanged += DetectedLanguages_CollectionChanged;

            LoadWindowSettings();
            Closing += MainWindow_Closing;
        }

        private void DetectedLanguages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // When languages are added, try to restore their previous selection state
            if (e.NewItems != null)
            {
                _ = RestoreLanguagePreferences();
            }
        }

        private async System.Threading.Tasks.Task RestoreLanguagePreferences()
        {
            try
            {
                var settings = await _settingsService.LoadSettingsAsync();

                foreach (var language in _viewModel.DetectedLanguages)
                {
                    if (settings.LanguagePreferences.ContainsKey(language.Name))
                    {
                        language.IsSelected = settings.LanguagePreferences[language.Name];
                    }

                    // Subscribe to property changes to save preferences
                    language.PropertyChanged += Language_PropertyChanged;
                }
            }
            catch (Exception)
            {
                // Ignore errors when restoring preferences
            }
        }

        private void Language_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DetectedLanguage.IsSelected))
            {
                // Auto-save language preferences when changed
                _ = SaveLanguagePreferences();
            }
        }

        private async System.Threading.Tasks.Task SaveLanguagePreferences()
        {
            try
            {
                var settings = await _settingsService.LoadSettingsAsync();

                // Update language preferences
                settings.LanguagePreferences.Clear();
                foreach (var language in _viewModel.DetectedLanguages)
                {
                    settings.LanguagePreferences[language.Name] = language.IsSelected;
                }

                await _settingsService.SaveSettingsAsync(settings);
            }
            catch (Exception)
            {
                // Ignore errors when saving preferences
            }
        }

        private async void LoadWindowSettings()
        {
            try
            {
                var settings = await _settingsService.LoadSettingsAsync();

                if (settings.WindowWidth > 0 && settings.WindowHeight > 0)
                {
                    Width = settings.WindowWidth;
                    Height = settings.WindowHeight;
                }

                if (settings.WindowWidth <= 0)
                {
                    WindowStartupLocation = WindowStartupLocation.CenterScreen;
                }
            }
            catch (Exception)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
        }

        private async void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                // Save current settings including window size and language preferences
                await _viewModel.SaveSettingsAsync();
                await SaveLanguagePreferences();

                var settings = await _settingsService.LoadSettingsAsync();
                settings.WindowWidth = (int)ActualWidth;
                settings.WindowHeight = (int)ActualHeight;

                await _settingsService.SaveSettingsAsync(settings);
            }
            catch (Exception)
            {
                // Don't prevent closing if save fails
            }
        }
    }
}