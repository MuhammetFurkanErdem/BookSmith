using System.Collections.Generic;
using BookSmith.Core.Models;

namespace BookSmith.Core.Interfaces;

/// <summary>
/// Interface for managing built-in and custom user cleaning presets.
/// </summary>
public interface IPresetManager
{
    /// <summary>Returns all built-in and user-saved presets.</summary>
    IReadOnlyList<CleaningPreset> GetAllPresets();

    /// <summary>Returns a preset by ID, or null if not found.</summary>
    CleaningPreset? GetPresetById(string id);

    /// <summary>Saves a new custom preset or updates an existing one.</summary>
    void SavePreset(CleaningPreset preset);

    /// <summary>Deletes a custom user preset. Returns false if built-in or not found.</summary>
    bool DeletePreset(string id);
}
