using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace AI_Knowledge_Generator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Handle unhandled exceptions
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            base.OnStartup(e);
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            LogException(e.Exception);

            MessageBox.Show(
                $"An unexpected error occurred:\n\n{e.Exception.Message}\n\nThe application will continue running.",
                "Unexpected Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);

            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                LogException(ex);

                MessageBox.Show(
                    $"A critical error occurred:\n\n{ex.Message}\n\nThe application will now close.",
                    "Critical Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private static void LogException(Exception ex)
        {
            try
            {
                var logPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "AI Knowledge Generator",
                    "error.log"
                );

                var logDirectory = Path.GetDirectoryName(logPath);
                if (!Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory!);
                }

                var logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {ex}\n\n";
                File.AppendAllText(logPath, logEntry);
            }
            catch
            {
                // If logging fails, don't crash the app
            }
        }
    }
}