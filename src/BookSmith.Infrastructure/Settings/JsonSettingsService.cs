using System;
using System.IO;
using System.Text.Json;
using BookSmith.Core.Interfaces;
using BookSmith.Core.Models;

namespace BookSmith.Infrastructure.Settings;

public class JsonSettingsService : ISettingsService
{
    private readonly string _filePath;

    public JsonSettingsService(string? customFilePath = null)
    {
        if (!string.IsNullOrWhiteSpace(customFilePath))
        {
            _filePath = customFilePath;
        }
        else
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _filePath = Path.Combine(appData, "BookSmith", "settings.json");
        }
    }

    public AppSettings LoadSettings()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                return new AppSettings();
            }

            string json = File.ReadAllText(_filePath);
            var settings = JsonSerializer.Deserialize<AppSettings>(json);
            return settings ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }

    public void SaveSettings(AppSettings settings)
    {
        if (settings == null)
            throw new ArgumentNullException(nameof(settings));

        try
        {
            string directory = Path.GetDirectoryName(_filePath) ?? string.Empty;
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(settings, options);
            File.WriteAllText(_filePath, json);
        }
        catch
        {
            // Silently handle IO exceptions during save
        }
    }
}
