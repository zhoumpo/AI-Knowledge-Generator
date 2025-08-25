using AI_Knowledge_Generator.Models;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace AI_Knowledge_Generator.Services
{
    public interface ISettingsService
    {
        Task<UserSettings> LoadSettingsAsync();
        Task SaveSettingsAsync(UserSettings settings);
    }

    public class SettingsService : ISettingsService
    {
        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AI Knowledge Generator",
            "settings.json"
        );

        public async Task<UserSettings> LoadSettingsAsync()
        {
            try
            {
                if (!File.Exists(SettingsPath))
                {
                    return new UserSettings();
                }

                var json = await File.ReadAllTextAsync(SettingsPath);
                var settings = JsonSerializer.Deserialize<UserSettings>(json);
                return settings ?? new UserSettings();
            }
            catch (Exception)
            {
                // If settings are corrupted, return defaults
                return new UserSettings();
            }
        }

        public async Task SaveSettingsAsync(UserSettings settings)
        {
            try
            {
                var directory = Path.GetDirectoryName(SettingsPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory!);
                }

                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                await File.WriteAllTextAsync(SettingsPath, json);
            }
            catch (Exception)
            {
                // Silently fail - settings are not critical
                // In a real app, you might want to log this
            }
        }
    }
}