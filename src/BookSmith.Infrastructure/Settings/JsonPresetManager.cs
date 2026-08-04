using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using BookSmith.Core.Interfaces;
using BookSmith.Core.Models;

namespace BookSmith.Infrastructure.Settings;

public class JsonPresetManager : IPresetManager
{
    private readonly string _presetsFilePath;
    private readonly List<CleaningPreset> _builtInPresets;

    public JsonPresetManager(string? customPath = null)
    {
        if (!string.IsNullOrEmpty(customPath))
        {
            _presetsFilePath = customPath;
        }
        else
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string folder = Path.Combine(appData, "BookSmith");
            Directory.CreateDirectory(folder);
            _presetsFilePath = Path.Combine(folder, "presets.json");
        }

        _builtInPresets = InitializeBuiltInPresets();
    }

    private static List<CleaningPreset> InitializeBuiltInPresets()
    {
        return new List<CleaningPreset>
        {
            new CleaningPreset
            {
                Id = "preset-default",
                Name = "Default Rules",
                Description = "Standard cleaning rules suitable for most fiction and non-fiction books.",
                IsBuiltIn = true,
                RemoveHeaders = true,
                RemoveFooters = true,
                RemovePageNumbers = true,
                FixBrokenWords = true,
                MergeWrappedLines = true,
                SmartDialogueFormatting = true,
                ElevenReaderMode = false,
                ExportEpub = false,
                RemoveFrontMatter = true
            },
            new CleaningPreset
            {
                Id = "preset-elevenreader",
                Name = "ElevenReader Fiction",
                Description = "Optimized for ElevenReader TTS app narration with smart dialogue & front-matter filtering.",
                IsBuiltIn = true,
                RemoveHeaders = true,
                RemoveFooters = true,
                RemovePageNumbers = true,
                FixBrokenWords = true,
                MergeWrappedLines = true,
                SmartDialogueFormatting = true,
                ElevenReaderMode = true,
                ExportEpub = false,
                RemoveFrontMatter = true
            },
            new CleaningPreset
            {
                Id = "preset-academic",
                Name = "Academic Clean",
                Description = "Aggressive header/footer & front-matter removal for papers and textbooks.",
                IsBuiltIn = true,
                RemoveHeaders = true,
                RemoveFooters = true,
                RemovePageNumbers = true,
                FixBrokenWords = true,
                MergeWrappedLines = true,
                SmartDialogueFormatting = false,
                ElevenReaderMode = false,
                ExportEpub = false,
                RemoveFrontMatter = true
            }
        };
    }

    public IReadOnlyList<CleaningPreset> GetAllPresets()
    {
        var userPresets = LoadUserPresets();
        var all = new List<CleaningPreset>(_builtInPresets);
        all.AddRange(userPresets);
        return all.AsReadOnly();
    }

    public CleaningPreset? GetPresetById(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;
        return GetAllPresets().FirstOrDefault(p => p.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
    }

    public void SavePreset(CleaningPreset preset)
    {
        if (preset == null) throw new ArgumentNullException(nameof(preset));

        // Cannot overwrite built-in presets
        if (preset.IsBuiltIn)
            throw new InvalidOperationException("Cannot modify a built-in preset.");

        var userPresets = LoadUserPresets();
        int existingIndex = userPresets.FindIndex(p => p.Id.Equals(preset.Id, StringComparison.OrdinalIgnoreCase));

        if (existingIndex >= 0)
        {
            userPresets[existingIndex] = preset;
        }
        else
        {
            userPresets.Add(preset);
        }

        SaveUserPresets(userPresets);
    }

    public bool DeletePreset(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return false;

        // Cannot delete built-in presets
        if (_builtInPresets.Any(p => p.Id.Equals(id, StringComparison.OrdinalIgnoreCase)))
            return false;

        var userPresets = LoadUserPresets();
        int removedCount = userPresets.RemoveAll(p => p.Id.Equals(id, StringComparison.OrdinalIgnoreCase));

        if (removedCount > 0)
        {
            SaveUserPresets(userPresets);
            return true;
        }

        return false;
    }

    private List<CleaningPreset> LoadUserPresets()
    {
        if (!File.Exists(_presetsFilePath))
            return new List<CleaningPreset>();

        try
        {
            string json = File.ReadAllText(_presetsFilePath);
            return JsonSerializer.Deserialize<List<CleaningPreset>>(json) ?? new List<CleaningPreset>();
        }
        catch
        {
            return new List<CleaningPreset>();
        }
    }

    private void SaveUserPresets(List<CleaningPreset> userPresets)
    {
        try
        {
            string json = JsonSerializer.Serialize(userPresets, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_presetsFilePath, json);
        }
        catch
        {
            // Ignore write errors in fallback scenarios
        }
    }
}
